using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator m_animator;
    // Start is called before the first frame update
    void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        PlayerController.OnAttackStart += OnAttackStart;
        PlayerController.OnAttackEnd += OnAttackEnd;
        PlayerController.OnAttackLoadingStart += OnAttackLoadingStart;
        PlayerController.OnAttackLoadingCancel += OnAttackLoadingCancel;
        PlayerController.OnAttackLoadingEnd += OnAttackLoadingEnd;
        PlayerController.OnAttackRecoveryStart += OnAttackRecoveryStart;
        PlayerController.OnAttackRecoveryEnd += OnAttackRecoveryEnd;
    }

    private void OnDisable()
    {
        PlayerController.OnAttackStart -= OnAttackStart;
        PlayerController.OnAttackEnd -= OnAttackEnd;
        PlayerController.OnAttackLoadingStart -= OnAttackLoadingStart;
        PlayerController.OnAttackLoadingCancel -= OnAttackLoadingCancel;
        PlayerController.OnAttackLoadingEnd -= OnAttackLoadingEnd;
        PlayerController.OnAttackRecoveryStart -= OnAttackRecoveryStart;
        PlayerController.OnAttackRecoveryEnd -= OnAttackRecoveryEnd;
    }
    
    private void OnAttackStart(object sender, EventArgs e)
    {
        m_animator.SetTrigger("ReleaseAttack");
    }

    private void OnAttackEnd(object sender, EventArgs e)
    {
        
    }

    private void OnAttackLoadingStart(object sender, EventArgs e)
    {
        m_animator.SetTrigger("ChargeAttack");
        
    }

    private void OnAttackLoadingCancel(object sender, EventArgs e)
    {
        m_animator.SetTrigger("CancelAttack");
    }

    private void OnAttackLoadingEnd(object sender, EventArgs e)
    {
        
    }

    private void OnAttackRecoveryStart()
    {
        
    }

    private void OnAttackRecoveryEnd()
    {
        
    }
}
