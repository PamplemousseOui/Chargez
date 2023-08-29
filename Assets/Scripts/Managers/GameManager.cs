using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static PlayerController player;
    public static GameManager instance;
    public static bool gameIsPaused;
    public static bool canUpdateEnemies;
    public float enemiesFreezeDelay;

    public SpawnManager spawnManager;
    public WaveManager waveManager;
    public ScoreManager scoreManager;

    public static EventHandler OnPlayerDeath;
    public static EventHandler OnGameStart;
    public static EventHandler OnGamePause;
    public static EventHandler OnGameResume;
    public static EventHandler OnGameRetry;

    private float m_curEnemiesFreezeTime;

    private void OnEnable()
    {
        PlayerController.OnPlayerHit += OnPlayerHit;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerHit -= OnPlayerHit;
    }

    private void OnPlayerHit()
    {
        canUpdateEnemies = false;
        m_curEnemiesFreezeTime = 0;
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public void Start()
    {
        m_curEnemiesFreezeTime = enemiesFreezeDelay;
        gameIsPaused = true;
        canUpdateEnemies = true;

        StartGame();
    }

    private void Update()
    {
        if (m_curEnemiesFreezeTime < enemiesFreezeDelay)
        {
            m_curEnemiesFreezeTime += Time.deltaTime;
        }
        else if (!canUpdateEnemies)
        {
            canUpdateEnemies = true;
        }
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
