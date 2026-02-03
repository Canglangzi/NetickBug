using System;
using System.Collections.Generic;


namespace CockleBurs.GameFramework.Utility
{
public class ObservableActionVariable<T>
{
    private T _value;
    private List<Action<T>> _listeners = new List<Action<T>>();

    // 获取或设置变量的值，并通知所有监听者
    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                NotifyListeners();
            }
        }
    }

    // 添加监听者
    public void AddListener(Action<T> listener)
    {
        if (!_listeners.Contains(listener))
        {
            _listeners.Add(listener);
        }
    }

    // 移除监听者
    public void RemoveListener(Action<T> listener)
    {
        if (_listeners.Contains(listener))
        {
            _listeners.Remove(listener);
        }
    }

    // 通知所有监听者
    private void NotifyListeners()
    {
        foreach (var listener in _listeners)
        {
            listener?.Invoke(_value);
        }
    }
}
}