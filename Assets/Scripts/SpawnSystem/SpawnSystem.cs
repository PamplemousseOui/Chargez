using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    public EnemyType type;
    public GameObject enemy;

    private void Start()
    {
        EnemyManager.nbEnemies++;
    }

    private void OnDestroy()
    {
        EnemyManager.nbEnemies--;
    }
    
    public void SpawneEnemy()
    {
        GameObject spawnedEnemy = Instantiate(enemy, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
