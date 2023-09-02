using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    public float baseRotationSpeed = 180.0f;

    public void Update()
    {
        transform.Rotate(Vector3.forward, baseRotationSpeed * Time.deltaTime);
    }

    public void SetRotation(float _rotation)
    {
        transform.localRotation = Quaternion.Euler(0.0f, 0.0f, _rotation);
    }
}
