using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tag
{
    Player,
    Enemy,
    Projectile,
    Trigger
}

[Serializable]
public enum EnemyType
{
    Footman,
    Archer,
    Wall,
    Projectile
}

[Serializable]
public enum SpawnPosition
{
    Wall,
    Inside
}