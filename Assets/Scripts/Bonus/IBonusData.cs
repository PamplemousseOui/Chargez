using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Bonus/empty")]
public class IBonusData : ScriptableObject
{
    public string name;
    public Sprite sprite;
    public string description;
    public int number = 1;
    private int m_currentNumber;
    public int currentNumber => m_currentNumber;

    public virtual void ApplyEffect()
    {
        --m_currentNumber;
        Debug.Log("Apply effect " + name);
    }

    public void Reset()
    {
        m_currentNumber = number;
    }
}
