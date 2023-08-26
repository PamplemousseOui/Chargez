using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SimpleFollow : MonoBehaviour
{
    public float baseSpeed = 1f;
    private float m_curSpeed;

    private void Start()
    {
        m_curSpeed = baseSpeed;
    }

    private void FixedUpdate()
    {
        if (!GameManager.gameIsPaused)
        {
            if (GameManager.player != null)
            {
                if (GameManager.player.healthComponent.isAlive)
                {
                    Vector3 direction = (GameManager.player.transform.position - transform.position).normalized * m_curSpeed * 0.01f;
                    gameObject.transform.position += direction;
                }
            }
        }
    }
}
