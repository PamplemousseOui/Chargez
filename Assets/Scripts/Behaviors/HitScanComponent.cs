using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HitScanComponent : MonoBehaviour
{
    [Header("If no object specified, target = player")]
    public GameObject target;
    public Gradient loadingColor;
    public Gradient shootingColor;
    public float baseShootDuration;
    public float baseLoadingTime;
    public float baseStopRotationBeforeShootTime;
    public float baseShootMinRate;
    public float baseShootMaxRate;
    public float baseShootLength;
    public float baseDamage;
    public float baseLaserWeight;
    public bool freezeOnPlayerHit;
    public float loadingLineWeight = 0.1f;
    public float shootingLineWeight = 0.5f;

    [SerializeField] private LineRenderer m_lineRenderer; 
    public bool isShooting { get; private set; }
    public SimpleLookAt lookAtComponent;
    public Transform rayTransform;

    public Action OnLoadingStart;
    public Action OnLoadingEnd;
    public Action<float> OnLoadingRatioUpdate; //current ratio
    public Action OnShootStart;
    public Action OnShootEnd;

    private float m_curShootDuration;
    private float m_curLoadingTime;
    private float m_curStopRotationBeforeShootTime;
    private float m_curRate;
    private float m_curRateTimerValue;
    private float m_curLoadingTimerValue;
    private float m_curshootDurationTimerValue;
    private float m_curShootLength;
    private float m_curDamage;
    private float m_currLaserWeight;
    
    private bool m_isLoading;
    private bool m_canHitPlayer;

    private Vector2 m_hitPoint;

    private void Start()
    {
        if (target == null)
            target = GameManager.player.gameObject;
        ReinitShoot();
    }

    private void Update()
    {
        if (!GameManager.gameIsPaused && GameManager.player.healthComponent.isAlive && (GameManager.canUpdateEnemies && freezeOnPlayerHit))
        {
            if (!freezeOnPlayerHit || GameManager.canUpdateEnemies)
            {
                if (m_curRateTimerValue <= m_curRate)
                {
                    m_curRateTimerValue += Time.deltaTime;
                    if (m_curRateTimerValue > m_curRate)
                    {
                        StartLoading();
                    }
                }

                UpdateRaycast();
                UpdateLoading();
                UpdateShoot();
            }
        }
        UpdateLine();
    }
    private void StartLoading()
    {
        m_isLoading = true;
        OnLoadingStart?.Invoke();
        m_curShootLength = baseShootLength;

        m_curLoadingTimerValue = 0;
        m_curLoadingTime = baseLoadingTime;
        m_curStopRotationBeforeShootTime = baseStopRotationBeforeShootTime;
        
        m_lineRenderer.startWidth = loadingLineWeight;
        m_lineRenderer.endWidth = loadingLineWeight;
        m_lineRenderer.colorGradient = loadingColor;
    }

    private void UpdateLoading()
    {
        if (m_isLoading)
        {
            m_curLoadingTimerValue += Time.deltaTime;
            OnLoadingRatioUpdate?.Invoke(m_curLoadingTimerValue / m_curLoadingTime);
            if (m_curLoadingTimerValue > m_curLoadingTime - m_curStopRotationBeforeShootTime && lookAtComponent.canRotate)
            {
                GetComponent<Animator>().SetTrigger("Attack");
                lookAtComponent.SetCanRotate(false);
            }
                
            if (m_curLoadingTimerValue > m_curLoadingTime)
            {
                StopLoading();
                StartShoot();
            }
        }
    }

    private void StopLoading()
    {
        m_isLoading = false;
        OnLoadingEnd?.Invoke();
    }

    private void StartShoot()
    {
        isShooting = true;
        m_canHitPlayer = true;
        m_curshootDurationTimerValue = 0; 
        m_curShootDuration = baseShootDuration;
        OnShootStart?.Invoke();
        
        m_lineRenderer.startWidth = shootingLineWeight;
        m_lineRenderer.endWidth = shootingLineWeight;
        m_lineRenderer.colorGradient = shootingColor;
    }

    private void UpdateShoot()
    {
        if (isShooting)
        {
            m_curshootDurationTimerValue += Time.deltaTime;
            if (m_curshootDurationTimerValue > m_curShootDuration)
            {
                StopShoot();
            }
        }
    }

    private void StopShoot()
    {
        isShooting = false;
        OnShootEnd?.Invoke();
        ReinitShoot();
    }

    private void ReinitShoot()
    {
        m_curRateTimerValue = 0;
        m_curRate = UnityEngine.Random.Range(baseShootMinRate, baseShootMaxRate);
        lookAtComponent.SetCanRotate(true);
    }

    private void UpdateRaycast()
    {
        if (isShooting || m_isLoading)
        {
            var position = transform.position;
            var up = transform.up;
            LayerMask mask = LayerMask.GetMask(new []{"Player", "Default", "Wall", "Shield"});
            RaycastHit2D hit = Physics2D.Raycast(position + up, up, m_curShootLength, mask);
            RaycastHit2D hitLeft = Physics2D.Raycast(position + up + transform.right * m_currLaserWeight  / 2.0f, up, m_curShootLength, mask);
            RaycastHit2D hitRight = Physics2D.Raycast(position + up - transform.right * m_currLaserWeight  / 2.0f, up, m_curShootLength, mask);

            Collider2D other = null;
            if (hit.collider && hit.transform.gameObject.CompareTag(Tag.Player.ToString())) other = hit.collider;
            else if (hitLeft.collider && hitLeft.transform.gameObject.CompareTag(Tag.Player.ToString())) other = hitLeft.collider;
            else if (hitRight.collider && hitRight.transform.gameObject.CompareTag(Tag.Player.ToString())) other = hitRight.collider;
            
            if(other)
            {
                if (m_canHitPlayer && isShooting && other.gameObject.TryGetComponent(out HealthComponent healthComponent))
                {
                    Debug.Log($"hit object {hit.transform.gameObject}");
                    m_curDamage = baseDamage;
                    healthComponent.ApplyDamage(m_curDamage);
                    m_canHitPlayer = false;
                }
                //StopShoot();
            }
        }
    }

    private void UpdateLine()
    {
        if (!isShooting && !m_isLoading)
        {
            m_lineRenderer.enabled = false;
            return;
        }
        
        var position = transform.position;
        var up = transform.up;
        m_hitPoint = Vector2.zero;
        LayerMask mask = LayerMask.GetMask(new []{"Wall", "Shield"});
        RaycastHit2D hit = Physics2D.Raycast(position + up, up, m_curShootLength, mask);
        if (!hit.collider)
        {
            m_lineRenderer.enabled = false;
            return;
        }

        m_hitPoint = hit.point + (Vector2)up * 0.2f;
        
        Vector3[] points = {m_lineRenderer.transform.position, m_hitPoint};
        if (m_isLoading)
        {
            m_lineRenderer.enabled = true;
            m_lineRenderer.SetPositions(points);
        }
        else if (isShooting)
        {
            m_lineRenderer.enabled = true;
            m_lineRenderer.SetPositions(points);
        }
        else
        {
            m_lineRenderer.enabled = false;
        }
    }
}
