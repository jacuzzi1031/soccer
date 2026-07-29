using System;
using System.Collections.Generic;
public class EventSystem
{
    private interface IEventDispatcher
    {
        void Unsubscribe(Delegate subscriber);
    }

    private class EventDispatcher<T> : IEventDispatcher
        where T : struct, IEvent
    {
        private readonly List<Action<T>> _subscribers = new();

        public void Subscribe(Action<T> subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void Unsubscribe(Delegate subscriber)
        {
            _subscribers.Remove((Action<T>)subscriber);
        }

        public void Publish(T evt)
        {
            for (int i = 0; i < _subscribers.Count; i++)
            {
                Action<T> subscriber = _subscribers[i];

                Invoker.Instance.Enqueue(
                    () => subscriber(evt)
                );
            }
        }
    }

    private readonly Dictionary<Type, IEventDispatcher> _dispatchers = new();

    public void Subscribe<T>(Action<T> subscriber)
        where T : struct, IEvent
    {
        Type eventType = typeof(T);

        if (!_dispatchers.TryGetValue(eventType, out var dispatcher))
        {
            dispatcher = new EventDispatcher<T>();
            _dispatchers.Add(eventType, dispatcher);
        }

        ((EventDispatcher<T>)dispatcher).Subscribe(subscriber);
    }

    public void Unsubscribe<T>(Action<T> subscriber)
        where T : struct, IEvent
    {
        if (_dispatchers.TryGetValue(typeof(T), out var dispatcher))
        {
            dispatcher.Unsubscribe(subscriber);
        }
    }

    public void Publish<T>(T evt)
        where T : struct, IEvent
    {
        if (_dispatchers.TryGetValue(typeof(T), out var dispatcher))
        {
            ((EventDispatcher<T>)dispatcher).Publish(evt);
        }
    }
}