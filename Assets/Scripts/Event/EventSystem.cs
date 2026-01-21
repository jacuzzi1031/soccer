using System;
using System.Collections.Generic;
/// <summary>
///
// 所有会影响比赛结果 / GameState 的事件 → 必须走 EventSystem + Invoker
// 所有不影响确定性的本地行为 → 可以直接 += / -=

/// @前缀可以让关键字（keyword）当作普通变量名使用。
/// 有先发布后订阅 _mEventPool
/// Subscribe 先执行_mEventPool的，再_mEvents订阅
/// publish() 如果_mEvents有订阅者 调用Invoker执行。如果没有订阅者 → 放入 _mEventPool。
/// 半帧同步事件系统，把事件延迟执行交给了 Invoker
/// </summary>
public class EventSystem
{
    private readonly Dictionary<Type, Delegate> _mEvents = new();
    private readonly Dictionary<Type, List<IEvent>> _mEventPool = new();
    
    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        // string key = GetKey(handler);

        var @event = typeof(T); //@event.FullName == "Namespace.ChangeGameStateEvent"

        if (_mEventPool.TryGetValue(@event, out var eventList))
        {
            foreach (var e in eventList)
            {
                Invoker.Instance.DelegateList.Add(() => { handler.Invoke((T)e); });
            }
            _mEventPool[@event].Clear();
        }
        if (_mEvents.TryGetValue(@event, out var existHandlers))
        {
            _mEvents[@event] = Delegate.Combine(existHandlers, handler);
        }
        else
        {
            _mEvents[@event] = handler;
        }
    }

    public void Unsubscribe<T>(Action<T> handler) where T : IEvent
    {
        var key = typeof(T);
        if (_mEvents.TryGetValue(key, out var existHandlers))
        {
            Delegate newHandlers = Delegate.Remove(existHandlers, handler);
            if (newHandlers == null)
            {
                _mEvents.Remove(key);
            }
            else
            {
                _mEvents[key] = newHandlers;
            }
        }
    }

    public void Publish<T>(T e) where T : IEvent
    {
        var @event = typeof(T);
        if (_mEvents.TryGetValue(@event, out var existHandlers))
        {
            // (existHandlers as Action<T>)?.Invoke(e);
            Invoker.Instance.DelegateList.Add(() => { (existHandlers as Action<T>)?.Invoke(e); });
            return;
        }
        
        if (_mEventPool.TryGetValue(@event, out var eventList))
        {
            eventList.Add(e);
        }
        else
        {
            _mEventPool[@event] = new List<IEvent> { e };
        }
    }

    public void Publish<T>() where T : IEvent, new()
    {
        T e = new T();
        var @event = typeof(T);
        if (_mEvents.TryGetValue(@event, out var existHandlers))
        {
            // (existHandlers as Action<T>)?.Invoke(e);
            Invoker.Instance.DelegateList.Add(() => { (existHandlers as Action<T>)?.Invoke(e); });
            return; //如果有订阅者 调用Invoker。如果没有订阅者 → 放入 _mEventPool。
        }

        if (_mEventPool.TryGetValue(@event, out var eventList))
        {
            eventList.Add(e);
        }
        else
        {
            _mEventPool[@event] = new List<IEvent> { e };
        }
    }
}