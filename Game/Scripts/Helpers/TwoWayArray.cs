using System.Collections.Generic;

public class TwoWayArray<T>
{
    private T[] _data;
    private int _currentIndex;

    public T Current
    {
        get
        {
            if (_data.Length == 0) return default(T);
            if (_currentIndex < 0 || _currentIndex >= _data.Length) return default(T);
            return _data[_currentIndex];
        }
    }

    public T Next
    {
        get
        {
            _currentIndex++;
            if (_currentIndex >= _data.Length) _currentIndex = 0;
            return Current;
        }
    }

    public T Prev
    {
        get
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _data.Length - 1;
            return Current;
        }
    }

    public TwoWayArray(T[] data)
    {
        _data = data;
        _currentIndex = 0;
    }

    public TwoWayArray(IList<T> data)
    {
        _currentIndex = 0;
        _data = new T[data.Count];
        for (var i = 0; i < data.Count; i++)
            _data[i] = data[i];
    }
}
