using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HitScanComponent : MonoBehaviour
{
    [Header("If no object specified, target = player")]
    public GameObject target;
    public Color loadingColor;
    public Color shootingColor;
    public float baseShootDuration;
    public float baseLoadingTime;
    public float baseStopRotationBeforeShootTime;
    public float baseShootMinRate;
    public float baseShootMaxRate;
    public float baseShootLength;
    public float baseDamage;
    public bool freezeOnPlayerHit;
    public bool isShooting { get; private set; }
    public SimpleLookAt lookAtComponent;
    public Transform rayTransform;
    public SpriteRenderer raySprite;

    public EventHandler<float> OnLoadingRatioUpdate;
    public EventHandler OnShootActivate;
    public EventHandler OnShootDeactivate;

    private float m_curShootDuration;
    private float m_curLoadingTime;
    private float m_curStopRotationBeforeShootTime;
    private float m_curRate;
    private float m_curRateTimerValue;
    private float m_curLoadingTimerValue;
    private float m_curshootDurationTimerValue;
    private float m_curShootLength;
    private float m_curDamage;
    
    private bool m_isLoading;
    private bool m_canHitPlayer;

    private void Start()
    {
        if (target == null)
            target = GameManager.player.gameObject;
        DeactivateRay();
        ReinitShoot();
    }

    private void Update()
    {
        if (!GameManager.gameIsPaused && GameManager.player.healthComponent.isAlive && (GameManager.canUpdateEnemies && freezeOnPlayerHit))
        {
            if (freezeOnPlayerHit)
            {
                if ((freezeOnPlayerHit && GameManager.canUpdateEnemies) || !freezeOnPlayerHit)
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
        }
    }

    private void StartLoading()
    {
        m_isLoading = true;
        m_curShootLength = baseShootLength;

        m_curLoadingTimerValue = 0;
        m_curLoadingTime = baseLoadingTime;
        m_curStopRotationBeforeShootTime = baseStopRotationBeforeShootTime;

        raySprite.color = loadingColor;
        ActivateRay();
    }

    private void UpdateLoading()
    {
        if (m_isLoading)
        {
            m_curLoadingTimerValue += Time.deltaTime;
            if(m_curLoadingTimerValue > m_curLoadingTime - m_curStopRotationBeforeShootTime)
                lookAtComponent.SetCanRotate(false);
                
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
    }

    private void StartShoot()
    {
        isShooting = true;
        m_canHitPlayer = true;
        m_curshootDurationTimerValue = 0; 
        m_curShootDuration = baseShootDuration;

        raySprite.color = shootingColor;
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
        DeactivateRay();
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
        if (isShooting && m_canHitPlayer)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up, transform.up, m_curShootLength, LayerMask.GetMask("Default"));

            Debug.DrawLine(transform.position, transform.position + transform.up * m_curShootLength, Color.red);

            if (hit.collider != null && hit.transform.gameObject.tag == Tag.Player.ToString())
            {
                if (hit.collider.gameObject.TryGetComponent(out HealthComponent healthComponent))
                {
                    Debug.Log($"hit object {hit.transform.gameObject}");
                    m_curDamage = baseDamage;
                    healthComponent.ApplyDamage(m_curDamage);
                    m_canHitPlayer = false;
                }
                //StopShoot();
            }
        }
        else if (m_isLoading)
        {
            Debug.DrawLine(transform.position, transform.position + transform.up * m_curShootLength, loadingColor);
        }
    }
    
    private void ActivateRay()
    {
        rayTransform.gameObject.SetActive(true);
        rayTransform.localPosition = new Vector3(0, m_curShootLength * 1 / transform.localScale.y / 2, 0);
        rayTransform.localScale = new Vector3(0.2f, m_curShootLength * 1 / transform.localScale.y, 1);
    }

    private void DeactivateRay()
    {
        rayTransform.gameObject.SetActive(false);
    }
}
