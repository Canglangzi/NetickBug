using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;


namespace CockleBurs.GameFramework.Utility
{
public struct  ObservableStatic<T> : INotifyPropertyChanged  
{
    private T _value;

    public static event Action<T> OnValueChanged;


    public T Value
    {
        get => _value;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(_value, value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value); // 触发静态事件
                // 通知外部监听
                StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(nameof(Value)));
              //  OnPropertyChanged(nameof(Value));
            }
        }
    }
    public static event PropertyChangedEventHandler StaticPropertyChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    // 提供静态方法，来触发通知
    public static void AddStaticPropertyChangedListener(PropertyChangedEventHandler handler)
    {
        StaticPropertyChanged += handler;
    }

    // 提供静态方法移除监听
    public static void RemoveStaticPropertyChangedListener(PropertyChangedEventHandler handler)
    {
        StaticPropertyChanged -= handler;
    }
    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

}