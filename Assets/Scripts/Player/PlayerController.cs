    using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
    using System.Security.Cryptography;
    using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerController : MonoBehaviour
{
    [Header("Controller data")]
    public float attackTime = 2f;
    public float attackMinLoadingTime = 1f;
    public float attackMaxLoadingTime = 5f;
    public bool attackAutoReleaseOnLoadingEnd;
    public float attackRecoveryTime;
    public List<float> attackRotations;
    public List<Modifier> modifiers = new List<Modifier>();
    private float GetModifierValue(string _name)
    {
        var modifier = modifiers.Find(x => x.name == _name);
        return modifier?.value ?? 0.0f;
    }

    public float baseSpeed = 0.1f;
    public float maxTurnSpeed = 1f;
    public float maxDashTurnSpeed = 1f;
    public float turnInertia = 0.1f;

    public float baseDashSpeed;
    public float baseDashConsumption;
    public float baseRefillSpeed;
    public bool canRotateWhileDashing;
    public bool canDriftWhileDashing;
    public bool canTurnWhileDashing;
    public bool resetVelocityOnDashEnd;
    public bool isInvincibleDuringDash;
    public float baseDashCooldown;

    public float baseGripFactor;

    public GameObject LeftAttackTriggerPrefab;

    public bool reinitHealthOnNewWave;

    [Header("Setup data")]
    public HealthComponent healthComponent;
    public Slider healthSlider;
    public Slider dashEnergySlider;

    [Header("Fmod parameters")]
    public string dashInvicibilityParam;
    public string dashSpeedParam;
    public string attackLoadingRatioParam;

    //Attack Events
    public static EventHandler OnAttackStart;
    public static EventHandler OnAttackEnd;
    public static EventHandler OnAttackLoadingStart;
    public static EventHandler OnAttackLoadingCancel;
    public static Action OnAttackLoadingEnd;
    public static Action OnAttackRecoveryStart;
    public static Action OnAttackRecoveryEnd;
    public static Action OnAttackReleasable;
    public static Action OnAttackEnding; //reach 0.6s
    public static EventHandler<EnemyType> OnEnemyKilled;

    //Health Events
    public static Action<float, float> OnDamageReceived; //new health ratio, damages
    public static Action OnDeath;

    //Dash Events
    public static EventHandler OnDashStart;
    public static EventHandler OnDashStop;
    public static EventHandler<float> OnDashEnergyConsumption; //new ratio
    public static EventHandler<float> OnDashEnergyRefill; //new ratio

    private float m_currentAttackLoading;
    private float m_currentAttackProgress;
    private float m_curAttackTime;
    private bool m_isAttackRecovering;
    private IEnumerator AttackRecoveryCoroutine;
    private bool m_isPressingNewAttackInput;
    private bool m_isAttackReleasable;
    private bool m_isAttackEnding;

    private bool m_isAttackLoading;
    private bool m_isAttacking;
    public bool isDashing { get; private set; }
    private List<GameObject> m_enemiesWithinMaxRange;
    private float m_currentSpeed => baseSpeed * (1.0f + GetModifierValue("character_speed"));


    private float m_currentTurnSpeed;
    private float m_targetTurnSpeed;

    //Dash
    private float m_curDashSpeed => baseDashSpeed * (1.0f + GetModifierValue("character_speed"));
    private float m_curDashConsumption;
    private float m_curDashRefillSpeed => baseRefillSpeed * (1.0f + GetModifierValue("energy_regeneration"));
    private float m_curDashEnergyRatio;
    private float m_curDashCooldown;
    private float m_dashCooldownValue;
    private bool m_canDash;
    private Vector2 m_curDashDirection;
    private Rigidbody2D m_rigidbody;

    //Attack
    private List<AttackTrigger> m_curAttacks;

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
        
        InputManager.OnAttackPress += StartAttackLoading;
        InputManager.OnAttackRelease += ReleaseAttack;
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
        
        InputManager.OnAttackPress -= StartAttackLoading;
        InputManager.OnAttackRelease -= ReleaseAttack;
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
        StopAttack();
    }

    private void OnHealthUpdate(float _healthRatio, float _damage)
    {
        if (healthSlider.value > _healthRatio && _healthRatio != 0)
        {
            OnDamageReceived?.Invoke(_healthRatio, _damage); //J'AI MAAAAAAAAAAAAAAAAAAAAAAAL
        }
        healthSlider.value = _healthRatio;
    }

    private void OnHealthComponentDeath()
    {
        GameManager.OnPlayerDeath?.Invoke();
        OnDeath?.Invoke();
        GameManager.gameIsPaused = true;
    }

    private void Start()
    {
        //debugAttackProgressObject.SetActive(false);
        //debugMaxRangeObject.transform.localScale = Vector2.one + Vector2.one * attackRange;
        Init();
    }

    private void Init()
    {
        m_curAttacks = new List<AttackTrigger>();
        modifiers = new List<Modifier>();
        ResetAttack();

        m_curDashEnergyRatio = 1;
        InitDashProperties();
        if (AttackRecoveryCoroutine != null)
            StopCoroutine(AttackRecoveryCoroutine);
    }

    private void Update()
    {
        if (healthComponent.isAlive && !GameManager.gameIsPaused)
        {
            UpdateDashCooldown();

            if (m_isAttackLoading)
            {
                UpdateAttackLoading();
                if (m_currentAttackLoading > attackMaxLoadingTime)
                {
                    if (attackAutoReleaseOnLoadingEnd)
                    {
                        StartAttack();
                    }
                    
                    StopAttackLoading(true);
                }
            }
            else if (m_isAttacking)
            {
                UpdateAttackProgress();
            }
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

    private void StartAttackLoading()
    {
        if (!m_isPressingNewAttackInput)
            m_isPressingNewAttackInput = true;
        //Debug.Log("Attack loading start");
        if (m_isAttackRecovering || m_isAttacking || GameManager.gameIsPaused || !healthComponent.isAlive) return;
        else
        {
            m_isPressingNewAttackInput = false;
            OnAttackLoadingStart?.Invoke(this, null);
            m_isAttackLoading = true;
            m_currentAttackLoading = 0f;
        }
    }

    private void ReleaseAttack()
    {
        m_isPressingNewAttackInput = false;
        if (m_isAttackLoading && !(GameManager.gameIsPaused || !healthComponent.isAlive))
        {
            if (m_currentAttackLoading > attackMinLoadingTime)
            {
                StartAttack();
            }
            else
            {
                StopAttackLoading(false);
            }
            m_isAttackLoading = false;
        }
    }

    private void StopAttackLoading(bool _maxTimeReached)
    {
        m_isAttackLoading = false;
        if (_maxTimeReached)
        {
            //Debug.Log("Attack loading end");
            OnAttackLoadingEnd?.Invoke();
        }
        else
        {
            //Debug.Log("Attack loading canceled");
            OnAttackLoadingCancel?.Invoke(this, null);
        }
    }

    private void StartAttack()
    {
        //Debug.Log("Firing attack");
        OnAttackStart?.Invoke(this, null);
        m_isAttacking = true;
        m_isAttackReleasable = false;
        ResetAttackTriggers();

        List<float> attacks = new List<float>(attackRotations);
        if(GetModifierValue("backward_attack") > 0.1f) attacks.Add(180.0f);
        
        foreach (var attackRot in attacks)
        {
            GameObject curAttack = Instantiate(LeftAttackTriggerPrefab, transform);
            curAttack.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, attackRot);
            AttackTrigger attackTrigger = curAttack.GetComponent<AttackTrigger>();
            attackTrigger.ApplyAreaModifier(GetModifierValue("attack_area"));
            attackTrigger.ApplyDurationModifier(GetModifierValue("attack_duration"));
            m_curAttacks.Add(attackTrigger);    
        }
        
        m_currentAttackProgress = 0f;
        m_curAttackTime = attackTime + GetModifierValue("attack_duration");
    }

    private void StopAttack()
    {
        //Debug.Log("Attack stopped");
        OnAttackEnd?.Invoke(this, null);
        if (m_isAttacking)
        {
            m_isAttackRecovering = true;
            m_isAttacking = false;
            m_isAttackEnding = false;
            AttackRecoveryCoroutine = AttackRecoveryDelay();
            StartCoroutine(AttackRecoveryCoroutine);
        }
        ResetAttackTriggers();
    }

    private IEnumerator AttackRecoveryDelay()
    {
        OnAttackRecoveryStart?.Invoke();
        yield return new WaitForSeconds(attackRecoveryTime);
        m_isAttackRecovering = false;
        OnAttackRecoveryEnd?.Invoke();
        if (m_isPressingNewAttackInput)
        {
            StartAttackLoading();
        }
    }

    private void ResetAttackTriggers()
    {
        m_curAttacks = new List<AttackTrigger>();
    }

    private void UpdateAttackLoading()
    {
        m_currentAttackLoading += Time.deltaTime;
        if (m_currentAttackLoading > attackMinLoadingTime && !m_isAttackReleasable)
        {
            m_isAttackReleasable = true;
            OnAttackReleasable?.Invoke();
        }
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(attackLoadingRatioParam, m_currentAttackLoading / attackMaxLoadingTime);
    }

    private void UpdateAttackProgress()
    {
        m_currentAttackProgress += Time.deltaTime;

        if (m_curAttackTime - m_currentAttackProgress < 0.6f && !m_isAttackEnding)
        {
            m_isAttackEnding = true;
            OnAttackEnding?.Invoke();
        }

        if (m_currentAttackProgress > m_curAttackTime)
            StopAttack();
        else
        {
            CheckEnemyWithinMaxRange();
        }
    }

    private void ResetAttack()
    {
        m_isAttackRecovering = false;
        m_isAttacking = false;
        m_isAttackLoading = false;
        m_isAttackReleasable = false;
        m_isAttackEnding = false;
        m_curAttackTime = 0;
        m_currentAttackLoading = 0;
    }

    private void CheckEnemyWithinMaxRange()
    {
        List<EnemyComponent> killedEnemies = new List<EnemyComponent>();
        foreach (AttackTrigger attack in m_curAttacks)
        {
            foreach (EnemyComponent enemy in attack.enemiesInRange)
            {
                killedEnemies.Add(enemy);
            }   
        }

        for (int i = killedEnemies.Count - 1; i >= 0; --i)
        {
            EnemyComponent enemy = killedEnemies[i];
            enemy.healthComponent.InstantKill();
            OnEnemyKilled?.Invoke(this, enemy.type);
            //Debug.Log($"Destroying enemy {enemy.name}");
        }
    }

    private void UpdatePosition()
    {
        if (!isDashing || canRotateWhileDashing)
        {
            if (InputManager.isLeft)
            {
                //Debug.Log("Turning left");
                m_targetTurnSpeed = 1.0f;
            }
            else if (InputManager.isRight)
            {
                //Debug.Log("Turning left");
                m_targetTurnSpeed = -1.0f;
            }
            else
                m_targetTurnSpeed = 0;

            m_targetTurnSpeed *= isDashing ? maxDashTurnSpeed : maxTurnSpeed;
            m_targetTurnSpeed *= Time.deltaTime;

            m_currentTurnSpeed = Mathf.Lerp(m_currentTurnSpeed, m_targetTurnSpeed, turnInertia);
            gameObject.transform.Rotate(transform.forward, m_currentTurnSpeed);
        }

        if (isDashing)
        {
            if (canDriftWhileDashing)
            {
                if (canTurnWhileDashing)
                {
                    m_rigidbody.velocity = Vector2.Lerp(m_rigidbody.velocity.normalized, gameObject.transform.up.normalized, baseGripFactor) * m_curDashSpeed;
                }
                else
                    m_rigidbody.velocity = Vector2.Lerp(m_rigidbody.velocity.normalized, m_curDashDirection.normalized, baseGripFactor) * m_curDashSpeed;
            }
            else
            {
                if (canTurnWhileDashing)
                    m_rigidbody.velocity = gameObject.transform.up.normalized * m_curDashSpeed;
                else
                    m_rigidbody.velocity = m_curDashDirection.normalized * m_curDashSpeed;
            }
        }
        else
        {
            m_rigidbody.velocity = Vector2.Lerp(m_rigidbody.velocity.normalized, gameObject.transform.up.normalized, baseGripFactor) * m_currentSpeed;
        }
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
        m_curDashConsumption = baseDashConsumption;
        m_curDashCooldown = baseDashCooldown;

        //Setting global fmod parameters
        float invicibilityParam = 0;
        if (isInvincibleDuringDash) invicibilityParam = 1;
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(dashInvicibilityParam, invicibilityParam);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName(dashSpeedParam, m_curDashSpeed);
    }

    private void StartDash()
    {
        if (m_canDash)
        {
            m_curDashDirection = transform.up.normalized;
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
                m_rigidbody.velocity = transform.up.normalized * m_currentSpeed;
            isDashing = false;
            OnDashStop?.Invoke(this, null);
            StartDashCooldown();
            healthComponent.SetCanTakeDamage(true);
        }
    }

    private void UpdateDash()
    {
        if (isDashing)
        {
            if (m_curDashEnergyRatio > 0)
            {
                m_curDashEnergyRatio -= m_curDashConsumption * Time.deltaTime;
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
            m_curDashEnergyRatio += m_curDashRefillSpeed * Time.deltaTime;
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

    public float GetCurrentDashSpeed()
    {
        return m_curDashSpeed;
    }

    private void OnGameRetry()
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
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

