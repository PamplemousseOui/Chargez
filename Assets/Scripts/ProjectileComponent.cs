using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    public float baseSpeed;
    public float baseDamage;
    public float baseLifetime;

    private float m_curSpeed;
    private float m_curDamage;
    private float m_curLifetime;
    private float m_curAliveTime;

    private void Start()
    {
        m_curSpeed = baseSpeed;
        m_curDamage = baseDamage;
        m_curLifetime = baseLifetime;
        m_curAliveTime = 0;
    }

    private void FixedUpdate()
    {
        if (!GameManager.gameIsPaused)
        {
            Vector3 direction = transform.up.normalized * m_curSpeed * 0.01f;
            transform.position += direction;

            m_curAliveTime += Time.deltaTime;
            if (m_curAliveTime > m_curLifetime)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out HealthComponent healthComponent))
        {
            healthComponent.ApplyDamage(m_curDamage);
        }
        Destroy(this);
    }
}
