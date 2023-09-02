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
    public FMODUnity.EventReference attackLoadStartEvent;
    public FMODUnity.EventReference attackLoadLoopEvent;
    public FMODUnity.EventReference attackLoadEndEvent;
    public FMODUnity.EventReference attackStartEvent;
    public FMODUnity.EventReference attackEndingEvent;

    private void OnEnable()
    {
        PlayerController.OnDashStart += OnDashStart;
        PlayerController.OnAttackLoadingStart += OnAttackLoadingStart;
        PlayerController.OnAttackLoadingCancel += OnAttackLoadingCancel;
        PlayerController.OnAttackReleasable += OnAttackReleasable;
        PlayerController.OnAttackStart += OnAttackStart;
        PlayerController.OnAttackEnding += OnAttackEnding;
    }

    private void OnDisable()
    {
        PlayerController.OnDashStart -= OnDashStart;
        PlayerController.OnAttackLoadingStart -= OnAttackLoadingStart;
        PlayerController.OnAttackLoadingCancel -= OnAttackLoadingCancel;
        PlayerController.OnAttackReleasable -= OnAttackReleasable;
        PlayerController.OnAttackStart -= OnAttackStart;
        PlayerController.OnAttackEnding -= OnAttackEnding;
    }

    private void OnDashStart(object sender, EventArgs e)
    {
        emitter.PlayOneShot(dashStartEvent);
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
}
