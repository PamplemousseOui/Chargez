using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public List<GameObject> healtPoints = new List<GameObject>();
    public PlayerController player;
    public Sprite full;
    public Sprite empty;
    public Color fullColor;
    public Color emptyColor;

    private void OnEnable()
    {
        player.healthComponent.OnHealthUpdated += OnHealthUpdated;
    }

    private void OnDisable()
    {
        player.healthComponent.OnHealthUpdated -= OnHealthUpdated;
    }

    private void Start()
    {
        OnHealthUpdated(1,0);
    }

    private void OnHealthUpdated(float _newRatio, float _damages)
    {
        for (int i = 0; i < healtPoints.Count; i++)
        {
            if (i > (player.healthComponent.currentHealth - 1))
            {
                healtPoints[i].GetComponent<Image>().sprite = empty;
                healtPoints[i].GetComponent<Image>().color = emptyColor;
            }
            else
            {
                healtPoints[i].GetComponent<Image>().sprite = full;
                healtPoints[i].GetComponent<Image>().color = fullColor;
            }
        }
    }
}
