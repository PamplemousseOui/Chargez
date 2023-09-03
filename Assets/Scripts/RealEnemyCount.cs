using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealEnemyCount : MonoBehaviour
{
    private void Start()
    {
        EnemyManager.nbRealEnemies++;
    }

    private void OnDestroy()
    {
        EnemyManager.nbRealEnemies--;
    }
}
