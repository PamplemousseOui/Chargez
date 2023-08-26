using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileComponent : MonoBehaviour
{
    private float m_baseSpeed;
    private float m_baseDamage;
    private float m_baseLifetime;

    private float m_curSpeed;
    private float m_curDamage;
    private float m_curLifetime;
    private float m_curAliveTime;

    public void Init(float _speed, float _damage, float _lifetime)
    {
        m_curSpeed = m_baseSpeed = _speed;
        m_curDamage = m_baseDamage = _damage;
        m_curLifetime = m_baseLifetime = _lifetime;
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
        if (collision.tag != Tag.Trigger.ToString())
        {
            if (collision.TryGetComponent(out HealthComponent healthComponent))
            {
                healthComponent.ApplyDamage(m_curDamage);
                Debug.Log($"Object {collision.gameObject.name} received {m_curDamage} damages.");
            }
            Destroy(gameObject);
        }
    }
}
