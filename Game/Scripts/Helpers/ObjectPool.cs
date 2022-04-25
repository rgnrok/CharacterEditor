using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class ObjectPool<T>
{
    private readonly List<T> _objects;

    private readonly Func<T> _objectGenerator;
    private readonly Action<T> _objectDestructor;
    private readonly Action<T> _objectHidder;

    public int CurrentIndex { get; private set; }

    public ObjectPool(Func<T> objectGenerator, Action<T> objectDestructor, Action<T> objectHidder)
    {
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        _objectDestructor = objectDestructor ?? throw new ArgumentNullException(nameof(objectDestructor));
        _objectHidder = objectHidder ?? throw new ArgumentNullException(nameof(objectHidder));

        _objects = new List<T>();
    }

    public T Get(int index)
    {
        if (index < _objects.Count) return _objects[index];
        return default(T);
    }


    public T Get()
    {
        if (CurrentIndex >= _objects.Count)
        {
            _objects.Add(_objectGenerator());
        }

        return _objects[CurrentIndex++];
    }

    public void Reset()
    {
        CurrentIndex = 0;
        foreach (var obj in _objects)
        {
            _objectHidder(obj);
        }

    }

    public void Clear()
    {
        _objects.Clear();
        foreach (var obj in _objects)
        {
            _objectDestructor(obj);
        }
    }

    public void HiddeOthers()
    {
        for (int i = CurrentIndex; i < _objects.Count; i++)
        {
            _objectHidder(_objects[i]);
        }
    }
}
