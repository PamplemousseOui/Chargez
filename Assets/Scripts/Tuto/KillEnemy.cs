using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KillEnemy : MonoBehaviour
{
    public UnityEvent reachEvent;
    public bool reached = false;
    public void Update()
    {
        if (!reached && EnemyManager.nbEnemies == 0)
        {
            reached = true;
            reachEvent?.Invoke();
        }
    }
}
