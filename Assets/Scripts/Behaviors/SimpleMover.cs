using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMover : MonoBehaviour
{
    public MoveDirection moveDirection;
    public float baseMoveSpeed;
    public Rigidbody2D rb;

    private float m_curMoveSpeed;

    private void Start()
    {
        m_curMoveSpeed = baseMoveSpeed;
    }

    private void FixedUpdate()
    {
        Vector2 direction = Vector2.zero;

        switch (moveDirection)
        {
            case MoveDirection.Foward:
                direction = transform.up.normalized * m_curMoveSpeed;
                break;
        }

        rb.velocity = direction;
    }
}
