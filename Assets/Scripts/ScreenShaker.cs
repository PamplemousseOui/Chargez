using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ScreenShaker : MonoBehaviour
{
    public float intensity;
    public float sustain;
    public float release;
    private float m_shakeElapsed;
    public CinemachineVirtualCamera virtualCamera;

    private CinemachineBasicMultiChannelPerlin m_noise;

    private void Start()
    {
        m_noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        m_shakeElapsed = -1;
    }

    private void Update()
    {
        if (m_shakeElapsed < 0.0f) return; 
        
        if (m_shakeElapsed < sustain)
        {
            m_shakeElapsed += Time.deltaTime;
            m_noise.m_AmplitudeGain = intensity;
            m_noise.m_FrequencyGain = intensity;
        }
        else if (m_shakeElapsed < release + sustain)
        {
            m_shakeElapsed += Time.deltaTime;

            m_noise.m_AmplitudeGain = intensity * (1 - m_shakeElapsed / release);
            m_noise.m_FrequencyGain = intensity * (1 - m_shakeElapsed / release);
        }
        else
        {
            m_noise.m_AmplitudeGain = 0.0f;
            m_noise.m_FrequencyGain = 0.0f;
        }
    }

    public void ScreenShake()
    {
        m_shakeElapsed = 0f;
    }
}
