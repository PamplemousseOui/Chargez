using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleLookAt : MonoBehaviour
{
    public float baseRotationSpeed = 1f;

    private float m_curRotSpeed;
    private bool m_canRotate;

    private void Start()
    {
        m_curRotSpeed = baseRotationSpeed;
        m_canRotate = true;
    }

    private void FixedUpdate()
    {
        if (!GameManager.gameIsPaused)
        {
            if (GameManager.player != null)
            {
                if (GameManager.player.healthComponent.isAlive && m_canRotate)
                {
                    Vector3 direction = GameManager.player.transform.position - transform.position;
                    transform.up = Vector3.Lerp(transform.up, direction, m_curRotSpeed * 0.01f);
                }
            }
        }
    }

    public void SetCanRotate(bool canRotate)
    {
        m_canRotate = canRotate;
    }
}
