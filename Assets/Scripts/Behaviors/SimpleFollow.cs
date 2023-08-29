using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SimpleFollow : MonoBehaviour
{
    public float baseSpeed = 1f;
    private float m_curSpeed;
    private Rigidbody2D m_rigidbody;
    public bool freezeOnPlayerHit;

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();
    }

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
                    if ((freezeOnPlayerHit && GameManager.canUpdateEnemies) || !freezeOnPlayerHit)
                        m_rigidbody.velocity = transform.up * m_curSpeed;
                    else
                        m_rigidbody.velocity = Vector2.zero;
                }
                else
                    m_rigidbody.velocity = Vector2.zero;
            }
            else
                m_rigidbody.velocity = Vector2.zero;
        }
        else
        {
            m_rigidbody.velocity = Vector2.zero;
        }
    }
}
