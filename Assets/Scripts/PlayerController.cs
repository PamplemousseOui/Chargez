using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class PlayerController : MonoBehaviour
{
    [Header("Controller data")]
    public float maxHealth = 1f;
    public float attackRange = 2f;
    public float attackTime = 2f;
    public float attackMinLoadingTime = 1f;
    public float attackMaxLoadingTime = 5f;
    public List<Modifier> modifiers = new List<Modifier>();
    public float baseSpeed = 0.1f;
    public float maxTurnSpeed = 1f;
    public float turnInertia = 0.1f;
    public float baseDashSpeed;
    public float baseDashConsumption;
    public float baseRefillSpeed;
    public bool canTurnWhileDashing;
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
    public Rigidbody2D rb;

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

    //Attack
    private GameObject m_curLeftAttackTrigger;
    private AttackTrigger m_leftAttackTrigger;

    private void Awake()
    {
        GameManager.player = this;
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
        rb.velocity = Vector2.zero;
        StopAttack();
    }

    private void OnHealthUpdate(object sender, float _health)
    {
        healthSlider.value = _health;
    }

    private void OnDeath(object sender, EventArgs e)
    {
        GameManager.OnPlayerDeath?.Invoke(this, null);
    }

    private void Start()
    {
        //debugAttackProgressObject.SetActive(false);
        //debugMaxRangeObject.transform.localScale = Vector2.one + Vector2.one * attackRange;

        Init();
    }

    private void Init()
    {
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
        m_curLeftAttackTrigger = Instantiate(LeftAttackTriggerPrefab, transform);
        m_leftAttackTrigger = m_curLeftAttackTrigger.GetComponent<AttackTrigger>();
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
        Destroy(m_curLeftAttackTrigger);
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
        List<GameObject> killedEnemy = new List<GameObject>();
        foreach (GameObject enemy in m_leftAttackTrigger.objectsInRange)
        {

            if ((enemy.transform.position - transform.position).magnitude < m_currentAttackRange)
            {
                //Debug.Log($"Enemy is in attack range. Destroying it");
                killedEnemy.Add(enemy);
            }
        }

        int count = killedEnemy.Count;
        for (int i  = 0; i < count; i++)
        {
            if (killedEnemy[i].TryGetComponent(out HealthComponent healthComponent) && killedEnemy[i].TryGetComponent(out EnemyComponent enemyComponent))
            {
                healthComponent.InstantKill();
                OnEnemyKilled?.Invoke(this, enemyComponent.type);
                Debug.Log($"Destroying enemy {killedEnemy[i].name}");
            }
            else
                Debug.LogError("Enemy is missing components to be properly killed");
        }
    }

    private void UpdatePosition()
    {
        m_currentSpeed = baseSpeed;

        if (!m_isDashing || canTurnWhileDashing)
        {
            if (InputManager.isLeft)
            {
                //Debug.Log("Turning left");
                m_targetTurnSpeed = maxTurnSpeed * Time.deltaTime;
            }
            else if (InputManager.isRight)
            {
                //Debug.Log("Turning left");
                m_targetTurnSpeed = -maxTurnSpeed * Time.deltaTime;
            }
            else
                m_targetTurnSpeed = 0;

            m_currentTurnSpeed = Mathf.Lerp(m_currentTurnSpeed, m_targetTurnSpeed, turnInertia);
            gameObject.transform.Rotate(transform.forward, m_currentTurnSpeed);
        }

        float speedToApply;

        if (m_isDashing)
        {
            speedToApply = m_curDashSpeed;
        }
        else
        {
            speedToApply = m_currentSpeed;
        }

        rb.velocity = Vector2.Lerp(rb.velocity.normalized, gameObject.transform.up.normalized, baseGripFactor) * speedToApply;
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
            m_isDashing = true;
            healthComponent.SetCanTakeDamage(false);
            ComputeDashProperties();
            Debug.Log("Start dash");
        }
    }
    
    private void StopDash()
    {
        if (m_isDashing)
        {
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

