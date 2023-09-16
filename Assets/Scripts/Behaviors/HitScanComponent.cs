using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HitScanComponent : MonoBehaviour
{
    [Header("If no object specified, target = player")]
    public GameObject target;
    public Material loadingColor;
    public Material shootingColor;
    public float shootDuration = 1f;
    public float loadingTime = 2f;
    public float stopRotationBeforeShootTime = 0.28f;
    public float shootMinRate = 1f;
    public float shootMaxRate = 2f;
    public float shootLength = 2000f;
    public float damage = 1f;
    public float laserWeight = 0.5f;
    public bool freezeOnPlayerHit;
    public bool canHitPlayerWhileDashing = false;
    public float loadingLineWeight = 0.8f;
    public float shootingLineWeight = 0.8f;

    [SerializeField] private LineRenderer m_lineRenderer; 
    public bool isShooting { get; private set; }
    public SimpleLookAt lookAtComponent;
    public Transform rayTransform;

    public Action OnLoadingStart;
    public Action OnLoadingEnd;
    public Action OnLoadingEnding; //at most 1s before the end
    public Action<float> OnLoadingRatioUpdate; //current ratio
    public Action OnShootStart;
    public Action OnShootEnd;

    private float m_curRate;
    private float m_curRateTimerValue;
    private float m_curLoadingTimerValue;
    private float m_curshootDurationTimerValue;
    private float m_currLaserWeight;
    
    private bool m_isLoading;
    private bool m_canHitPlayer;
    private bool m_isLoadingEnding;

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

        m_curLoadingTimerValue = 0;
        
        m_lineRenderer.startWidth = loadingLineWeight;
        m_lineRenderer.endWidth = loadingLineWeight;
        m_lineRenderer.material = loadingColor;
    }

    private void UpdateLoading()
    {
        if (m_isLoading)
        {
            m_curLoadingTimerValue += Time.deltaTime;
            OnLoadingRatioUpdate?.Invoke(m_curLoadingTimerValue / loadingTime);
            if (m_curLoadingTimerValue > loadingTime - stopRotationBeforeShootTime && lookAtComponent.canRotate)
            {
                GetComponent<Animator>().SetTrigger("Attack");
                lookAtComponent.SetCanRotate(false);
            }

            if (loadingTime - m_curLoadingTimerValue < 1f && !m_isLoadingEnding)
            {
                OnLoadingEnding?.Invoke();
                m_isLoadingEnding = true;
            }

            if (m_curLoadingTimerValue > loadingTime)
            {
                StopLoading();
                StartShoot();
            }
        }
    }

    private void StopLoading()
    {
        m_isLoading = false;
        m_isLoadingEnding = false;
        OnLoadingEnd?.Invoke();
    }

    private void StartShoot()
    {
        isShooting = true;
        m_canHitPlayer = true;
        m_curshootDurationTimerValue = 0; 
        OnShootStart?.Invoke();
        
        m_lineRenderer.startWidth = shootingLineWeight;
        m_lineRenderer.endWidth = shootingLineWeight;
        m_lineRenderer.material = shootingColor;
    }

    private void UpdateShoot()
    {
        if (isShooting)
        {
            m_curshootDurationTimerValue += Time.deltaTime;
            if (m_curshootDurationTimerValue > shootDuration)
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
        m_curRate = UnityEngine.Random.Range(shootMinRate, shootMaxRate);
        lookAtComponent.SetCanRotate(true);
    }

    private void UpdateRaycast()
    {
        if (isShooting || m_isLoading)
        {
            var position = transform.position;
            var up = transform.up;
            LayerMask mask = LayerMask.GetMask(new []{"Player", "Wall", "Shield"});
            RaycastHit2D hit = Physics2D.Raycast(position + up, up, shootLength, mask);
            RaycastHit2D hitLeft = Physics2D.Raycast(position + up + transform.right * m_currLaserWeight  / 2.0f, up, shootLength, mask);
            RaycastHit2D hitRight = Physics2D.Raycast(position + up - transform.right * m_currLaserWeight  / 2.0f, up, shootLength, mask);

            Collider2D other = null;
            if (hit.collider && hit.transform.gameObject.CompareTag(Tag.Player.ToString())) other = hit.collider;
            else if (hitLeft.collider && hitLeft.transform.gameObject.CompareTag(Tag.Player.ToString())) other = hitLeft.collider;
            else if (hitRight.collider && hitRight.transform.gameObject.CompareTag(Tag.Player.ToString())) other = hitRight.collider;
            
            if(other && (!GameManager.player.isDashing  || canHitPlayerWhileDashing))
            {
                if (m_canHitPlayer && isShooting && other.gameObject.TryGetComponent(out HealthComponent healthComponent))
                {
                    Debug.Log($"hit object {hit.transform.gameObject}");
                    damage = damage;
                    healthComponent.ApplyDamage(damage);
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
        RaycastHit2D hit = Physics2D.Raycast(position + up, up, shootLength, mask);
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
