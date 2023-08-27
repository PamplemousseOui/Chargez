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

    public void DrawBonus(List<IBonusData> _bonuses)
    {
        foreach (UIBonus bonus in bonuses)
        {
            bonus.gameObject.SetActive(false);
        }
        for (int i = 0; i < bonuses.Count && i < _bonuses.Count; ++i)
        {
            bonuses[i].SetBonus(_bonuses[i]);
            bonuses[i].gameObject.SetActive(true);
        }
        animator.SetTrigger("ShowBonus");
    }

    public void HideBonus()
    {
        animator.SetTrigger("HideBonus");
    }
}
