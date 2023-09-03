using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TutoManager : MonoBehaviour
{
    public List<GameObject> tutorials;
    public int currentTutoriel;
    public CinemachineVirtualCamera camera;

    public void Awake()
    {
        foreach (var gameObject in tutorials)
        {
            gameObject.SetActive(false);
        }
        tutorials[0].gameObject.SetActive(true);
    }

    public void OnEnable()
    {
        InputManager.OnPausePress += StartGame;
    }

    public void OnDisable()
    {
        
        InputManager.OnPausePress -= StartGame;
    }

    public void StartGame()
    {
        GameManager.instance.waveManager.gameObject.SetActive(true);
        GameManager.instance.StartGame();
        camera.Priority = 0;
        Destroy(gameObject);
    }

    public void NextTuto()
    {
        Destroy(tutorials[currentTutoriel].gameObject);
        ++currentTutoriel;
        if (currentTutoriel == tutorials.Count)
        {
            StartGame();
        }
        else tutorials[currentTutoriel].gameObject.SetActive(true);
    }
}
