using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public bool isAudioMuted;

    private void Start()
    {
        if (isAudioMuted)
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Mute", 1);
    }
}
