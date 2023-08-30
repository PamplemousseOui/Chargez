using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public float startHealth = 10f;
    public float currentHealth;
    public bool isAlive;

    public Action<float,float> OnHealthUpdated; //new ratio, damages
    public Action OnDeath;

    private bool m_canTakeDamage;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        currentHealth = startHealth;
        OnHealthUpdated?.Invoke(currentHealth / startHealth, 0);
        isAlive = true;
        m_canTakeDamage = true;
    }

    public void ApplyDamage(float _damage)
    {
        if (currentHealth > 0)
        {
            currentHealth -= _damage;
            OnHealthUpdated?.Invoke(currentHealth / startHealth, _damage);
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
            Debug.Log($"Received {_damage}. {currentHealth} HP remaining.");
            if (currentHealth == 0)
            {
                Kill();
            }
        }
    }

    public void InstantKill()
    {
        Kill();
    }

    private void Kill()
    {
        if (m_canTakeDamage)
        {
            isAlive = false;
            Debug.Log($"Character {gameObject.name} is dead. Cheh.");
            OnDeath?.Invoke();
        }
    }

    public void SetCanTakeDamage(bool _canTakeDamage)
    {
        m_canTakeDamage = _canTakeDamage;
    }

    public bool GetCanTakeDamage()
    {
        return m_canTakeDamage;
    }
}
