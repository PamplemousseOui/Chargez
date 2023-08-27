using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBonus : MonoBehaviour
{
    public delegate void SelectBonus(IBonusData _bonusData);
    public static SelectBonus OnSelectBonus;
    
    public IBonusData bonusData;
    
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI description;

    public void SetBonus(IBonusData _data)
    {
        bonusData = _data;
        name.text = bonusData.name;
        description.text = bonusData.description;
        image.sprite = bonusData.sprite;
    }

    public void Select()
    {
        bonusData.ApplyEffect();
        OnSelectBonus?.Invoke(bonusData);
        Debug.Log("Select bonus " + name.text);
    }
}
