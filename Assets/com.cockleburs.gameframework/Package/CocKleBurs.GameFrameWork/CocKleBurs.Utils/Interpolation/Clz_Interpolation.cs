using System;
using Unity.Mathematics;
using UnityEngine;


using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public interface IInterpolator<T>
{
    T Lerp(T from, T to, float alpha);
}

public class Vector3Interpolator : IInterpolator<Vector3>
{
    public Vector3 Lerp(Vector3 from, Vector3 to, float alpha)
    {
        return Vector3.LerpUnclamped(from, to, alpha);
    }
}

public class FloatInterpolator : IInterpolator<float>
{
    public float Lerp(float from, float to, float alpha)
    {
        return Mathf.LerpUnclamped(from, to, alpha);
    }
}
public class Float3Interpolator : IInterpolator<float3>
{
    public float3 Lerp(float3 from, float3 to, float alpha)
    {
        return math.lerp(from, to, alpha);
    }
}

public class quaternionInterpolator : IInterpolator<quaternion>
{
    public quaternion Lerp(quaternion from, quaternion to, float alpha)
    {
        return math.slerp(from, to, alpha);
    }
}
public class QuaternionInterpolator : IInterpolator<Quaternion>
{
    public Quaternion Lerp(Quaternion from, Quaternion to, float alpha)
    {
        return Quaternion.SlerpUnclamped(from, to, alpha);
    }
}

/// <summary>
/// Represents a generic interpolation class for interpolating values of type T over time.
/// </summary>
/// <typeparam name="T">The type of values to interpolate. Must support interpolation through an <see cref="IInterpolator{T}"/> implementation.</typeparam>
public class Clz_Interpolation<T>
{
    /// <summary>
    /// Represents an entry in the interpolation buffer with a value and its associated time.
    /// </summary>
    private struct Entry
    {
        public T Value;
        public float Time;
    }

    private Entry[] _entries;
    private int _head;
    private int _count;
    private float _buffering;
    private float _currentTime;
    private IInterpolator<T> _interpolator;

    /// <summary>
    /// Gets the current interpolated value.
    /// </summary>
    public T Current { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Clz_Interpolation{T}"/> class.
    /// </summary>
    /// <param name="interpolator">The interpolator to use for the type T.</param>
    /// <param name="capacity">The initial capacity of the buffer.</param>
    /// <param name="buffering">The buffering time in seconds.</param>
    public Clz_Interpolation(IInterpolator<T> interpolator, int capacity = 60, float buffering = 0.1f)
    {
        _entries = new Entry[capacity];
        _buffering = buffering;
        _interpolator = interpolator;
        Reset();
    }

    /// <summary>
    /// Adds a new value at a specific time to the interpolation buffer.
    /// </summary>
    /// <param name="time">The time at which the value was captured.</param>
    /// <param name="value">The value to add to the buffer.</param>
    public void Add(float time, T value)
    {
        if (_count == _entries.Length)
        {
            Array.Resize(ref _entries, _entries.Length * 2);
        }

        _entries[_head].Time = time;
        _entries[_head].Value = value;

        _head = (_head + 1) % _entries.Length;
        _count++;
    }

    /// <summary>
    /// Updates the current interpolated value based on the current time.
    /// </summary>
    /// <param name="currentTime">The current time.</param>
    public void Update(float currentTime)
    {
        _currentTime = currentTime;

        if (_count == 0)
        {
            return;
        }

        Entry latest = _entries[_count - 1];

        if (_currentTime >= latest.Time)
        {
            Reset();
            Current = latest.Value;
            return;
        }

        for (int i = 0; i < _count - 1; i++)
        {
            Entry current = _entries[i];
            Entry next = _entries[i + 1];

            if (current.Time <= _currentTime && next.Time >= _currentTime)
            {
                float range = next.Time - current.Time;
                float timeDiff = _currentTime - current.Time;
                Current = _interpolator.Lerp(current.Value, next.Value, Mathf.Clamp01(timeDiff / range));
                return;
            }
        }

        Current = _entries[0].Value;
    }

    /// <summary>
    /// Resets the interpolation buffer.
    /// </summary>
    public void Reset()
    {
        _head = 0;
        _count = 0;
        Current = default;
    }

    /// <summary>
    /// Adds a new value to the buffer and resets the interpolation to start from this value.
    /// </summary>
    /// <param name="time">The time at which the value was captured.</param>
    /// <param name="value">The value to start interpolation from.</param>
    public void Teleport(float time, T value)
    {
        Reset();
        Add(time, value);
    }


    //     LocalTick：

    // 定义：本地游标，不同步的游标。此值仅递增。可以用于具有自定义逻辑的索引或标识。
    // 解释：在 FishNet 中，LocalTick 是一个在客户端和服务器之间本地使用的递增值。在服务器上调用时返回 Tick（服务器的网络时间），而在客户端上调用时返回本地的 LocalTick。LocalTick 的值在断开连接时重置。可以用于管理需要递增索引或标识的场景，例如生成唯一的对象 ID 等。
    // Tick：

    // 定义：当前的近似网络时刻，当作为客户端运行时，它是对服务器时刻的近似。该字段的值可能会因为时间调整而增加或减少。该值在断开连接时重置。
    // 解释：Tick 表示当前的网络时刻，对于服务器和客户端来说都是重要的概念。在客户端，它近似于服务器的时刻，而在服务器上，它是实际的网络时刻。Tick 的值可能会根据网络延迟和调整而变动，但在整个网络中用来同步事件和操作的顺序。
    // TickDelta：

    // 定义：TickRate 的固定 deltaTime。
    // 解释：TickDelta 是 TickRate 的倒数，表示每个 Tick 之间的时间间隔。它可以帮助确定每个 Tick 的持续时间，是时间相关操作的一个重要基准。
    // TickRate：

    // 定义：服务器模拟的每秒次数。这并不限制服务器的帧率。
    // 解释：TickRate 指定了服务器每秒进行模拟和更新的次数。尽管它控制了服务器模拟的频率，但并不直接限制服务器的实际帧率，因为帧率可能受到服务器性能和负载的影响。
    // ServerUptime：

    // 定义：服务器已连接的时间长度。
    // 解释：ServerUptime 表示服务器已经保持连接的时间。这对于监视服务器运行时间和运行时统计信息非常有用。
    // ClientUptime：

    // 定义：客户端已连接的时间长度。
    // 解释：ClientUptime 表示客户端已经保持连接的时间。这可以用来计算客户端的运行时长或者进行相关的时间相关逻辑。

}

}