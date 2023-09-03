using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CavController : MonoBehaviour
{
    public WallController wallController;

    private EnemyComponent m_enemy;
    private CavController m_left;
    private CavController m_right;
    private Animator m_animator;

    private bool m_main = true;
    public void SetLeft(CavController _left)
    {
        m_left = _left;
    }

    public void SetRight(CavController _right)
    {
        m_right = _right;
    }

    public void SetWaitingTime(float _wait)
    {
        StartCoroutine(EnableAnimation(_wait));
    }

    private IEnumerator EnableAnimation(float _wait)
    {
        yield return new WaitForSeconds(_wait);
        m_animator.enabled = true;
    }

    private void Awake()
    {
        m_enemy = GetComponent<EnemyComponent>();
        m_animator = GetComponent<Animator>();
        m_enemy.healthComponent.OnDeath += PlayKillAnimation;
        m_animator.enabled = false;
    }

    private void PlayKillAnimation()
    {
        if (m_main) m_animator.SetTrigger("DeathMain");
        else m_animator.SetTrigger("Death");
    }

    public void DestroyNeighbour()
    {
        if(m_left) m_left.Kill();
        if(m_right) m_right.Kill();
    }
    public void ResquestDestroy()
    {
        Destroy(gameObject);
    }
    public void Kill()
    {
        if (!m_enemy.healthComponent.isAlive) return;
        m_main = false;
        m_enemy.healthComponent.InstantKill();
    }
}
