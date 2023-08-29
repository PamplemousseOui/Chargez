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
    public float attackRange = 2f;
    public float attackTime = 2f;
    public float attackMinLoadingTime = 1f;
    public float attackMaxLoadingTime = 5f;
    public bool attackAutoReleaseOnLoadingEnd;
    public List<float> attackRotations;
    public List<Modifier> modifiers = new List<Modifier>();
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
    public GameObject debugAttackMinLoadingFeedback;
    public GameObject debugAttackMaxLoadingFeedback;
    public HealthComponent healthComponent;
    public Slider healthSlider;
    public Slider dashEnergySlider;

    public static EventHandler OnPlayerAttackStart;
    public static EventHandler OnPlayerAttackEnd;
    public static EventHandler OnPlayerAttackLoadingStart;
    public static EventHandler OnPlayerAttackLoadingCancel;
    public static EventHandler OnPlayerAttackLoadingEnd;
    public static EventHandler OnPlayerReceiveDamage;
    public static EventHandler<EnemyType> OnEnemyKilled;
    public static EventHandler OnDashStart;
    public static EventHandler OnDashStop;
    public static EventHandler<float> OnDashEnergyConsumption;
    public static EventHandler<float> OnDashEnergyRefill;
    public delegate void OnPlayerHitEvent();
    public static OnPlayerHitEvent OnPlayerHit;

    private float m_currentAttackLoading;
    private float m_currentAttackProgress;
    private float m_currentAttackRange;
    private float m_curAttackTime;
    private bool m_isLoading;
    private bool m_isAttacking;
    private bool m_isDashing;
    private List<GameObject> m_enemiesWithinMaxRange;
    private float m_currentSpeed;
    private float m_currentTurnSpeed;
    private float m_targetTurnSpeed;

    //Dash
    private float m_curDashSpeed;
    private float m_curDashConsumption;
    private float m_curDashRefillSpeed;
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
        healthComponent.OnDeath += OnDeath;
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
        healthComponent.OnDeath -= OnDeath;
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
        ReinitPos();
        if (reinitHealthOnNewWave)
            healthComponent.Init();
    }

    private void OnEndWave()
    {
        m_rigidbody.velocity = Vector2.zero;
        StopAttack();
    }

    private void OnHealthUpdate(object sender, float _healthRatio)
    {
        if (healthSlider.value > _healthRatio && _healthRatio != 0)
        {
            OnPlayerHit?.Invoke();
        }
        healthSlider.value = _healthRatio;
    }

    private void OnDeath(object sender, EventArgs e)
    {
        GameManager.OnPlayerDeath?.Invoke(this, null);
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
        debugAttackMinLoadingFeedback.transform.localScale = Vector3.zero;
        debugAttackMaxLoadingFeedback.transform.localScale = Vector3.zero;

        m_currentSpeed = baseSpeed;

        m_curDashEnergyRatio = 1;
        InitDashProperties();
    }

    private void Update()
    {
        if (healthComponent.isAlive && !GameManager.gameIsPaused)
        {
            UpdateDashCooldown();

            if (m_isLoading)
            {
                UpdateAttackLoading();
                if (m_currentAttackLoading > attackMaxLoadingTime)
                {
                    if (attackAutoReleaseOnLoadingEnd)
                    {
                        FireAttack();
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
        //Debug.Log("Attack loading start");
        OnPlayerAttackLoadingStart?.Invoke(this, null);
        m_isLoading = true;
        m_currentAttackLoading = 0f;

        debugAttackMinLoadingFeedback.transform.localScale = Vector3.zero;
        debugAttackMaxLoadingFeedback.transform.localScale = Vector3.zero;
    }

    private void ReleaseAttack()
    {
        if (m_isLoading)
        {
            if (m_currentAttackLoading > attackMinLoadingTime)
            {
                FireAttack();
            }
            else
            {
                StopAttackLoading(false);
            }
            m_isLoading = false;
        }
    }

    private void StopAttackLoading(bool _maxTimeReached)
    {
        m_isLoading = false;
        if (_maxTimeReached)
        {
            //Debug.Log("Attack loading end");
            OnPlayerAttackLoadingEnd?.Invoke(this, null);
        }
        else
        {
            //Debug.Log("Attack loading canceled");
            OnPlayerAttackLoadingCancel?.Invoke(this, null);
        }

        debugAttackMinLoadingFeedback.transform.localScale = Vector3.zero;
        debugAttackMaxLoadingFeedback.transform.localScale = Vector3.zero;
    }

    private void FireAttack()
    {
        //Debug.Log("Firing attack");
        OnPlayerAttackStart?.Invoke(this, null);
        m_isAttacking = true;
        ResetAttack();
        foreach (var attackRot in attackRotations)
        {
            GameObject curAttack = Instantiate(LeftAttackTriggerPrefab, transform);
            curAttack.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, attackRot);
            AttackTrigger attackTrigger = curAttack.GetComponent<AttackTrigger>();
            m_curAttacks.Add(attackTrigger);    
        }
        
        m_currentAttackProgress = 0f;
        m_curAttackTime = attackTime;
        m_currentAttackRange = attackRange;

        debugAttackMinLoadingFeedback.transform.localScale = Vector3.zero;
        debugAttackMaxLoadingFeedback.transform.localScale = Vector3.zero;
    }

    private void StopAttack()
    {
        //Debug.Log("Attack stopped");
        OnPlayerAttackEnd?.Invoke(this, null);
        m_isAttacking = false;
        ResetAttack();
    }

    private void ResetAttack()
    {
        foreach (var attack in m_curAttacks)
        {
            Destroy(attack.gameObject);
        }

        m_curAttacks = new List<AttackTrigger>();
    }

    private void UpdateAttackLoading()
    {
        m_currentAttackLoading += Time.deltaTime;

        if (m_currentAttackLoading < attackMinLoadingTime)
        {
            float minLoadingTimeCompletion = Mathf.Clamp(Mathf.Abs((attackMinLoadingTime - m_currentAttackLoading) / attackMinLoadingTime - 1), 0, 1);
            debugAttackMinLoadingFeedback.transform.localScale = new Vector3(0.1f, 1 * minLoadingTimeCompletion, 1);
            debugAttackMinLoadingFeedback.transform.localPosition = new Vector3(0, -0.5f + minLoadingTimeCompletion / 2f, 0);
        }
        else
        {
            debugAttackMinLoadingFeedback.transform.localScale = new Vector3(0.1f, 1, 1);
            debugAttackMinLoadingFeedback.transform.localPosition = new Vector3(0, 0, 0);

            float maxLoadingTimeCompletion = Mathf.Clamp(Mathf.Abs(ExtensionMethods.MapValueRange(m_currentAttackLoading, attackMinLoadingTime, attackMaxLoadingTime,0,1)), 0, 1);
            debugAttackMaxLoadingFeedback.transform.localScale = new Vector3(0.1f, 1 * maxLoadingTimeCompletion, 1);
            debugAttackMaxLoadingFeedback.transform.localPosition = new Vector3(0, -0.5f + maxLoadingTimeCompletion / 2f, 0);
        }
    }

    private void UpdateAttackProgress()
    {
        m_currentAttackProgress += Time.deltaTime;
        if (m_currentAttackProgress > m_curAttackTime)
            StopAttack();
        else
        {
            CheckEnemyWithinMaxRange();
        }
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

        for (int i = 0; i < killedEnemies.Count; ++i)
        {
            EnemyComponent enemy = killedEnemies[i];
            enemy.healthComponent.InstantKill();
            OnEnemyKilled?.Invoke(this, enemy.type);
            Debug.Log($"Destroying enemy {enemy.name}");
        }
    }

    private void UpdatePosition()
    {
        m_currentSpeed = baseSpeed;

        if (!m_isDashing || canRotateWhileDashing)
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

            m_targetTurnSpeed *= m_isDashing ? maxDashTurnSpeed : maxTurnSpeed;
            m_targetTurnSpeed *= Time.deltaTime;

            m_currentTurnSpeed = Mathf.Lerp(m_currentTurnSpeed, m_targetTurnSpeed, turnInertia);
            gameObject.transform.Rotate(transform.forward, m_currentTurnSpeed);
        }

        if (m_isDashing)
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
        m_curDashSpeed = baseDashSpeed;
        m_curDashConsumption = baseDashConsumption;
        m_curDashRefillSpeed = baseRefillSpeed;
        m_curDashCooldown = baseDashCooldown;
    }

    private void StartDash()
    {
        if (m_canDash)
        {
            m_curDashDirection = transform.up.normalized;
            m_isDashing = true;
            if (isInvincibleDuringDash)
                healthComponent.SetCanTakeDamage(false);
            ComputeDashProperties();
            Debug.Log("Start dash");
        }
    }
    
    private void StopDash()
    {
        if (m_isDashing)
        {
            if (resetVelocityOnDashEnd)
                m_rigidbody.velocity = transform.up.normalized * m_currentSpeed;
            m_isDashing = false;
            StartDashCooldown();
            healthComponent.SetCanTakeDamage(true);
            Debug.Log("Stop dash");
        }
    }

    private void UpdateDash()
    {
        if (m_isDashing)
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

    private void OnGameRetry(object sender, EventArgs e)
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

