using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyComponent : MonoBehaviour
{
    public EnemyType type;
    public float contactDamage;
    public HealthComponent healthComponent;

    private void OnEnable()
    {
        if (healthComponent != null)
            healthComponent.OnDeath += OnDeath;

        GameManager.OnGameRetry += OnGameRetry;
    }

    private void OnDisable()
    {
        if (healthComponent != null)
            healthComponent.OnDeath -= OnDeath;

        GameManager.OnGameRetry -= OnGameRetry;
    }

    private void OnDeath(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Tag.Player.ToString())
        {
            if (collision.TryGetComponent(out HealthComponent healthComponent))
            {
                if (healthComponent.isAlive)
                {
                    healthComponent.ApplyDamage(contactDamage);
                }
            }
        }
    }

    private void OnGameRetry(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }
}
