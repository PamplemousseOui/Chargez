using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    public Tag tagToCheck;
    public List<GameObject> objectsInRange = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log($"GameObject {collision.gameObject.name} entered range.");
        if (collision.tag == tagToCheck.ToString())
        {
            objectsInRange.Add(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Debug.Log($"GameObject {collision.gameObject.name} exited range.");
        if (collision.tag == tagToCheck.ToString())
        {
            objectsInRange.Remove(collision.gameObject);
        }
    }
}
