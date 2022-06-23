using System;
using System.Collections.Generic;

public class ObjectPool<T>
{
    private readonly List<T> _objects;

    private readonly Func<T> _objectGenerator;
    private readonly Action<T> _objectDestructor;
    private readonly Action<T> _objectHider;

    public int CurrentIndex { get; private set; }

    public ObjectPool(Func<T> objectGenerator, Action<T> objectDestructor, Action<T> objectHider)
    {
        _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
        _objectDestructor = objectDestructor ?? throw new ArgumentNullException(nameof(objectDestructor));
        _objectHider = objectHider ?? throw new ArgumentNullException(nameof(objectHider));

        _objects = new List<T>();
    }

    public T Get()
    {
        if (CurrentIndex < _objects.Count)
            return _objects[CurrentIndex++];

        var obj = _objectGenerator();
        _objects.Add(obj);

        return obj;
    }

    public void Reset()
    {
        foreach (var obj in _objects)
            _objectHider(obj);

        CurrentIndex = 0;
    }

    public void Clear()
    {
        foreach (var obj in _objects)
            _objectDestructor(obj);

        _objects.Clear();
    }

    public void HideOthers()
    {
        for (var i = CurrentIndex; i < _objects.Count; i++)
            _objectHider(_objects[i]);
    }
}
