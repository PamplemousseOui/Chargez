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
    public FMODUnity.ParamRef dashInvicibilityParam;
    public FMODUnity.ParamRef dashSpeedParam;

    private void OnEnable()
    {
        PlayerController.OnDashStart += OnDashStart;
    }

    private void OnDisable()
    {
        PlayerController.OnDashStart -= OnDashStart;
    }

    private void OnDashStart(object sender, EventArgs e)
    {
        float dashInvicibilityParamValue = 0;
        if (playerController.isInvincibleDuringDash)
            dashInvicibilityParamValue = 1;

        emitter.Play(dashStartEvent);
    }
}
