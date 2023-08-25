using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public float startHealth = 10f;
    public float currentHealth;
    public bool isAlive;

    public EventHandler OnDeath;

    private void Start()
    {
        currentHealth = startHealth;
        isAlive = true;
    }

    public void ApplyDamage(float _damage)
    {
        currentHealth -= _damage;
        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    public void InstantKill()
    {
        Kill();
    }

    private void Kill()
    {
        isAlive = false;
        Debug.Log($"Character {gameObject.name} is dead. Cheh.");
        OnDeath?.Invoke(this, null);
    }
}
