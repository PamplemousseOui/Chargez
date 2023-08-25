using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI deathText;

    private void Start()
    {
        deathText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.OnPlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath(object sender, EventArgs e)
    {
        deathText.gameObject.SetActive(true);
    }
}
