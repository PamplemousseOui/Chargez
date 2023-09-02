using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HitscanSound : MonoBehaviour
{
    public HitScanComponent hitscan;
    public HealthComponent health;
    public SoundEmitter emitter;

    public EventReference loadStartEvent;
    public EventReference loadEndEvent;
    public EventReference shootStartEvent;
    public EventReference shootLoopEvent;

    private void OnEnable()
    {
        hitscan.OnLoadingStart += OnLoadingStart;
        hitscan.OnLoadingEnding += OnLoadingEnding;
        hitscan.OnShootStart += OnShootStart;
        hitscan.OnShootEnd += OnShootEnd;

        health.OnDeath += OnDeath;
    }

    private void OnDisable()
    {
        hitscan.OnLoadingStart -= OnLoadingStart;
        hitscan.OnLoadingEnding -= OnLoadingEnding;
        hitscan.OnShootStart -= OnShootStart;
        hitscan.OnShootEnd -= OnShootEnd;

        health.OnDeath -= OnDeath;
    }

    private void OnLoadingStart()
    {
        emitter.PlayAndForget(loadStartEvent);
    }

    private void OnLoadingEnding()
    {
        emitter.PlayAndForget(loadEndEvent);
    }

    private void OnShootStart()
    {
        emitter.Stop(loadStartEvent);
        emitter.PlayOneShot(shootStartEvent);
        emitter.Play(shootLoopEvent);
    }

    private void OnShootEnd()
    {
        emitter.Stop(shootLoopEvent);
    }

    private void OnDeath()
    {
        emitter.StopAll();
    }
}
