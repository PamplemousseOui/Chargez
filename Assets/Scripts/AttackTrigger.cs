using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    public Tag tagToCheck;
    public float maxAngle;
    private List<EnemyComponent> m_enemiesInRange = new List<EnemyComponent>();

    public List<EnemyComponent> enemiesInRange
    {
        get
        {
            List<EnemyComponent> enemiesInQuadrant = new List<EnemyComponent>();
            foreach (EnemyComponent enemy in m_enemiesInRange)
            {
                var dir = (enemy.transform.position - transform.position).normalized;
                var dotDir = Vector2.Dot(transform.up, dir);
                //Debug.Log(dotDir + " - " + math.cos(maxAngle * math.PI/180.0f));
                
                if(dotDir > math.cos(maxAngle * math.PI/180.0f)) enemiesInQuadrant.Add(enemy);
            }
            return enemiesInQuadrant;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(tagToCheck.ToString()))
        {
            if (collision.TryGetComponent<EnemyComponent>(out var enemy) && collision.TryGetComponent(out HealthComponent healthComponent))
            {
                m_enemiesInRange.Add(enemy);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(tagToCheck.ToString()))
        {
            if (collision.TryGetComponent<EnemyComponent>(out var enemy) && collision.TryGetComponent(out HealthComponent healthComponent))
            {
                m_enemiesInRange.Remove(enemy);
            }
        }
    }

    public void ResquestDestroy()
    {
        Destroy(gameObject);
    }

    public void ApplyAreaModifier(float _areaModifier)
    {
        transform.localScale *= (1.0f + _areaModifier);
    }

    public void ApplyDurationModifier(float _durationModifier)
    {
        StartCoroutine(StopAttackCoroutine(_durationModifier));
    }

    public IEnumerator StopAttackCoroutine(float _overTime)
    {
        yield return new WaitForSeconds(_overTime);
        GetComponent<Animator>().SetTrigger("Cancel");
    }
}
