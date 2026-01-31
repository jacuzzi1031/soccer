
using System;
using System.Collections.Generic;

public class SimEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

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
        if (_handlers.TryGetValue(typeof(T), out var list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                ((Action<T>)list[i])(evt);
            }
        }
    }
}
