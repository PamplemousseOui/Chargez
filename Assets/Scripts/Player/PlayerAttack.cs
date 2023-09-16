using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    [Header("Controller data")]
    public float attackTime = 0.66f;
    public float attackMinLoadingTime = 0.5f;
    public float attackMaxLoadingTime = 1.33f;
    public bool attackAutoReleaseOnLoadingEnd = true;
    public float attackRecoveryTime = 0.33f;
    public List<float> attackRotations;
    public GameObject LeftAttackTriggerPrefab;

    [Header("Setup data")]
    public HealthComponent healthComponent;
    public PlayerModifiers playerModifiers;
    [FMODUnity.ParamRef] public string attackLoadingRatioParam;

    private float m_currentAttackLoading;
    private float m_currentAttackProgress;
    private float m_curAttackTime;
    private bool m_isAttackRecovering;
    private IEnumerator AttackRecoveryCoroutine;
    private bool m_isPressingNewAttackInput;
    private bool m_isAttackReleasable;
    private bool m_isAttackEnding;
    private List<AttackTrigger> m_curAttacks;

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

    private bool m_isAttackLoading;
    private bool m_isAttacking;

    private void OnEnable()
    {
        InputManager.OnAttackPress += StartAttackLoading;
        InputManager.OnAttackRelease += ReleaseAttack;
        WaveManager.OnEndWaveEvent += OnEndWave;
    }

    private void OnDisable()
    {


        InputManager.OnAttackPress -= StartAttackLoading;
        InputManager.OnAttackRelease -= ReleaseAttack;
        WaveManager.OnEndWaveEvent -= OnEndWave;
    }

    private void OnEndWave()
    {
        StopAttack();
    }

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        ResetAttack();
        m_curAttacks = new List<AttackTrigger>();
        if (AttackRecoveryCoroutine != null)
            StopCoroutine(AttackRecoveryCoroutine);
    }

    private void Update()
    {
        if (healthComponent.isAlive && !GameManager.gameIsPaused)
        {
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
        if (playerModifiers.GetModifierValue("backward_attack") > 0.1f) attacks.Add(180.0f);

        foreach (var attackRot in attacks)
        {
            GameObject curAttack = Instantiate(LeftAttackTriggerPrefab, transform);
            curAttack.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, attackRot);
            AttackTrigger attackTrigger = curAttack.GetComponent<AttackTrigger>();
            attackTrigger.ApplyAreaModifier(playerModifiers.GetModifierValue("attack_area"));
            attackTrigger.ApplyDurationModifier(playerModifiers.GetModifierValue("attack_duration"));
            m_curAttacks.Add(attackTrigger);
        }

        m_currentAttackProgress = 0f;
        m_curAttackTime = attackTime + playerModifiers.GetModifierValue("attack_duration");
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
}
