using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModifiers : MonoBehaviour
{
    public List<Modifier> modifiers = new List<Modifier>();
    public float GetModifierValue(string _name)
    {
        var modifier = modifiers.Find(x => x.name == _name);
        return modifier?.value ?? 0.0f;
    }

    private void OnEnable()
    {
        GameManager.OnGameRetry += OnGameRetry;
    }

    private void OnDisable()
    {
        GameManager.OnGameRetry -= OnGameRetry;
    }

    private void OnGameRetry()
    {
        Init();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        modifiers = new List<Modifier>();
    }
}
