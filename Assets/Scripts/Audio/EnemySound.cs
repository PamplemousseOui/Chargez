using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySound : MonoBehaviour
{
    public SoundEmitter emitter;
    public HealthComponent health;
    public EnemyComponent enemy;

    public FMODUnity.EventReference deathEvent;

    private void OnEnable()
    {
        health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        health.OnDeath -= OnDeath;
    }

    private void OnDeath()
    {
        emitter.PlayOneShot(deathEvent);
        Debug.Log("Enemykilled");
    }
}
