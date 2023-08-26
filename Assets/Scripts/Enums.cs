using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Tag
{
    Player,
    Enemy,
    Projectile,
    Trigger,
    Arena
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

[Serializable]
public enum MoveDirection
{
    Foward,
    Backward,
    Left,
    Right,
}