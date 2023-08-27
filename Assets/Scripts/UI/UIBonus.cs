using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBonus : MonoBehaviour
{
    public IBonus bonusData;
    
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI description;

    public void SetBonus(IBonus _data)
    {
        bonusData = _data;
        name.text = bonusData.name;
        description.text = bonusData.description;
        image.sprite = bonusData.sprite;
    }

    public void Select()
    {
        Debug.Log("Select bonus " + name.text);
    }
}
