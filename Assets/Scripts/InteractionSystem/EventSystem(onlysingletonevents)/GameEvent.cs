using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameEvent : ScriptableObject
{
    List<EventListener> eventListeners = new List<EventListener>();

    public void Invoke()
    {
        for(int i=0; i<eventListeners.Count; i++)
        {
            eventListeners[i].OnEventInvoked();
        }
    }

    public void Subscribe(EventListener listener)
    {
        if(!eventListeners.Contains(listener))
            eventListeners.Add(listener);
    }

    public void Unsubscribe(EventListener listener)
    {
        if(eventListeners.Contains(listener))
            eventListeners.Remove(listener);
    }
}
