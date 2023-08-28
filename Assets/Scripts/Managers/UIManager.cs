using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI deathText;
    public Button startButton;
    public Button retryButton;

    private void Start()
    {
        deathText.gameObject.SetActive(false);
        startButton.onClick.AddListener(delegate { OnStartButtonPressed(); });
        retryButton.onClick.AddListener(delegate { OnRetryButtonPressed(); });
        retryButton.gameObject.SetActive(false);
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
        retryButton.gameObject.SetActive(true);
    }

    private void OnRetryButtonPressed()
    {
        GameManager.instance.RetryGame();
        retryButton.gameObject.SetActive(false);
        deathText.gameObject.SetActive(false);
    }

    private void OnStartButtonPressed()
    {
        GameManager.instance.StartGame();
        startButton.gameObject.SetActive(false);
    }
}
