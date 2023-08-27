using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    public Button button;

    private void Start()
    {
        button.onClick.AddListener(delegate { OnButtonPressed(); });
    }

    private void OnButtonPressed()
    {
        GameManager.instance.StartGame();
    }
}
