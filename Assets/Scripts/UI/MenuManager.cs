using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private Animator m_animator;
    public List<UIBonus> bonuses;
    public GameObject retryWinGame;
    public GameObject retryLooseGame;
    public GameObject resumeGame;
    public TextMeshProUGUI waveNumber;
    public TextMeshProUGUI gameOverScore;
    
    public void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void OnEnable()
    {
        GameManager.OnPlayerDeath += LooseGame;
        GameManager.OnGameResume += ResumeGame;
        InputManager.OnPausePress += PauseGame;
    }

    public void OnDisable()
    {
        GameManager.OnPlayerDeath -= LooseGame;
        GameManager.OnGameResume -= ResumeGame;
        InputManager.OnPausePress -= PauseGame;
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
        StartCoroutine(SelectGameObject(bonuses[0].gameObject));
    }

    private IEnumerator SelectGameObject(GameObject _gameObject)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForSeconds(0.3f);
        EventSystem.current.SetSelectedGameObject(_gameObject);
    }
    public void HideBonus(IBonusData _selected)
    {
        m_animator.SetTrigger("HideBonus");
        foreach (UIBonus bonus in bonuses)
        {
            if(bonus.gameObject.activeSelf && _selected != bonus.bonusData) bonus.animator.SetTrigger("UnChoose");
        }
    }

    public void BeginWave(int _waveNumber)
    {
        waveNumber.text = (_waveNumber < 10 ? "0" : "") + _waveNumber;
        m_animator.SetTrigger("BeginWave");
        
    }

    public void EndWave()
    {
        m_animator.SetTrigger("EndWave");
    }

    public void WinGame()
    {
        m_animator.SetTrigger("EndWave");
        m_animator.SetTrigger("GameWin");
        StartCoroutine(SelectGameObject(retryWinGame));
    }

    public void LooseGame()
    {
        gameOverScore.text = waveNumber.text;
        m_animator.SetTrigger("EndWave");
        m_animator.SetTrigger("GameOver");
        StartCoroutine(SelectGameObject(retryLooseGame));
    }

    private bool pause = false;
    public void PauseGame()
    {
        Debug.Log("escape input");

        if (!pause)
        {
            if (GameManager.gameIsPaused) return;
            pause = true;
            GameManager.instance.PauseGame();
            m_animator.SetTrigger("GamePause");
            StartCoroutine(SelectGameObject(resumeGame));
        }
        else
        {
            GameManager.instance.ResumeGame();
            pause = false;
        }
    }

    private void ResumeGame()
    {
        if (!pause) return;
        m_animator.SetTrigger("GameResume");
        pause = false;
    }

    public void OnResumeGameButtonPress()
    {
        GameManager.instance.ResumeGame();
    }
    
    public void OnRestartGameButtonPress()
    {
        GameManager.instance.RetryGame();
    }
}
