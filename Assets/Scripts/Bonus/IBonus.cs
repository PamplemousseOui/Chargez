using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class IBonus : ScriptableObject
{
    public string name;
    public Sprite sprite;
    public string description;

    public abstract void ApplyEffect();
}
