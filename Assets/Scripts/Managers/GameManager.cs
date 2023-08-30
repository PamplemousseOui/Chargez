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

    public static Action OnPlayerDeath;
    public static Action OnGameStart;
    public static Action OnGamePause;
    public static Action OnGameResume;
    public static Action OnGameRetry;
    public static Action OnEnemiesFreezeStart;
    public static Action OnEnemiesFreezeStop;

    private float m_curEnemiesFreezeTime;

    private void OnEnable()
    {
        PlayerController.OnDamageReceived += OnPlayerHit;
    }

    private void OnDisable()
    {
        PlayerController.OnDamageReceived -= OnPlayerHit;
    }

    private void OnPlayerHit(float _healthRatio, float _damage)
    {
        canUpdateEnemies = false;
        m_curEnemiesFreezeTime = 0;
        OnEnemiesFreezeStart?.Invoke();
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
            OnEnemiesFreezeStop?.Invoke();
        }
    }

    public void StartGame()
    {
        OnGameStart?.Invoke();
        gameIsPaused = false;
    }

    public void RetryGame()
    {
        gameIsPaused = true;
        OnGameRetry?.Invoke();
    }

    public void PauseGame()
    {
        gameIsPaused = true;
        OnGamePause?.Invoke();
    }

    public void ResumeGame()
    {
        gameIsPaused = false;
        OnGameResume?.Invoke();
    }
}
