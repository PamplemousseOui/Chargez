using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public float baseRotationSpeed = 180.0f;
    private float m_currentAngle = 0.0f;

    public void Update()
    {
        m_currentAngle += baseRotationSpeed * Time.deltaTime;
        m_currentAngle %= 360.0f;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, m_currentAngle);
    }

    public void SetRotation(float _rotation)
    {
        m_currentAngle = _rotation;
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, _rotation);
    }
}
