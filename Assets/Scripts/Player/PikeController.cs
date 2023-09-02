using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PikeController : MonoBehaviour
{
    private Animator m_animator;
    private Quaternion m_orientation;
    private bool m_isDashing;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        //transform.parent = null;
    }

    private void Update()
    {
        transform.rotation = m_orientation;
        transform.position = GameManager.player.transform.position;
    }
    
    private void OnEnable()
    {
        PlayerController.OnDashStart += DashStart;
        PlayerController.OnDashStop += DashStop;
    }

    private void OnDisable()
    {
        PlayerController.OnDashStart -= DashStart;
        PlayerController.OnDashStop -= DashStop;
        
    }


    private void DashStart(object sender, EventArgs e)
    {
        m_animator.SetTrigger("StartDash");
        m_orientation = GameManager.player.transform.rotation;
        m_isDashing = true;
    }

    private void DashStop(object sender, EventArgs e)
    {
        m_isDashing = false;
        m_animator.SetTrigger("StopDash");

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<EnemyComponent>(out var enemy))
        {
            if(enemy.healthComponent.isAlive) enemy.healthComponent.InstantKill();
        }
    }
}
