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
    public static EventHandler OnGameStart;
    public static EventHandler OnGamePause;
    public static EventHandler OnGameResume;
    public static EventHandler OnGameRetry;

    private void Awake()
    {
        gameIsPaused = true;

        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void Start()
    {
        StartGame();
    }
    
    public void StartGame()
    {
        OnGameStart?.Invoke(this, null);
        gameIsPaused = false;
    }

    public void RetryGame()
    {
        gameIsPaused = true;
        OnGameRetry?.Invoke(this, null);
    }

    public void PauseGame()
    {
        gameIsPaused = true;
        OnGamePause?.Invoke(this, null);
    }

    public void ResumeGame()
    {
        gameIsPaused = false;
        OnGameResume?.Invoke(this, null);
    }
}
