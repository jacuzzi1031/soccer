using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class SimEventBus
{
    // 不同事件队列的统一抽象
    private interface ISimEventQueue
    {
        void FlushEvents();
    }

    // 每一种具体事件都有自己的泛型队列
    private class SimEventQueue<T> : ISimEventQueue
        where T : struct
    {
        // 演算层产生、等待桥接层消费的事件
        private readonly List<T> _pendingEvents = new();

        // 桥接层订阅该事件的所有处理函数
        private readonly List<Action<T>> _eventSubscribers = new();

        // 桥接层订阅事件
        public void Subscribe(Action<T> subscriber)
        {
            _eventSubscribers.Add(subscriber);
        }
        // 演算层发布事件
        public void Publish(T simEvent)
        {
            _pendingEvents.Add(simEvent);
        }
        
        public void FlushEvents()
        {
            for (int i = 0; i < _pendingEvents.Count; i++)
            {
                T simEvent = _pendingEvents[i];

                for (int j = 0; j < _eventSubscribers.Count; j++)
                {
                    _eventSubscribers[j](simEvent);
                }
            }

            _pendingEvents.Clear();
        }
    }

    // 根据事件类型找到对应的事件队列
    private readonly Dictionary<Type, ISimEventQueue> _eventQueues = new();

    // 桥接层订阅事件
    public void Subscribe<T>(Action<T> subscriber) where T : struct
    {
        Type eventType = typeof(T);

        if (!_eventQueues.TryGetValue(eventType, out var eventQueue))
        {
            eventQueue = new SimEventQueue<T>();
            _eventQueues.Add(eventType, eventQueue);
        }

        ((SimEventQueue<T>)eventQueue).Subscribe(subscriber);
    }

    // 演算层发布事件
    public void Publish<T>(T simEvent) where T : struct
    {
        Type eventType = typeof(T);

        if (!_eventQueues.TryGetValue(eventType, out var eventQueue))
        {
            eventQueue = new SimEventQueue<T>();
            _eventQueues.Add(eventType, eventQueue);
        }

        ((SimEventQueue<T>)eventQueue).Publish(simEvent);
    }

    // 在演算帧结束后统一派发事件
    public void FlushEvents()
    {
        foreach (var eventQueue in _eventQueues.Values)
        {
            eventQueue.FlushEvents();
        }
    }
}