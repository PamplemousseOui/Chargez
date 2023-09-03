using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReachTrigger : MonoBehaviour
{
    public UnityEvent reachEvent;

    private void OnTriggerEnter2D(Collider2D col)
    {
        reachEvent?.Invoke();
    }
}
