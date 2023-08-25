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
        healthComponent.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        healthComponent.OnDeath -= OnDeath;
    }

    private void OnDeath(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }
}
