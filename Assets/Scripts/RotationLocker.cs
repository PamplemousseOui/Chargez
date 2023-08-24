using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationLocker : MonoBehaviour
{
    private void Update()
    {
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }
}
