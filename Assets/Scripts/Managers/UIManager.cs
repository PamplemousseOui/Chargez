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
        retryButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameRetry += OnGameStart;
        GameManager.OnPlayerDeath += OnPlayerDeath;
        startButton.onClick.AddListener(OnStartButtonPressed);
        retryButton.onClick.AddListener(OnRetryButtonPressed);
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameRetry -= OnGameStart;
        GameManager.OnPlayerDeath -= OnPlayerDeath;
        startButton.onClick.RemoveListener(OnStartButtonPressed);
        retryButton.onClick.RemoveListener(OnRetryButtonPressed);
    }

    private void OnPlayerDeath()
    {
        deathText.gameObject.SetActive(true);
        retryButton.gameObject.SetActive(true);
    }


    private void OnGameStart()
    {
        retryButton.gameObject.SetActive(false);
        startButton.gameObject.SetActive(false);
        deathText.gameObject.SetActive(false);
    }
    
    private void OnRetryButtonPressed()
    {
        GameManager.instance.RetryGame();
    }

    private void OnStartButtonPressed()
    {
        GameManager.instance.StartGame();
    }
}
