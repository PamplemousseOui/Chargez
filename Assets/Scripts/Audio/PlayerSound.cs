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

    public ParamRef healthParam;

    private void OnEnable()
    {
        PlayerController.OnDashStart += OnDashStart;
        PlayerController.OnDashStop += OnDashStop;

        PlayerController.OnAttackLoadingStart += OnAttackLoadingStart;
        PlayerController.OnAttackLoadingCancel += OnAttackLoadingCancel;
        PlayerController.OnAttackReleasable += OnAttackReleasable;
        PlayerController.OnAttackStart += OnAttackStart;
        PlayerController.OnAttackEnding += OnAttackEnding;

        PlayerController.OnDamageReceived += OnDamageReceived;
        PlayerController.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        PlayerController.OnDashStart -= OnDashStart;
        PlayerController.OnDashStop -= OnDashStop;

        PlayerController.OnAttackLoadingStart -= OnAttackLoadingStart;
        PlayerController.OnAttackLoadingCancel -= OnAttackLoadingCancel;
        PlayerController.OnAttackReleasable -= OnAttackReleasable;
        PlayerController.OnAttackStart -= OnAttackStart;
        PlayerController.OnAttackEnding -= OnAttackEnding;

        PlayerController.OnDamageReceived -= OnDamageReceived;
        PlayerController.OnDeath -= OnDeath;
    }

    private void OnDashStart(object sender, EventArgs e)
    {
        emitter.PlayOneShot(dashStartEvent);
        emitter.Play(dashLoopEvent);
    }

    private void OnDashStop(object sender, EventArgs e)
    {
        emitter.PlayOneShot(dashStopEvent);
        emitter.Stop(dashLoopEvent);
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

    private void OnDeath()
    {
        emitter.PlayOneShot(deathEvent);
    }
}
