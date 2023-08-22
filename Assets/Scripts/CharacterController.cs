using Microsoft.Unity.VisualStudio.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    [Header("Controller data")]
    public float maxHealth = 1f;
    public float attackRange = 2f;
    public float attackSpeed = 2f;
    public float attackAngle = 1f;
    public float attackMinLoadingTime = 1f;
    public float attackMaxLoadingTime = 5f;
    public List<Modifier> modifiers = new List<Modifier>();

    [Header("Setup data")]
    public GameObject debugAttackProgressObject;
    public GameObject debugMaxRangeObject;
    public Slider debugAttackMinLoadingSlider;
    public Slider debugAttackMaxLoadingSlider;

    public static EventHandler OnPlayerAttackStart;
    public static EventHandler OnPlayerAttackEnd;
    public static EventHandler OnPlayerAttackLoadingStart;
    public static EventHandler OnPlayerAttackLoadingCancel;
    public static EventHandler OnPlayerAttackLoadingEnd;
    public static EventHandler OnPlayerReceiveDamage;
    public static EventHandler OnPlayerHit;

    private float m_currentHealth;
    private float m_currentAttackLoading;
    private float m_currentAttackProgress;
    private float m_currentAttackRange;
    private bool m_isLoading;
    private bool m_isAttacking;
    private List<GameObject> m_enemiesWithinMaxRange;

    private void Start()
    {
        m_currentHealth = maxHealth;
        debugAttackProgressObject.SetActive(false);
        debugMaxRangeObject.transform.localScale = Vector2.one + Vector2.one * attackRange;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartAttackLoading();
        }

        if (Input.GetMouseButtonUp(0))
        {
            m_isLoading = false;
            if (m_currentAttackLoading > attackMinLoadingTime)
            {
                FireAttack();
            }
            else
            {
                StopAttackLoading(false);
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

    private void StartAttackLoading()
    {
        Debug.Log("Attack loading start");
        OnPlayerAttackLoadingStart?.Invoke(this, null);
        m_isLoading = true;
        m_currentAttackLoading = 0f;
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
    }

    private void FireAttack()
    {
        Debug.Log("Firing attack");
        OnPlayerAttackStart?.Invoke(this, null);
        m_isAttacking = true;
        m_currentAttackProgress = 0f;
        m_currentAttackRange = 0f;
        debugAttackProgressObject.SetActive(true);
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
        Debug.Log($"Current attack loading = {m_currentAttackLoading}");
    }

    private void UpdateAttackProgress()
    {
        m_currentAttackProgress += Time.deltaTime * attackSpeed;
        Debug.Log($"Current attack progress = {m_currentAttackProgress}");
        m_currentAttackRange = m_currentAttackProgress * attackRange;
        Debug.Log($"Current attack range = {m_currentAttackRange}");
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

    }
}
