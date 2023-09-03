using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private Animator m_animator;
    public List<UIBonus> bonuses;
    
    public void Awake()
    {
        m_animator = GetComponent<Animator>();
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
            //bonuses[i].animator.SetTrigger("ShowBonus");
        }
        m_animator.SetTrigger("ShowBonus");
        EventSystem.current.SetSelectedGameObject(bonuses[0].gameObject);
    }

    public void HideBonus(IBonusData _selected)
    {
        m_animator.SetTrigger("HideBonus");
        foreach (UIBonus bonus in bonuses)
        {
            if(bonus.gameObject.activeSelf && _selected != bonus.bonusData) bonus.animator.SetTrigger("UnChoose");
        }
    }
}
