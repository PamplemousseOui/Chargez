using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBonus : MonoBehaviour
{
    public delegate void SelectBonus(IBonusData _bonusData);
    public static SelectBonus OnSelectBonus;
    
    public IBonusData bonusData;
    
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI description;
    
    private Button m_button;
    private bool m_selected;
    private Animator m_animator;
    public Animator animator => m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
        m_button = GetComponent<Button>();
    }
    private void Update()
    {
        if(EventSystem.current.currentSelectedGameObject == m_button.gameObject)
        {
            if (!m_selected)
            {
                m_selected = true;
                m_animator.SetTrigger("Select");
            }
        }
        else if(m_selected)
        {
            m_selected = false;
            m_animator.SetTrigger("Deselect");
        }
    }

    public void SetBonus(IBonusData _data)
    {
        bonusData = _data;
        name.text = bonusData.name;
        description.text = bonusData.description;
        image.sprite = bonusData.sprite;
        m_animator.SetTrigger("Appears");
        
    }

    public void Select()
    {
        bonusData.ApplyEffect();
        OnSelectBonus?.Invoke(bonusData);
        Debug.Log("Select bonus " + name.text);
        m_animator.SetTrigger("Choose");
    }
}
