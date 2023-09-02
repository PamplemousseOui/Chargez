using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Modifier
{
    public string name;
    public float value;

    public Modifier(Modifier _other)
    {
        name = _other.name;
        value = _other.value;
    }
}
