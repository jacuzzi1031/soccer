using System;
using System.Diagnostics;
using System.Threading;


/// <summary>
/// 对象池实现（线程安全）
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T> where T : class
{   
    
    //UDP 通信通常用于 高频率实时同步（例如帧同步、状态广播）每一帧都要 new 一个 ResFrameSyncData 会产生大量 GC 压力
    
    //List<T>等价于 private T[] _items和private int _size;  从而少边界检查、少分支 访问更快
    //避免List 的“动态扩容” 扩容意味着1分配新的更大数组；2把旧数据复制过去；3旧数组成为垃圾（等待 GC）。
    private struct Element
    {
        public T value;
    }
    //也就是先看T _mFirstItem是否可以取走，不行再考虑Element[] _mElementItems Release也是同样的
    private T _mFirstItem;
    private readonly Element[] _mElementItems;

    private readonly Func<T> _mFactory;

    public ObjectPool(Func<T> factory) : this(factory, Environment.ProcessorCount * 2)
    {
    }

    public ObjectPool(Func<T> mFactory, int size)
    {
        Debug.Assert(size > 1);
        _mFactory = mFactory;
        _mElementItems = new Element[size - 1];
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private T CreateInstance()
    {   
        //保存方法调用
        return _mFactory();
    }

    public T Allocate()
    {
        T inst = _mFirstItem;
        //CompareExchange是ref==comparand，就返回值同时设为新值null
        //也就是CompareExchange中 _mFirstItem==inst /return inst/inst！Interlocked为false 
        //被取走 inst!=null为true  AllocateSlow
            if (inst == null || inst != Interlocked.CompareExchange(ref _mFirstItem, null, inst))
        {
            inst = AllocateSlow();
        }

        return inst;
    }

    private T AllocateSlow()
    {
        Element[] items = _mElementItems;
        for (int i = 0; i < items.Length; i++)
        {
            T inst = items[i].value;
            if (inst != null)
            {
                if (inst == Interlocked.CompareExchange(ref items[i].value, null, inst))
                {
                    return inst;
                }
            }
        }

        return CreateInstance();
    }

    public void Release(T obj)
    {
        Validate(obj);

        if (_mFirstItem == null)
        {
            _mFirstItem = obj;
        }
        else
        {
            ReleaseSlow(obj);
        }
    }

    private void ReleaseSlow(T obj)
    {
        Element[] items = _mElementItems;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].value == null)
            {
                items[i].value = obj;
                break;
            }
        }
    }

    private void Validate(object obj)
    {      
        //为假触发
        Debug.Assert(obj != null, "free obj is null?");
        Debug.Assert(_mFirstItem != obj, "free obj twice?");

        var items = _mElementItems;
        for (int i = 0; i < items.Length; i++)
        {
            var value = items[i].value;
            if (value == null)
            {
                return;
            }

            Debug.Assert(value != obj, "free obj twice?");
        }
    }
}