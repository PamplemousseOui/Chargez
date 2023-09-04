using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    [SerializeField] private GameObject m_cavPrefab;
    [SerializeField] private float m_cavSize = 1.6f;
    [SerializeField] private float m_idleDuration = 1.0f;
    public bool bypassInstantiation;

    private float m_size;
    private bool m_isDestroyed;

    public Action OnWallDestroyed;
    
    public void SetSize(float _size)
    {
        m_size = _size;
    }

    private void Start()
    {
        if (bypassInstantiation) return;
        CavController previous = null;
        float waitingTime = 0.0f;
        for (float i = -m_size / 2.0f; i < m_size / 2.0f; i += m_cavSize)
        {
            var cavInstance = Instantiate(m_cavPrefab, transform);
            cavInstance.transform.localPosition = Vector3.right * i;

            CavController cav = cavInstance.GetComponent<CavController>();
            cav.wallController = this;
            if (previous)
            {
                cav.SetRight(previous);
                previous.SetLeft(cav);
            }
            cav.SetWaitingTime(waitingTime);
            
            waitingTime += 0.1f;
            waitingTime %= m_idleDuration;
            previous = cav;
        }
    }

    private void Update()
    {
        if(transform.childCount == 0) Destroy(gameObject);
    }
    
    public void DestroyWall()
    {
        if (m_isDestroyed) return;
        
        m_isDestroyed = true;
        OnWallDestroyed?.Invoke();
        
    }
}
