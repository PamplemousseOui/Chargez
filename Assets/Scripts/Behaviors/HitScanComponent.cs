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
    public float baseShootMinRate;
    public float baseShootMaxRate;
    public float baseShootLength;
    public float baseDamage;
    public bool isShooting { get; private set; }
    public SimpleLookAt lookAtComponent;

    public EventHandler<float> OnLoadingRatioUpdate;
    public EventHandler OnShootActivate;
    public EventHandler OnShootDeactivate;

    private float m_curShootDuration;
    private float m_curLoadingTime;
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

        ReinitShoot();
    }

    private void Update()
    {
        if (!GameManager.gameIsPaused && GameManager.player.healthComponent.isAlive)
        {
            if (m_curRateTimerValue <= m_curRate)
            {
                m_curRateTimerValue += Time.deltaTime;
                if (m_curRateTimerValue > m_curRate)
                {
                    StartLoading();
                }
            }
        }

        UpdateLoading();
        UpdateShoot();
        UpdateRaycast();
    }

    private void StartLoading()
    {
        m_isLoading = true;
        m_curShootLength = baseShootLength;

        m_curLoadingTimerValue = 0;
        m_curLoadingTime = baseLoadingTime;
    }

    private void UpdateLoading()
    {
        if (m_isLoading)
        {
            m_curLoadingTimerValue += Time.deltaTime;
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
        m_curshootDurationTimerValue = 0; 
        m_curShootDuration = baseShootDuration;
        lookAtComponent.SetCanRotate(false);
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
        if (isShooting)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position + transform.up, transform.up, m_curShootLength);

            Debug.DrawLine(transform.position, transform.position + transform.up * m_curShootLength, Color.red);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject.TryGetComponent(out HealthComponent healthComponent))
                {
                    Debug.Log("hit by archer");
                    m_curDamage = baseDamage;
                    healthComponent.ApplyDamage(m_curDamage);
                }
                StopShoot();
            }
        }
        else if (m_isLoading)
        {
            Debug.DrawLine(transform.position, transform.position + transform.up * m_curShootLength, loadingColor);
        }
    }
}
