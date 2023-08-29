using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SimpleLookAt : MonoBehaviour
{
    public float baseRotationSpeed = 1f;
    public bool freezeOnPlayerHit;

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
                    if ((GameManager.canUpdateEnemies && freezeOnPlayerHit) || !freezeOnPlayerHit)
                    {
                        Vector3 direction = GameManager.player.transform.position - transform.position;
                        transform.up = Vector3.RotateTowards(transform.up, direction, m_curRotSpeed * Time.deltaTime * math.PI / 180.0f, 0.0f);
                    }
                }
            }
        }
    }

    public void SetCanRotate(bool canRotate)
    {
        m_canRotate = canRotate;
    }
}
