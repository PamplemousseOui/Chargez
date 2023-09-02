using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator m_animator;

    [SerializeField] private GameObject m_dashFart;
    [SerializeField] private GameObject m_dashGhost;
    // Start is called before the first frame update
    void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        PlayerController.OnAttackStart += OnAttackStart;
        PlayerController.OnAttackEnd += OnAttackEnd;
        PlayerController.OnDashStart += OnDashStart;
        PlayerController.OnDashStop += OnDashStop;
        PlayerController.OnAttackLoadingStart += OnAttackLoadingStart;
        PlayerController.OnAttackLoadingCancel += OnAttackLoadingCancel;
        PlayerController.OnAttackLoadingEnd += OnAttackLoadingEnd;
        PlayerController.OnAttackRecoveryStart += OnAttackRecoveryStart;
        PlayerController.OnAttackRecoveryEnd += OnAttackRecoveryEnd;
        WaveManager.OnEndWaveEvent += OnEndWave;
    }

    private void OnDisable()
    {
        PlayerController.OnAttackStart -= OnAttackStart;
        PlayerController.OnAttackEnd -= OnAttackEnd;
        PlayerController.OnDashStart -= OnDashStart;
        PlayerController.OnDashStop -= OnDashStop;
        PlayerController.OnAttackLoadingStart -= OnAttackLoadingStart;
        PlayerController.OnAttackLoadingCancel -= OnAttackLoadingCancel;
        PlayerController.OnAttackLoadingEnd -= OnAttackLoadingEnd;
        PlayerController.OnAttackRecoveryStart -= OnAttackRecoveryStart;
        PlayerController.OnAttackRecoveryEnd -= OnAttackRecoveryEnd;
        WaveManager.OnEndWaveEvent -= OnEndWave;
    }

    private void OnEndWave()
    {
        m_animator.ResetTrigger("ReleaseAttack");
        m_animator.ResetTrigger("StopAttack");
        m_animator.ResetTrigger("ChargeAttack");
        m_animator.ResetTrigger("CancelAttack");
        m_animator.ResetTrigger("StartDash");
        m_animator.ResetTrigger("StopDash");
    }

    private void OnAttackStart(object sender, EventArgs e)
    {
        m_animator.SetTrigger("ReleaseAttack");
    }

    private void OnAttackEnd(object sender, EventArgs e)
    {
        m_animator.SetTrigger("StopAttack");
        
    }

    private void OnAttackLoadingStart(object sender, EventArgs e)
    {
        m_animator.SetTrigger("ChargeAttack");
        
    }

    private void OnAttackLoadingCancel(object sender, EventArgs e)
    {
        m_animator.SetTrigger("CancelAttack");
    }

    private void OnAttackLoadingEnd()
    {
        
    }

    private void OnAttackRecoveryStart()
    {
        
    }

    private void OnAttackRecoveryEnd()
    {
        
    }

    private void OnDashStart(object sender, EventArgs e)
    {
        m_animator.SetTrigger("StartDash");
        SpawnFart();
    }

    private void OnDashStop(object sender, EventArgs e)
    {
        m_animator.SetTrigger("StopDash");
    }

    public void SpawnFart()
    {
        if (!m_dashFart) return;
        var ghost = Instantiate(m_dashFart, transform.position, transform.rotation);
    }
    

    public void SpawnGhost()
    {
        if (!m_dashGhost) return;
        var sprites = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sprite in sprites)
        {
            var ghost = Instantiate(m_dashGhost, sprite.transform.position, sprite.transform.rotation);
            ghost.transform.localScale = sprite.transform.lossyScale;
            ghost.GetComponent<SpriteRenderer>().sprite = sprite.sprite;
        }
    }
}
