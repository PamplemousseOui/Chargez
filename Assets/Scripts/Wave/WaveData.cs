using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveData
{
    [Serializable]
    public class NumberOfEnemyByType
    {
        public EnemyType type;
        public int number = 1;
        public float minCooldown = 0.0f;
        public float maxCooldown = 5.0f;
        
        public NumberOfEnemyByType Clone()
        {
            NumberOfEnemyByType clone = new NumberOfEnemyByType();
            clone.type = type;
            clone.number = number;
            clone.minCooldown = minCooldown;
            clone.maxCooldown = maxCooldown;
            return clone;
        }
    }

    public string friendlyName = "Wave 0";
    public float startCooldown = 5.0f;
    public int maxEnemiesInArena = 5;
    public List<NumberOfEnemyByType> numberOfEnemyByTypes;

    
}
