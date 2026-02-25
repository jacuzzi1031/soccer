
using System;
using System.Collections.Generic;

public class SimEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private readonly List<object> _eventQueue = new();
    public void Subscribe<T>(Action<T> handler) where T : struct
    {
        var type = typeof(T);
        if (!_handlers.TryGetValue(type, out var list))
        {
            list = new List<Delegate>();
            _handlers[type] = list;
        }
        list.Add(handler);
    }


    public void Publish<T>(T evt) where T : struct
    {
        _eventQueue.Add(evt);
    }


    public void Flush()
    {
        for (int i = 0; i < _eventQueue.Count; i++)
        {
            var evt = _eventQueue[i];
            var type = evt.GetType();

            if (_handlers.TryGetValue(type, out var list))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    list[j].DynamicInvoke(evt);
                }
            }
        }

        _eventQueue.Clear();
    }
}
