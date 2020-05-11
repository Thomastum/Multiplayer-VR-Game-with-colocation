using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class EventListener : MonoBehaviour
{
    public GameEvent Event;
    public UnityEvent Response;

    void OnEnable() => Event.Subscribe(this);
    void OnDisable() => Event.Unsubscribe(this);
    public void OnEventInvoked()
    {
       Response.Invoke();
    }
}
