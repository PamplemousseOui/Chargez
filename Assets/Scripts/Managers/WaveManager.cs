using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] 
    private List<WaveData> waves;
    [SerializeField]
    private List<IBonus> bonuses;
    [SerializeField]
    private MenuManager menuManager;

    private int currentWaveNumber = 0;
    private WaveData currentWaveData;
    private List<WaveData.NumberOfEnemyByType> remainingEnemiesInWave; 
    private List<IBonus> remainingBonuses; 
    
    void Start()
    {
        if (waves.Count == 0) Debug.LogError("No wave in data");
        else
        {
            remainingBonuses = new List<IBonus>(bonuses);
            
            currentWaveNumber = 0;
            StartNextWave();
        }
    }
    
    void Update()
    {
        if (currentWaveData != null && remainingEnemiesInWave.Count == 0 && SpawnManager.instance.enemies.Count == 0)
        {
            EndOfWave();
        }
    }

    private void EndOfWave()
    {
        currentWaveData = null;
        
    }

    private void StartNextWave()
    {
        if (currentWaveNumber >= waves.Count)
        {
            return;
        }
        currentWaveData = waves[currentWaveNumber];
        remainingEnemiesInWave = new List<WaveData.NumberOfEnemyByType>(currentWaveData.numberOfEnemyByTypes);
        currentWaveNumber++;
        Debug.Log("Start wave " + currentWaveNumber);
    }
}
