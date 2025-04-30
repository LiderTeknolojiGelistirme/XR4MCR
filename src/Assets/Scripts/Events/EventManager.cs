using System;
using System.Collections.Generic;
using NodeSystem.Events;
using UnityEngine.Events;

namespace NodeSystem.Events
{
    public enum LTGEventType
    {
        OnPointerDown, OnDrag, OnPointerUp, OnDeleteKeyPressed,
        OnPointerHoverEnter, OnPointerHoverExit,
        OnConnectionCreated, OnConnectionRemoved,
        NodeAdded, NodeRemoved, ConnectionAdded, ConnectionRemoved,
        OnElementSelected, OnElementUnselected
    }

    [System.Serializable]
    public class Event<T> : UnityEvent<T> { }

    public class EventManager<T>
    {
        Dictionary<LTGEventType,Event<T>> _eventDictionaryElement;

        public EventManager()
        {
            if (_eventDictionaryElement == null)
            {
                _eventDictionaryElement = new Dictionary<LTGEventType, Event<T>>();
            }
        }

        public void StartListening(LTGEventType eventName, UnityAction<T> listener)
        {
            Event<T> thisEvent = null;
            if (_eventDictionaryElement.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new Event<T>();
                thisEvent.AddListener(listener);
                _eventDictionaryElement.Add(eventName, thisEvent);
            }
        }

        public void StopListening(LTGEventType eventName, UnityAction<T> listener)
        {
            Event<T> thisEvent = null;
            if (_eventDictionaryElement.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public void TriggerEvent(LTGEventType eventName, T obj)
        {
            Event<T> thisEvent = null;
            if (_eventDictionaryElement.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke(obj);
            }
        }
    }
} 