using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Controller data")]
    public float baseSpeed = 0.1f;
    public float maxTurnSpeed = 1f;
    public float maxDashTurnSpeed = 1f;
    public float turnInertia = 0.1f;

    public PikeController pike;
    public float baseDashSpeed;
    public float baseDashConsumptionAtStart;
    public float baseDashConsumptionOverTime;
    public float baseRefillSpeed;
    public AnimationCurve dashRefillRatio;
    public bool canRotateWhileDashing;
    public bool canDriftWhileDashing;
    public bool canTurnWhileDashing;
    public bool resetVelocityOnDashEnd;
    public bool isInvincibleDuringDash;
    public float baseDashCooldown;

    public float baseGripFactor;

    public GameObject shieldPrefab;

    public bool reinitHealthOnNewWave;
    public float hitTimeFreezeDuration = 0.2f;

    [Header("Setup data")]
    public HealthComponent healthComponent;
    public Slider healthSlider;
    public Slider dashEnergySlider;
    public PlayerModifiers playerModifiers;

    [Header("Fmod parameters")]
    [FMODUnity.ParamRef] public string dashInvicibilityParam;
    [FMODUnity.ParamRef] public string dashSpeedParam;

    //Health Events
    public static Action<float, float> OnDamageReceived; //new health ratio, damages
    public static Action OnDeath;

    //Dash Events
    public static EventHandler OnDashStart;
    public static EventHandler OnDashStop;
    public static EventHandler<float> OnDashEnergyConsumption; //new ratio
    public static EventHandler<float> OnDashEnergyRefill; //new ratio
    public static Action<Vector2, Vector2> OnPlayerHitWall;
    public bool isDashing { get; private set; }
    private float m_currentSpeed => baseSpeed * (1.0f + playerModifiers.GetModifierValue("character_speed"));
    private float m_currentTurnSpeed;
    private float m_targetTurnSpeed;

    //Dash
    private float m_curDashSpeed => baseDashSpeed * (1.0f + playerModifiers.GetModifierValue("character_speed"));
    private float m_curDashConsumptionOverTime;
    private float m_curDashConsumptionAtStart;
    private float m_curDashRefillSpeed => baseRefillSpeed * (1.0f + playerModifiers.GetModifierValue("energy_regeneration"));
    private float m_curDashEnergyRatio;
    private float m_curDashCooldown;
    private float m_stopDashTimer;
    private float m_dashCooldownValue;
    private bool m_canDash;
    private Vector2 m_curDashDirection;
    private Rigidbody2D m_rigidbody;

    public ScreenShaker screenShaker;
    
    //Shield
    private List<ShieldController> m_shields;

    public void AddShield()
    {
        if (m_shields == null) m_shields = new List<ShieldController>();
        GameObject shieldInstance = Instantiate(shieldPrefab, transform);
        m_shields.Add(shieldInstance.GetComponent<ShieldController>());
        for (int i = 0; i < m_shields.Count; ++i)
        {
            m_shields[i].SetRotation(i / (float)m_shields.Count * 360.0f);
        }
    }

    public void EnablePike()
    {
        pike.gameObject.SetActive(true);
    }
    private void Awake()
    {
        GameManager.player = this;
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        healthComponent.OnHealthUpdated += OnHealthUpdate;
        healthComponent.OnDeath += OnHealthComponentDeath;
        GameManager.OnGameRetry += OnGameRetry;
        WaveManager.OnStartNewWave += OnStartNewWave;
        WaveManager.OnEndWaveEvent += OnEndWave;

        InputManager.OnDashPress += StartDash;
        InputManager.OnDashRelease += StopDash;
    }

    private void OnDisable()
    {
        healthComponent.OnHealthUpdated -= OnHealthUpdate;
        healthComponent.OnDeath -= OnHealthComponentDeath;
        GameManager.OnGameRetry -= OnGameRetry;
        WaveManager.OnStartNewWave -= OnStartNewWave;
        WaveManager.OnEndWaveEvent -= OnEndWave;

        InputManager.OnDashPress -= StartDash;
        InputManager.OnDashRelease -= StopDash;
    }

    private void OnStartNewWave(WaveData _waveData)
    {
        //ReinitPos();
        if (reinitHealthOnNewWave)
            healthComponent.Init();
    }

    private void OnEndWave()
    {
        m_rigidbody.velocity = Vector2.zero;
    }

    private void OnHealthUpdate(float _healthRatio, float _damage)
    {
        if (healthSlider.value >= _healthRatio && _damage >= 0.0f)
        {
            if(playerModifiers.GetModifierValue("explosion") > 0.1f) GameManager.instance.spawnManager.DestroyAllEnemies();
            OnDamageReceived?.Invoke(_healthRatio, _damage); //J'AI MAAAAAAAAAAAAAAAAAAAAAAAL
            StartCoroutine(FreezeTime());
            StartCoroutine(screenShaker.ScreenShake());
        }
        healthSlider.value = _healthRatio;
    }

    private IEnumerator FreezeTime()
    {
        GameManager.instance.PauseGame();
        yield return new WaitForSeconds(hitTimeFreezeDuration); 
        GameManager.instance.ResumeGame();
    }

    private void OnHealthComponentDeath()
    {
        GameManager.OnPlayerDeath?.Invoke();
        OnDeath?.Invoke();
        foreach(var shield in m_shields) Destroy(shield.gameObject);
        
        GameManager.gameIsPaused = true;
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        pike.gameObject.SetActive(false);
        m_shields = new List<ShieldController>();

        m_curDashEnergyRatio = 1;
        InitDashProperties();
    }

    private void Update()
    {
        if (healthComponent.isAlive && !GameManager.gameIsPaused)
        {
            UpdateDashCooldown();
        }
    }

    private void FixedUpdate()
    {
        if (healthComponent.isAlive && !GameManager.gameIsPaused)
        {
            UpdateDash();
            UpdatePosition();
        }
    }

    public AnimationCurve m_hitSpeedOverTime;
    public float m_hitSpeed;
    private Vector2 m_currentVelocity = Vector2.zero;
    private Vector2 m_hitNormal = Vector2.zero;
    private float m_hitTimer = 0.0f; 
    private void UpdatePosition()
    {
        if (!isDashing || canRotateWhileDashing)
        {
            if (InputManager.isLeft)
            {
                //Debug.Log("Turning left");
                m_targetTurnSpeed = 1.0f;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("IsTurning", 1);
            }
            else if (InputManager.isRight)
            {
                //Debug.Log("Turning left");
                m_targetTurnSpeed = -1.0f;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("IsTurning", 1);
            }
            else
            {
                m_targetTurnSpeed = 0;
                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("IsTurning", 0);
            }

            m_targetTurnSpeed *= isDashing ? maxDashTurnSpeed : maxTurnSpeed;
            m_targetTurnSpeed *= Time.deltaTime;

            m_currentTurnSpeed = Mathf.Lerp(m_currentTurnSpeed, m_targetTurnSpeed, turnInertia);
            gameObject.transform.Rotate(transform.forward, m_currentTurnSpeed);
        }

        Vector2 desiredVelocity = Vector2.zero;
        if (isDashing)
        {
            if (canDriftWhileDashing)
            {
                if (canTurnWhileDashing)
                {
                    desiredVelocity = Vector2.Lerp(m_currentVelocity.normalized, gameObject.transform.up.normalized, baseGripFactor) * m_curDashSpeed;
                }
                else
                    desiredVelocity = Vector2.Lerp(m_currentVelocity.normalized, m_curDashDirection.normalized, baseGripFactor) * m_curDashSpeed;
            }
            else
            {
                if (canTurnWhileDashing)
                    desiredVelocity = gameObject.transform.up.normalized * m_curDashSpeed;
                else
                    desiredVelocity = m_curDashDirection.normalized * m_curDashSpeed;
            }
        }
        else
        {
            desiredVelocity = Vector2.Lerp(m_currentVelocity.normalized, gameObject.transform.up.normalized, baseGripFactor) * m_currentSpeed;
        }

        float ratio = m_hitSpeedOverTime.Evaluate(m_hitTimer);
        Vector2 additionnalVelocity = m_hitNormal * ratio * m_hitSpeed;
        m_hitTimer += Time.deltaTime;
        
        Vector2 right = new Vector2(m_hitNormal.y, m_hitNormal.x);
        
        m_rigidbody.velocity = Vector2.Lerp(desiredVelocity, right * Vector2.Dot(right, desiredVelocity), ratio) + additionnalVelocity;
        m_currentVelocity = desiredVelocity;
    }

    private void OnCollisionEnter2D(Collision2D _other)
    {
        m_hitNormal = _other.contacts[0].normal;
        OnPlayerHitWall?.Invoke(_other.contacts[0].point, m_hitNormal);
        m_hitTimer = 0.0f;
    }

    private void ReinitPos()
    {
        transform.position = new Vector3(0, 0, 0);
        transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    //DASH FUNCTIONS
    private void InitDashProperties()
    {
        m_canDash = true;
        dashEnergySlider.value = 1;
        ComputeDashProperties();
    }

    public void ComputeDashProperties()
    {
        m_curDashConsumptionOverTime = baseDashConsumptionOverTime;
        m_curDashConsumptionAtStart = baseDashConsumptionAtStart;
        m_curDashCooldown = baseDashCooldown;

        //Setting global fmod parameters
        float invicibilityParam = 0;
        if (isInvincibleDuringDash) invicibilityParam = 1;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(dashInvicibilityParam, invicibilityParam);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(dashSpeedParam, m_curDashSpeed);
    }

    private void StartDash()
    {
        if (m_canDash && m_curDashEnergyRatio > 0 && !GameManager.gameIsPaused && healthComponent.isAlive)
        {
            m_curDashDirection = transform.up.normalized;
            m_curDashEnergyRatio -= m_curDashConsumptionAtStart;
            isDashing = true;
            OnDashStart?.Invoke(this, null);
            if (isInvincibleDuringDash)
                healthComponent.SetCanTakeDamage(false);
            ComputeDashProperties();
        }
    }
    
    private void StopDash()
    {
        if (isDashing)
        {
            if (resetVelocityOnDashEnd)
                m_currentVelocity = transform.up.normalized * m_currentSpeed;
            isDashing = false;
            OnDashStop?.Invoke(this, null);
            StartDashCooldown();
            healthComponent.SetCanTakeDamage(true);

            m_stopDashTimer = 0.0f;
        }
    }

    private void UpdateDash()
    {
        if (isDashing)
        {
            if (m_curDashEnergyRatio > 0)
            {
                m_curDashEnergyRatio -= m_curDashConsumptionOverTime * Time.deltaTime;
                OnDashEnergyConsumption?.Invoke(this, m_curDashEnergyRatio);

                dashEnergySlider.value = m_curDashEnergyRatio;
            }
            else
            {
                StopDash();
            }
        }
        else if (m_curDashEnergyRatio < 1)
        {
            m_stopDashTimer += Time.deltaTime;
            m_curDashEnergyRatio += m_curDashRefillSpeed * dashRefillRatio.Evaluate(m_stopDashTimer) * Time.deltaTime;
            OnDashEnergyRefill?.Invoke(this, m_curDashEnergyRatio);

            dashEnergySlider.value = m_curDashEnergyRatio;
        }
    }

    private void StartDashCooldown()
    {
        m_canDash = false;
        m_dashCooldownValue = 0;
    }

    private void UpdateDashCooldown()
    {
        if (!m_canDash)
        {
            m_dashCooldownValue += Time.deltaTime;
            if (m_dashCooldownValue > m_curDashCooldown)
            {
                m_canDash = true;
            }
        }
    }
    
    private void OnGameRetry()
    {
        healthComponent.Init();
        Init();
    }
}

public static class ExtensionMethods
{

    public static float MapValueRange(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}

