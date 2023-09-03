using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static int nbEnemies = 0;
    public static int nbRealEnemies = 0;
    [SerializeField] public int m_nbEnemies = 0;
    [SerializeField] public int m_nbRealEnemies = 0;

    public void Update()
    {
        m_nbEnemies = nbEnemies;
        m_nbRealEnemies = nbRealEnemies;
    }
}
