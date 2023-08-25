using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLookAt : MonoBehaviour
{
    private void FixedUpdate()
    {
        if (!GameManager.gameIsPaused)
        {
            if (GameManager.player != null)
            {
                if (GameManager.player.healthComponent.isAlive)
                {
                    Vector3 direction = GameManager.player.transform.position - transform.position;
                    transform.up = direction;
                }
            }
        }
    }
}
