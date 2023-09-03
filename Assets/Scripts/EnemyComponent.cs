using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyComponent : MonoBehaviour
{
    public EnemyType type;
    public float contactDamage;
    public HealthComponent healthComponent;
    public string deathAnimTrigger = "Death";

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

    private void Start()
    {
        EnemyManager.nbEnemies++;
    }

    private void OnDestroy()
    {
        EnemyManager.nbEnemies--;
    }

    private void OnDeath()
    {
        enabled = false;

        if (TryGetComponent(out Animator animator))
        {
            if (deathAnimTrigger.Length > 0)
            {
                animator.SetTrigger(deathAnimTrigger);
            }
        }
        else Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) return;
        if (collision.tag == Tag.Player.ToString() && GameManager.canUpdateEnemies)
        {
            if (collision.TryGetComponent(out HealthComponent healthComponent))
            {   
                if (healthComponent.isAlive && (healthComponent.GetCanTakeDamage() || (!GameManager.player.isDashing && type == EnemyType.Wall)))
                {
                    healthComponent.ApplyDamage(contactDamage);
                }
            }
        }
    }

    private void OnGameRetry()
    {
        Destroy(gameObject);
    }

    public void ResquestDestroy()
    {
        Destroy(gameObject);
    }
}
