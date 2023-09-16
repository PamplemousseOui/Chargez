using FMOD.Studio;
using FMODUnity;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    public SoundEmitter emitter;
    public PlayerController playerController;
    public FMODUnity.EventReference dashStartEvent;
    public FMODUnity.EventReference dashLoopEvent;
    public FMODUnity.EventReference dashStopEvent;
    public FMODUnity.EventReference attackLoadStartEvent;
    public FMODUnity.EventReference attackLoadLoopEvent;
    public FMODUnity.EventReference attackLoadEndEvent;
    public FMODUnity.EventReference attackStartEvent;
    public FMODUnity.EventReference attackEndingEvent;
    public EventReference hitEvent;
    public EventReference deathEvent;
    public EventReference moveEvent;
    public EventReference doinkEvent;

    public ParamRef healthParam;

    private bool m_isMoveLoopPlaying;

    private void OnEnable()
    {
        PlayerController.OnDashStart += OnDashStart;
        PlayerController.OnDashStop += OnDashStop;

        PlayerAttack.OnAttackLoadingStart += OnAttackLoadingStart;
        PlayerAttack.OnAttackLoadingCancel += OnAttackLoadingCancel;
        PlayerAttack.OnAttackReleasable += OnAttackReleasable;
        PlayerAttack.OnAttackStart += OnAttackStart;
        PlayerAttack.OnAttackEnding += OnAttackEnding;

        PlayerController.OnDamageReceived += OnDamageReceived;
        PlayerController.OnDeath += OnDeath;

        PlayerController.OnPlayerHitWall += OnPlayerHitWall;

        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameRetry += OnGameRetry;
        GameManager.OnGameResume += OnGameResume;
        GameManager.OnGamePause += OnGamePause;
    }

    private void OnDisable()
    {
        PlayerController.OnDashStart -= OnDashStart;
        PlayerController.OnDashStop -= OnDashStop;

        PlayerAttack.OnAttackLoadingStart -= OnAttackLoadingStart;
        PlayerAttack.OnAttackLoadingCancel -= OnAttackLoadingCancel;
        PlayerAttack.OnAttackReleasable -= OnAttackReleasable;
        PlayerAttack.OnAttackStart -= OnAttackStart;
        PlayerAttack.OnAttackEnding -= OnAttackEnding;

        PlayerController.OnDamageReceived -= OnDamageReceived;
        PlayerController.OnDeath -= OnDeath;

        PlayerController.OnPlayerHitWall -= OnPlayerHitWall;

        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameRetry -= OnGameRetry;
        GameManager.OnGameResume -= OnGameResume;
        GameManager.OnGamePause -= OnGamePause;
    }

    private void OnGameStart()
    {
        emitter.Play(moveEvent);
    }

    private void OnGameRetry()
    {
        emitter.Play(moveEvent);
    }

    private void OnGameResume()
    {
        emitter.Play(moveEvent);
    }

    private void OnGamePause()
    {
        emitter.Stop(moveEvent);
    }

    private void OnDashStart(object sender, EventArgs e)
    {
        emitter.PlayOneShot(dashStartEvent);
        emitter.Play(dashLoopEvent);
        emitter.Stop(moveEvent);
    }

    private void OnDashStop(object sender, EventArgs e)
    {
        emitter.PlayOneShot(dashStopEvent);
        emitter.Stop(dashLoopEvent);
        emitter.Play(moveEvent);
    }

    private void OnAttackLoadingStart(object sender, EventArgs e)
    {
        emitter.PlayOneShot(attackLoadStartEvent);
        emitter.Play(attackLoadLoopEvent);
    }

    private void OnAttackLoadingCancel(object sender, EventArgs e)
    {
        StopLoadingLoop();
    }

    private void OnAttackReleasable()
    {
        emitter.PlayOneShot(attackLoadEndEvent);
    }

    private void OnAttackStart(object sender, EventArgs e)
    {
        StopLoadingLoop();
        emitter.PlayOneShot(attackStartEvent);
    }

    private void OnAttackEnding()
    {
        emitter.PlayOneShot(attackEndingEvent);
    }

    private void StopLoadingLoop()
    {
        emitter.Stop(attackLoadLoopEvent);
    }

    private void OnDamageReceived(float _newHealthRatio, float _damage)
    {
        emitter.SetParameter(healthParam, _newHealthRatio);
        emitter.PlayAndForget(hitEvent);
    }

    private void OnPlayerHitWall(Vector2 vector1, Vector2 vector2)
    {
        emitter.PlayOneShot(doinkEvent);
    }

    private void OnDeath()
    {
        FMODUnity.RuntimeManager.StudioSystem.getBus("bus:/", out Bus bus);
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        emitter.PlayOneShot(deathEvent);
        emitter.Stop(moveEvent);
    }
}
