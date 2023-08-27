using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    public List<UIBonus> bonuses;

    public void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void DrawBonus(List<IBonus> _bonuses)
    {
        
    }
}
