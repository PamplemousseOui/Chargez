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
    public float attackSpeed = 2f;
    public float attackAngle = 1f;
    public float attackMinLoadingTime = 1f;
    public float attackMaxLoadingTime = 5f;
    public List<Modifier> modifiers = new List<Modifier>();
    public float baseSpeed = 0.1f;
    public float maxTurnSpeed = 1f;
    public float turnInertia = 0.1f;

    [Header("Setup data")]
    public GameObject debugAttackProgressObject;
    public GameObject debugMaxRangeObject;
    public GameObject debugAttackMinLoadingFeedback;
    public GameObject debugAttackMaxLoadingFeedback;
    public AttackRangeTrigger attackRangeTrigger;
    public HealthComponent healthComponent;

    public static EventHandler OnPlayerAttackStart;
    public static EventHandler OnPlayerAttackEnd;
    public static EventHandler OnPlayerAttackLoadingStart;
    public static EventHandler OnPlayerAttackLoadingCancel;
    public static EventHandler OnPlayerAttackLoadingEnd;
    public static EventHandler OnPlayerReceiveDamage;
    public static EventHandler<EnemyType> OnEnemyKilled;

    private float m_currentHealth;
    private float m_currentAttackLoading;
    private float m_currentAttackProgress;
    private float m_currentAttackRange;
    private bool m_isLoading;
    private bool m_isAttacking;
    private List<GameObject> m_enemiesWithinMaxRange;
    private float m_currentSpeed;
    private float m_currentTurnSpeed;
    private float m_targetTurnSpeed;
    private float m_curTurnInertia;

    private void Awake()
    {
        GameManager.player = this;
    }

    private void Start()
    {
        m_currentHealth = maxHealth;
        debugAttackProgressObject.SetActive(false);
        debugMaxRangeObject.transform.localScale = Vector2.one + Vector2.one * attackRange;

        debugAttackMinLoadingFeedback.transform.localScale = Vector3.zero;
        debugAttackMaxLoadingFeedback.transform.localScale = Vector3.zero;

        m_currentSpeed = baseSpeed;
    }

    private void Update()
    {
        if (healthComponent.isAlive && !GameManager.gameIsPaused)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartAttackLoading();
            }

            if (Input.GetMouseButtonUp(0))
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
            UpdatePosition();
    }

    private void StartAttackLoading()
    {
        Debug.Log("Attack loading start");
        OnPlayerAttackLoadingStart?.Invoke(this, null);
        m_isLoading = true;
        m_currentAttackLoading = 0f;

        debugAttackMinLoadingFeedback.transform.localScale = Vector3.zero;
        debugAttackMaxLoadingFeedback.transform.localScale = Vector3.zero;
    }

    private void StopAttackLoading(bool _maxTimeReached)
    {
        m_isLoading = false;
        if (_maxTimeReached)
        {
            Debug.Log("Attack loading end");
            OnPlayerAttackLoadingEnd?.Invoke(this, null);
        }
        else
        {
            Debug.Log("Attack loading canceled");
            OnPlayerAttackLoadingCancel?.Invoke(this, null);
        }

        debugAttackMinLoadingFeedback.transform.localScale = Vector3.zero;
        debugAttackMaxLoadingFeedback.transform.localScale = Vector3.zero;
    }

    private void FireAttack()
    {
        Debug.Log("Firing attack");
        OnPlayerAttackStart?.Invoke(this, null);
        m_isAttacking = true;
        m_currentAttackProgress = 0f;
        m_currentAttackRange = 0f;
        debugAttackProgressObject.SetActive(true);

        debugAttackMinLoadingFeedback.transform.localScale = Vector3.zero;
        debugAttackMaxLoadingFeedback.transform.localScale = Vector3.zero;
    }

    private void StopAttack()
    {
        Debug.Log("Attack stopped");
        OnPlayerAttackEnd?.Invoke(this, null);
        m_isAttacking = false;
        debugAttackProgressObject.SetActive(false);
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
            Debug.Log($"maxLoadingTimeCompletion = {maxLoadingTimeCompletion}");
            debugAttackMaxLoadingFeedback.transform.localScale = new Vector3(0.1f, 1 * maxLoadingTimeCompletion, 1);
            debugAttackMaxLoadingFeedback.transform.localPosition = new Vector3(0, -0.5f + maxLoadingTimeCompletion / 2f, 0);
        }
        //Debug.Log($"Current attack loading = {m_currentAttackLoading}");
    }

    private void UpdateAttackProgress()
    {
        m_currentAttackProgress += Time.deltaTime * attackSpeed;

        //Debug.Log($"Current attack progress = {m_currentAttackProgress}");
        m_currentAttackRange = m_currentAttackProgress * attackRange;
        //Debug.Log($"Current attack range = {m_currentAttackRange}");
        if (m_currentAttackRange > attackRange)
            StopAttack();
        else
        {
            debugAttackProgressObject.transform.localScale = Vector2.one + Vector2.one * m_currentAttackRange;
            CheckEnemyWithinMaxRange();
        }
    }

    private void CheckEnemyWithinMaxRange()
    {
        List<GameObject> killedEnemy = new List<GameObject>();
        foreach (GameObject enemy in attackRangeTrigger.objectsInRange)
        {
            float dotProduct = Vector2.Dot(transform.up, (enemy.transform.position - transform.position).normalized);
            //Debug.Log($"Dot product = {dotProduct}");
            if (Mathf.Abs(dotProduct) <= attackAngle)
            {
                //Debug.Log($"Enemy is in attack angle");
                if ((enemy.transform.position - transform.position).magnitude < m_currentAttackRange)
                {
                    Debug.Log($"Enemy is in attack range. Destroying it");
                    killedEnemy.Add(enemy);
                }
            }
        }

        int count = killedEnemy.Count;
        for (int i  = 0; i < count; i++)
        {
            if (killedEnemy[i].TryGetComponent(out HealthComponent healthComponent) && killedEnemy[i].TryGetComponent(out EnemyComponent enemyComponent))
            {
                healthComponent.InstantKill();
                OnEnemyKilled?.Invoke(this, enemyComponent.type);
            }
            else
                Debug.LogError("Enemy is missing components to be properly killed");
        }
    }

    private void UpdatePosition()
    {
        m_currentSpeed = baseSpeed;

        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            //Debug.Log("Turning left");
            m_targetTurnSpeed = maxTurnSpeed;
        }
        else if (!Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
        {
            //Debug.Log("Turning left");
            m_targetTurnSpeed = -maxTurnSpeed;
        }
        else
            m_targetTurnSpeed = 0;
            
        m_currentTurnSpeed = Mathf.Lerp(m_currentTurnSpeed, m_targetTurnSpeed, turnInertia);
        gameObject.transform.Rotate(transform.forward, m_currentTurnSpeed);

        Vector3 direction = gameObject.transform.up.normalized * m_currentSpeed * 0.01f;
        gameObject.transform.position += direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Tag.Enemy.ToString())
        {
            Debug.Log("Oh lala Brive la Gaillarde");
            if (collision.TryGetComponent(out EnemyComponent enemyComponent))
            {
                float damageToApply = enemyComponent.contactDamage;
                healthComponent.ApplyDamage(damageToApply);
                if (!healthComponent.isAlive)
                    GameManager.OnPlayerDeath?.Invoke(this, null);
            }
            else
                Debug.LogWarning("No enemy component found on enemy.");
        }
    }
}

public static class ExtensionMethods
{

    public static float MapValueRange(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

}

