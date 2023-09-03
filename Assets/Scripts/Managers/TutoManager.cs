using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class TutoManager : MonoBehaviour
{
    public List<GameObject> tutorials;
    public int currentTutoriel;
    public CinemachineVirtualCamera camera;
    
    public void StartGame()
    {
        GameManager.instance.waveManager.gameObject.SetActive(true);
        GameManager.instance.StartGame();
        camera.Priority = 0;
    }

    public void NextTuto()
    {
        tutorials[currentTutoriel].gameObject.SetActive(false);
        ++currentTutoriel;
        if (currentTutoriel == tutorials.Count)
        {
            StartGame();
        }
        else tutorials[currentTutoriel].gameObject.SetActive(true);
    }
}
