using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootmanSound : EnemySound
{
    public EventReference moveEvent;

    private void Start()
    {
        emitter.Play(moveEvent);
    }

    public override void OnDeath()
    {
        base.OnDeath();
        emitter.Stop(moveEvent);
    }
}
