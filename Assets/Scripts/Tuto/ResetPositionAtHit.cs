using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPositionAtHit : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRot;
    public void OnEnable()
    {
        originalPosition = GameManager.player.transform.position;
        originalRot = GameManager.player.transform.rotation;

        PlayerController.OnDamageReceived += ResetCharacterPosition;
    }
    public void OnDisable()
    {
        PlayerController.OnDamageReceived -= ResetCharacterPosition;
    }


    private void ResetCharacterPosition(float arg1, float arg2)
    {
        GameManager.player.transform.position = originalPosition;
        GameManager.player.transform.rotation = originalRot;
    }
}
