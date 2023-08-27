using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemySpawnData
{
    public bool allowSpawn;
    public EnemyType type;
    public GameObject enemyPrefab;
    public float spawnProbability;
    public uint countToSpawn;
    public float minSpawnTime;
    public float maxSpawnTime;
    public float curSpawnTimerValue { get; private set; }
    public float curSpawnTargetTime { get; private set; }

    private List<GameObject> m_spawnedEnemies = new List<GameObject>();
    private uint m_curSpawnCount;

    public void InitSpawnTimer()
    {
        curSpawnTimerValue = 0;
        curSpawnTargetTime = UnityEngine.Random.Range(minSpawnTime, maxSpawnTime);
    }

    public bool UpdateSpawnTimer()
    {
        if (m_curSpawnCount < countToSpawn && allowSpawn)
        {
            curSpawnTimerValue += Time.deltaTime;
            if (curSpawnTimerValue > curSpawnTargetTime)
            {
                if (UnityEngine.Random.value < spawnProbability)
                {
                    InitSpawnTimer();
                    m_curSpawnCount++;
                    return true;
                }
                return false;
            }
            else
                return false;
        }
        else
            return false; 
    }

    public void AddSpawnedEnemy(GameObject enemy)
    {
        m_spawnedEnemies.Add(enemy);
    }

    public void RemoveSpawnedEnemy(GameObject enemy)
    {
        m_spawnedEnemies.Remove(enemy);
    }

    public void ResetSpawnCounter()
    {
        m_curSpawnCount = 0;
    }
}

public class SpawnManager : MonoBehaviour
{
    public Vector2 arenaSize;
    public List<EnemySpawnData> enemiesSpawnData;

    public static EventHandler<EnemyType> OnEnemySpawn;

    public static SpawnManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        foreach (EnemySpawnData data in enemiesSpawnData)
        {
            data.InitSpawnTimer();
        }
    }

    private void Update()
    {
        if (!GameManager.gameIsPaused && GameManager.player.healthComponent.isAlive)
        {
            foreach (EnemySpawnData data in enemiesSpawnData)
            {
                if(data.UpdateSpawnTimer())
                    SpawnEnemy(data);
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
                        y = arenaSize.y / 2;
                        break;
                    case 1:
                        x = arenaSize.x / 2;
                        y = UnityEngine.Random.Range(0, arenaSize.y) - (arenaSize.y / 2);
                        break;
                    case 2:
                        x = UnityEngine.Random.Range(0, arenaSize.x) - (arenaSize.x / 2);
                        y = -arenaSize.y / 2;
                        break;
                    case 3:
                        x = -arenaSize.x / 2;
                        y = UnityEngine.Random.Range(0, arenaSize.y) - (arenaSize.y / 2);
                        break;
                }
                break;
            case EnemyType.Archer:
                x = UnityEngine.Random.Range(0, arenaSize.x) - (arenaSize.x / 2);
                y = UnityEngine.Random.Range(0, arenaSize.y) - (arenaSize.y / 2);
                break;
            case EnemyType.Wall:
                pickedWallIndex = UnityEngine.Random.Range(0, 4); //0 = upper then clockwise;
                switch (pickedWallIndex)
                {
                    case 0:
                        spawnedEnemy.transform.localScale = new Vector3(arenaSize.x, 1, 1);
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
                        x = 0;
                        y = -arenaSize.y / 2 - 2;
                        break;
                    case 1:
                        spawnedEnemy.transform.localScale = new Vector3(arenaSize.y, 1, 1);
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
                        x = arenaSize.x / 2 - 2;
                        y = 0;
                        break;
                    case 2:
                        spawnedEnemy.transform.localScale = new Vector3(arenaSize.x, 1, 1);
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        x = 0;
                        y = -arenaSize.y / 2 + 2;
                        break;
                    case 3:
                        spawnedEnemy.transform.localScale = new Vector3(arenaSize.y, 1, 1);
                        spawnedEnemy.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -90));
                        x = arenaSize.x / 2 + 2;
                        y = 0;
                        break;
                }
                break;
        }

        pos = new Vector3(x, y, 0);
        spawnedEnemy.transform.position = pos;
    }

    public void SpawnEnemyType(EnemyType _type)
    {
        foreach (EnemySpawnData enemySpawnData in enemiesSpawnData)
        {
            if (enemySpawnData.type == _type)
            {
                SpawnEnemy(enemySpawnData);
            }
        }
    }
}
