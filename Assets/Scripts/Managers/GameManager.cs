using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static PlayerController player;
    public static GameManager instance;
    public static bool gameIsPaused;

    public SpawnManager spawnManager;
    public WaveManager waveManager;
    public ScoreManager scoreManager;

    public static EventHandler OnPlayerDeath;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }
}
