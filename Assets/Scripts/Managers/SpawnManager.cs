using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnData
{
    public EnemyType type;
    public GameObject enemyPrefab;
}

public class SpawnManager : MonoBehaviour
{
    public Vector2 arenaSize;
    public List<EnemySpawnData> enemiesSpawnData;

    public static Action<EnemyType> OnEnemySpawned;

    public static SpawnManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(this);
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameRetry += OnGameStart;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameRetry -= OnGameStart;
    }
    
    private void OnGameStart()
    {
        
    }

    public void SpawnEnemyType(EnemyType _type)
    {
        foreach (EnemySpawnData enemySpawnData in enemiesSpawnData)
        {
            if (enemySpawnData.type == _type)
            {
                SpawnEnemy(enemySpawnData);
                return;
            }
        }
    }

    private void SpawnEnemy(EnemySpawnData enemySpawnData)
    {
        Vector3 pos = Vector3.zero;
        float x = 0;
        float y = 0;
        GameObject spawnedEnemy = Instantiate(enemySpawnData.enemyPrefab);

        switch (enemySpawnData.type)
        {
            case EnemyType.Footman:
                int pickedWallIndex = UnityEngine.Random.Range(0,4); //0 = upper then clockwise;
                switch (pickedWallIndex)
                {
                    case 0:
                        x = UnityEngine.Random.Range(0, arenaSize.x) - (arenaSize.x / 2);
                        y = arenaSize.y / 2 + 1;
                        break;
                    case 1:
                        x = arenaSize.x / 2 + 1;
                        y = UnityEngine.Random.Range(0, arenaSize.y) - (arenaSize.y / 2);
                        break;
                    case 2:
                        x = UnityEngine.Random.Range(0, arenaSize.x) - (arenaSize.x / 2);
                        y = -arenaSize.y / 2 - 1;
                        break;
                    case 3:
                        x = -arenaSize.x / 2 - 1;
                        y = UnityEngine.Random.Range(0, arenaSize.y) - (arenaSize.y / 2);
                        break;
                }
                break;
            case EnemyType.Archer:
                x = UnityEngine.Random.Range(2, arenaSize.x - 2) - (arenaSize.x / 2);
                y = UnityEngine.Random.Range(2, arenaSize.y - 2) - (arenaSize.y / 2);
                break;
            case EnemyType.Wall:
                pickedWallIndex = UnityEngine.Random.Range(0, 4); //0 = upper then clockwise;
                float size = 0.0f;
                switch (pickedWallIndex)
                {
                    case 0:
                        size = arenaSize.x;
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0,0,-180));
                        x = 0;
                        y = arenaSize.y / 2 + 2;
                        break;
                    case 1:
                        size = arenaSize.y;
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                        x = arenaSize.x / 2 + 2;
                        y = 0;
                        break;
                    case 2:
                        size = arenaSize.x;
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        x = 0;
                        y = -arenaSize.y / 2 - 2;
                        break;
                    case 3:
                        size = arenaSize.y;
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
                        x = -arenaSize.x / 2 - 2;
                        y = 0;
                        break;
                }
                spawnedEnemy.GetComponent<WallController>().SetSize(size);
                
                break;
        }

        pos = new Vector3(x, y, 0);
        spawnedEnemy.transform.position = pos;
        OnEnemySpawned?.Invoke(enemySpawnData.type);
    }

    public void DestroyAllEnemies()
    {
        foreach (EnemyComponent enemy in FindObjectsOfType<EnemyComponent>())
        {
            enemy.healthComponent.InstantKill();
        }
    }
}
