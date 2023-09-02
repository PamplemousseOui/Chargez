using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Events;

public class SoundEmitter : MonoBehaviour
{
    private Dictionary<FMODUnity.EventReference, List<EventInstance>> m_playingEventsDict = new Dictionary<FMODUnity.EventReference, List<EventInstance>>();
    private Dictionary<FMODUnity.ParamRef, float> m_localParameters = new Dictionary<FMODUnity.ParamRef, float>();

    public void Play(FMODUnity.EventReference _fmodEvent)
    {
        CreateAndPlayInstance(_fmodEvent);
    }

    public void Play(FMODUnity.EventReference _fmodEvent, EVENT_CALLBACK_TYPE _type, Action<object[]> _callbackAction)
    {
        EventInstance eventInstance = CreateAndPlayInstance(_fmodEvent);
        SoundCallbackGetter.AddCallback(eventInstance, _type, _callbackAction);
    }

    public void PlayWithOffset(FMODUnity.EventReference _fmodEvent, int _offset)
    {
        CreateAndPlayInstance(_fmodEvent, _offset);
    }

    public void PlayWithOffset(FMODUnity.EventReference _fmodEvent, int _offset, EVENT_CALLBACK_TYPE _type, Action<object[]> _callbackAction)
    {
        EventInstance eventInstance = CreateAndPlayInstance(_fmodEvent, _offset);
        SoundCallbackGetter.AddCallback(eventInstance, _type, _callbackAction);
    }

    public void PlayAndForget(FMODUnity.EventReference _fmodEvent)
    {
        CreateAndPlayInstance(_fmodEvent).release();
    }

    public void PlayOneShot(FMODUnity.EventReference _fmodEvent)
    {
        FMODUnity.RuntimeManager.PlayOneShotAttached(_fmodEvent, gameObject);
    }

    public void Stop(FMODUnity.EventReference _fmodEvent)
    {
        List<EventInstance> instancesToStop;
        if (m_playingEventsDict.TryGetValue(_fmodEvent, out instancesToStop))
        {
            foreach (EventInstance eventInstance in instancesToStop)
            {
                StopAndClear(eventInstance);
            }
        }
    }

    public void SetParameter(FMODUnity.ParamRef _param, float _value, bool _ignoreseekspeed = false)
    {
        if (m_localParameters.ContainsKey(_param))
        {
            m_localParameters[_param] = _value;
        }
        else
        {
            m_localParameters.Add(_param, _value);
        }

        foreach (List<EventInstance> eventInstances in m_playingEventsDict.Values)
        {
            foreach (EventInstance instance in eventInstances)
            {
                instance.setParameterByName(_param.Name, _value, _ignoreseekspeed);
            }
        }
    }

    private EventInstance CreateAndPlayInstance(FMODUnity.EventReference _fmodEvent)
    {
        EventInstance eventInstanceToPlay = FMODUnity.RuntimeManager.CreateInstance(_fmodEvent);
        List<EventInstance> playingEvents;

        if (m_playingEventsDict.ContainsKey(_fmodEvent))
        {
            if (m_playingEventsDict.TryGetValue(_fmodEvent, out playingEvents))
            {
                playingEvents.Add(eventInstanceToPlay);
                InstanceStart(eventInstanceToPlay);
            }
        }
        else
        {
            playingEvents = new List<EventInstance> { eventInstanceToPlay };
            m_playingEventsDict.Add(_fmodEvent, playingEvents);
            InstanceStart(eventInstanceToPlay);
        }
        return eventInstanceToPlay;
    }

    private EventInstance CreateAndPlayInstance(FMODUnity.EventReference _fmodEvent, int _startOffset)
    {
        EventInstance eventInstanceToPlay = FMODUnity.RuntimeManager.CreateInstance(_fmodEvent);
        List<EventInstance> playingEvents;
        eventInstanceToPlay.setTimelinePosition(_startOffset);

        if (m_playingEventsDict.ContainsKey(_fmodEvent))
        {
            if (m_playingEventsDict.TryGetValue(_fmodEvent, out playingEvents))
            {
                playingEvents.Add(eventInstanceToPlay);
                InstanceStart(eventInstanceToPlay);
            }
        }
        else
        {
            playingEvents = new List<EventInstance> { eventInstanceToPlay };
            m_playingEventsDict.Add(_fmodEvent, playingEvents);
            InstanceStart(eventInstanceToPlay);
        }
        return eventInstanceToPlay;
    }

    private void InstanceStart(EventInstance _eventInstance)
    {
        foreach (FMODUnity.ParamRef param in m_localParameters.Keys)
        {
            float value;
            m_localParameters.TryGetValue(param, out value);
            _eventInstance.setParameterByName(param.Name, value);
        }
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(_eventInstance, gameObject.transform);
        _eventInstance.start();
        StartCoroutine(DestroyOnEnd(_eventInstance));
    }

    private IEnumerator DestroyOnEnd(EventInstance _instance)
    {
        EventDescription eventDescription;
        _instance.getDescription(out eventDescription);
        int delay;
        eventDescription.getLength(out delay);
        yield return new WaitForSeconds((float)delay / 1000f);
        PLAYBACK_STATE playbackState;
        _instance.getPlaybackState(out playbackState);
        if (playbackState == PLAYBACK_STATE.STOPPED)
        {
            StopAndClear(_instance);
        }
    }

    private void OnDestroy()
    {
        foreach (List<EventInstance> eventInstances in m_playingEventsDict.Values)
        {
            foreach (EventInstance eventInstance in eventInstances)
            {
                StopAndClear(eventInstance);
            }
        }
    }

    private void StopAndClear(EventInstance _eventInstance)
    {
        _eventInstance.stop(STOP_MODE.ALLOWFADEOUT);
        _eventInstance.release();
        SoundCallbackGetter.RemoveCallback(_eventInstance);
    }
}
