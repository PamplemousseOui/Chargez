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
    public CinemachineVirtualCamera virtualCamera;

    private CinemachineBasicMultiChannelPerlin m_noise;

    private void Start()
    {
        m_noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public IEnumerator ScreenShake()
    {
        float elapsed = 0f;

        while (elapsed < sustain)
        {
            yield return new WaitForEndOfFrame();
            elapsed += Time.deltaTime * Time.timeScale;
            m_noise.m_AmplitudeGain = intensity / ExtensionMethods.MapValueRange(Time.timeScale, 0.01f, 1f, 0.1f, 1f);
            m_noise.m_FrequencyGain = intensity / ExtensionMethods.MapValueRange(Time.timeScale, 0.01f, 1f, 0.1f, 1f);
        }

        elapsed = 0f;

        while (elapsed < release)
        {
            yield return new WaitForEndOfFrame();
            elapsed += Time.deltaTime * Time.timeScale;

            m_noise.m_AmplitudeGain = intensity * (1 - elapsed / release) / Time.timeScale;
            m_noise.m_FrequencyGain = intensity * (1 - elapsed / release) / Time.timeScale;
        }

        yield return null;
    }
}
