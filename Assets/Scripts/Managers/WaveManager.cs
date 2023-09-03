using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static WaveManager;
using Random = UnityEngine.Random;

public class WaveManager : MonoBehaviour
{
    [SerializeField] 
    private List<WaveData> waves;
    [SerializeField]
    private List<IBonusData> bonuses;
    [SerializeField]
    private MenuManager menuManager;

    private int currentWaveNumber = 0;
    private WaveData currentWaveData;
    
    private List<WaveData.NumberOfEnemyByType> remainingEnemiesInWave; 
    private List<IBonusData> remainingBonuses;

    private float timerBeforeSpawn = 0.0f;

    public delegate void StartNewWaveEvent(WaveData waveData);
    public static StartNewWaveEvent OnStartNewWave;

    public delegate void SimpleEvent();
    public static SimpleEvent OnEndWaveEvent;
    public static SimpleEvent OnGameWinEvent;

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameRetry += OnGameStart;
        UIBonus.OnSelectBonus += OnSelectBonus;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameRetry -= OnGameStart;
        UIBonus.OnSelectBonus -= OnSelectBonus;
    }

    private void OnGameStart()
    {
        if (waves.Count == 0) Debug.LogError("No wave in data");
        else
        {
            remainingBonuses = new List<IBonusData>(bonuses);
            foreach (IBonusData bonusData in remainingBonuses)
            {
                bonusData.Reset();
            }
            
            currentWaveNumber = 0;
            StartNextWave();
        }
    }
    
    void Update()
    {
        if (currentWaveData == null || GameManager.gameIsPaused) return;
        
        if (remainingEnemiesInWave.Count == 0 && EnemyManager.nbEnemies <= 0)
        {
            EndOfWave();
        }
        else if(remainingEnemiesInWave.Count > 0)
        {
            timerBeforeSpawn -= Time.deltaTime;
            if (timerBeforeSpawn <= 0.0f)
            {
                SpawnEnemy();
            }
        }
    }

    private void StartNextWave()
    {
        if (currentWaveNumber >= waves.Count)
        {
            return;
        }
        currentWaveData = waves[currentWaveNumber];
        remainingEnemiesInWave = new List<WaveData.NumberOfEnemyByType>();
        foreach (var enemyInWave in currentWaveData.numberOfEnemyByTypes)
        {
            remainingEnemiesInWave.Add(enemyInWave.Clone());
        }
        currentWaveNumber++;
        timerBeforeSpawn = currentWaveData.startCooldown;
        menuManager.BeginWave(currentWaveNumber);
        OnStartNewWave?.Invoke(currentWaveData);
        GameManager.instance.ResumeGame();
        Debug.Log("Start wave " + currentWaveNumber);
    }

    private void EndOfWave()
    {
        currentWaveData = null;
        GameManager.instance.PauseGame();
        if (waves.Count > currentWaveNumber)
        {
            SelectBonuses();
            menuManager.EndWave();
            OnEndWaveEvent?.Invoke();
        }
        else
        {
            menuManager.WinGame();
            OnGameWinEvent?.Invoke();
        }
    }
    
    private void OnSelectBonus(IBonusData _bonusdata)
    {
        menuManager.HideBonus(_bonusdata);
        StartNextWave();
    }

    private void SpawnEnemy()
    {
        WaveData.NumberOfEnemyByType enemy = SelectEnemyTypeToSpawn();
        
        --enemy.number;
        if (enemy.number <= 0)
        {
            remainingEnemiesInWave.Remove(enemy);
        }
        
        SpawnManager.instance.SpawnEnemyType(enemy.type);

        timerBeforeSpawn = Random.Range(enemy.minCooldown, enemy.maxCooldown);

    }

    private WaveData.NumberOfEnemyByType SelectEnemyTypeToSpawn()
    {
        float totalWeight = 0;
        foreach (var weightedElement in remainingEnemiesInWave)
        {
            totalWeight += weightedElement.number;
        }

        float randomValue = Random.Range(0f, totalWeight);
        foreach (var weightedElement in remainingEnemiesInWave)
        {
            if (randomValue < weightedElement.number)
            {
                return weightedElement;
            }
            randomValue -= weightedElement.number;
        }
        
        return remainingEnemiesInWave[^1];
    }

    private void SelectBonuses()
    {
        remainingBonuses.RemoveAll(x => x.currentNumber <= 0);
        menuManager.DrawBonus(GetRandomBonuses(3));
    }
    
    public List<IBonusData> GetRandomBonuses(int count)
    {
        List<IBonusData> selectedElements = new List<IBonusData>();
        List<IBonusData> availableElements = new List<IBonusData>(remainingBonuses);

        while (selectedElements.Count < count && availableElements.Count > 0)
        {
            float totalWeight = 0;
            foreach (var weightedElement in availableElements)
            {
                totalWeight += weightedElement.currentNumber;
            }

            float randomValue = Random.Range(0f, totalWeight);

            foreach (var weightedElement in availableElements)
            {
                if (randomValue < weightedElement.currentNumber)
                {
                    selectedElements.Add(weightedElement);
                    availableElements.Remove(weightedElement);
                    break;
                }
                randomValue -= weightedElement.currentNumber;
            }

        }

        return selectedElements;
    }
}
