using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

[StructLayout(LayoutKind.Sequential)]
class TimelineInfo
{
    public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
}

public class ActionsList
{

    public List<Action<object[]>> m_actions;

    public ActionsList(Action<object[]> _action)
    {
        m_actions = new List<Action<object[]>>();
        m_actions.Add(_action);
    }
}

class CallbackEventsDict
{
    public Dictionary<FMOD.Studio.EVENT_CALLBACK_TYPE, ActionsList> m_callbacksEventsDict;

    public CallbackEventsDict(FMOD.Studio.EVENT_CALLBACK_TYPE _type, Action<object[]> _action)
    {
        m_callbacksEventsDict = new Dictionary<FMOD.Studio.EVENT_CALLBACK_TYPE, ActionsList>();
        m_callbacksEventsDict.Add(_type, new ActionsList(_action));
    }
}

public static class SoundCallbackGetter
{
    private static Dictionary<FMOD.Studio.EventInstance, ActionsList> m_instanceCallbacks = new Dictionary<FMOD.Studio.EventInstance, ActionsList>();

    public static void AddCallback(FMOD.Studio.EventInstance _eventInstance, FMOD.Studio.EVENT_CALLBACK_TYPE _type, Action<object[]> _action)
    {
        if (m_instanceCallbacks.ContainsKey(_eventInstance))
        {
            ActionsList actionList;
            if (m_instanceCallbacks.TryGetValue(_eventInstance, out actionList))
            {
                if (actionList == null)
                    actionList = new ActionsList(_action);
                else
                    actionList.m_actions.Add(_action);
                m_instanceCallbacks.Remove(_eventInstance);
                m_instanceCallbacks.Add(_eventInstance, actionList);
            }
        }
        else
        {
            m_instanceCallbacks.Add(_eventInstance, new ActionsList(_action));
        }
        AssignCallbackEvent(_eventInstance, _type);
    }

    public static void RemoveCallback(FMOD.Studio.EventInstance _eventInstance)
    {
        if (m_instanceCallbacks.ContainsKey(_eventInstance))
            m_instanceCallbacks.Remove(_eventInstance);
    }

    public static void RemoveCallback(FMOD.Studio.EventInstance _eventInstance, Action<object[]> _action)
    {
        if (m_instanceCallbacks.ContainsKey(_eventInstance))
        {
            ActionsList actionList;
            if(m_instanceCallbacks.TryGetValue(_eventInstance, out actionList))
            {
                actionList.m_actions.Remove(_action);
                m_instanceCallbacks.Remove(_eventInstance);
                m_instanceCallbacks.Add(_eventInstance, actionList);
            }
        }
    }

    private static void AssignCallbackEvent(FMOD.Studio.EventInstance _instance, FMOD.Studio.EVENT_CALLBACK_TYPE _type)
    {
        TimelineInfo timelineInfo = new TimelineInfo();
        GCHandle timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Normal);

        FMOD.Studio.EVENT_CALLBACK callBackEvent = new FMOD.Studio.EVENT_CALLBACK(EventCallback);
        _instance.setUserData(GCHandle.ToIntPtr(timelineHandle));
        _instance.setCallback(callBackEvent, _type);
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT EventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE _type, IntPtr _instancePtr, IntPtr _parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(_instancePtr);
        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Timeline Callback error: " + result);
        }
        else if (timelineInfoPtr != IntPtr.Zero)
        {
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

            switch (_type)
            {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(_parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.lastMarker = parameter.name;
                        string markerName = timelineInfo.lastMarker;
                        CallActions(instance, new object[] { _type, markerName });
                    }
                    break;

                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(_parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
                        float beatDuration = 48f / parameter.tempo;
                        CallActions(instance, new object[] { _type, beatDuration });
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.STARTED:
                    {
                        CallActions(instance, new object[] { _type });
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED:
                    {
                        CallActions(instance, new object[] { _type });
                    }
                    break;
            }
        }
        return FMOD.RESULT.OK;
    }

    private static void CallActions(FMOD.Studio.EventInstance instance, object[] eventData)
    {
        ActionsList actionsList = CheckCallback(instance);
        if (actionsList != null)
        {
            foreach (Action<object[]> action in actionsList.m_actions)
            {
                action?.Invoke(eventData);
            }
        }
    }

    public static ActionsList CheckCallback(FMOD.Studio.EventInstance _eventInstance)
    {
        ActionsList actionsList;
        if (m_instanceCallbacks.TryGetValue(_eventInstance, out actionsList))
        {
            //Debug.Log("POUET POUET"); //FAIRE ICI LES ENVOIES DES ACTIONS, PEUT ETRE TOUT RAPPATRIE DANS CETTE CLASSE AVEC UN DICT QUI ASSOCIE INSTANCE, TYPE ET ACTION
            return actionsList;
        }
        return null;
    }
}
