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
    public List<EnemyComponent> enemies;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            enemies = new List<EnemyComponent>();
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
        enemies = new List<EnemyComponent>();
    }

    private void Update()
    {
        enemies.RemoveAll(x => x == null);
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
                x = UnityEngine.Random.Range(0, arenaSize.x) - (arenaSize.x / 2 - 5);
                y = UnityEngine.Random.Range(0, arenaSize.y) - (arenaSize.y / 2 - 5);
                break;
            case EnemyType.Wall:
                pickedWallIndex = UnityEngine.Random.Range(0, 4); //0 = upper then clockwise;
                switch (pickedWallIndex)
                {
                    case 0:
                        spawnedEnemy.transform.localScale = new Vector3(arenaSize.x, 1, 1);
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0,0,-180));
                        x = 0;
                        y = arenaSize.y / 2 + 2;
                        break;
                    case 1:
                        spawnedEnemy.transform.localScale = new Vector3(arenaSize.y, 1, 1);
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                        x = arenaSize.x / 2 + 2;
                        y = 0;
                        break;
                    case 2:
                        spawnedEnemy.transform.localScale = new Vector3(arenaSize.x, 1, 1);
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        x = 0;
                        y = -arenaSize.y / 2 - 2;
                        break;
                    case 3:
                        spawnedEnemy.transform.localScale = new Vector3(arenaSize.y, 1, 1);
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
                        x = -arenaSize.x / 2 - 2;
                        y = 0;
                        break;
                }
                break;
        }

        pos = new Vector3(x, y, 0);
        spawnedEnemy.transform.position = pos;
        enemies.Add(spawnedEnemy.GetComponent<EnemyComponent>());
        OnEnemySpawned?.Invoke(enemySpawnData.type);
    }

    private void OnGameRetry()
    {
        enemies = new List<EnemyComponent>();
    }
}
