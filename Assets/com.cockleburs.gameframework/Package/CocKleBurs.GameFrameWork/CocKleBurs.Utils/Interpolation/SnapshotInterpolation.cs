
using System;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public abstract class SnapshotInterpolation<T> {
  const float TIME_DIALTION = 0.02f; // 时间扩展百分比
  const float TIME_DIALTION_DEADZONE = 0.5f; // 有效范围为0.0-1.0

  struct Entry {
    public T Value; // 值
    public float Time; // 时间戳
  }

  float _time; // 当前时间
  float _timeScale; // 时间缩放
  float _timeScaleTimer; // 时间缩放计时器

  int _rate; // 比率
  bool _insert; // 插入标志
  float _offset; // 偏移量

  int _head; // 头指针
  int _tail; // 尾指针
  int _count; // 计数

  Entry[] _array; // 快照数组
  Entry _current; // 当前快照

  Entry Oldest => this[0]; // 最旧的快照
  Entry Latest => this[_count - 1]; // 最新的快照

  Entry this[int index] {
    get {
      if ((uint)index >= (uint)_count) {
        throw new IndexOutOfRangeException();
      }

      return _array[(_tail + index) % _array.Length];
    }
  }

  public T Current => _current.Value; // 当前值
  public float Time => _time; // 当前时间

  public void Init(int rate, float buffering) {
    _rate = rate;
    _array = new Entry[rate / 2];
    _offset = buffering;

    Reset(); // 重置
  }

  public bool Update(float dt) {
    if (_count == 0) {
      return false;
    }

    // 将插值时间向前推进
    _time += dt * _timeScale;

    // 检查是否应停止时间扩展
    if (_timeScaleTimer > 0 && (_timeScaleTimer -= dt) <= 0) {
      SetTimeDialation(0f); // 设置时间扩展
    }

    // 查找插值点
    for (var i = 0; i < (_count - 1); ++i) {
      var f = this[i];
      var t = this[i + 1];

      if (f.Time <= _time && t.Time >= _time) {
        var range = t.Time - f.Time;
        var value = _time - f.Time;

        _current.Value = Interpolate(f.Value, t.Value, Mathf.Clamp01(value / range));
        _current.Time = _time;

        // 完成
        return true;
      }
    }

    // 回退到最老的（如果在之前）
    if (_time <= Oldest.Time) {
      _current = Oldest;
      return true;
    }

    // 这意味着时间超过了我们得到的最新快照，请处理这个
    if (_time >= Latest.Time) {
      var latest = Latest;
      Reset();
      _current = latest;
      _insert  = true;
      return true;

    } 
    
    // 这应该是不可能的...
    throw new Exception("找不到插值点");
  }

  public void Teleport(float time, T value) {
    Reset();
    Add(time, value);
  }

  public void Add(float time, T value) {
    if (_count == _array.Length) {
      _array[_tail] = default;

      _tail = (_tail + 1) % _array.Length;
      _count -= 1;
    }

    // 时间倒退？重置
    if (_count > 0 && Latest.Time > time) {
      Reset();
    }

    // 从第一个样本初始化时间
    if (_count == 0) {
      if (_insert) {
        _insert = false;
        Add(time - (1f / _rate), _current.Value);
      }

      _time = time - _offset;
    } 
    
    // 不是第一个样本...
    else {
      // 清除插入标志
      _insert = false;

      // 如果本地时间超过收到的时间，则重置本地时间
      if (_time >= time) {
        _time = time - _offset;
        SetTimeDialation(0f);
      }
      
      // 否则，计算最小/最大跨度并计算时间扩展
      else {
        var min = time - (_offset * (1f + TIME_DIALTION_DEADZONE));
        var max = time - (_offset * (1f - TIME_DIALTION_DEADZONE));

        if (_time < min) {
          SetTimeDialation(+TIME_DIALTION);
        } else if (_time > max) {
          SetTimeDialation(-TIME_DIALTION);
        } else {
          SetTimeDialation(0f);
        }
      }
    }

    // 存储在头部
    _array[_head].Time = time;
    _array[_head].Value = value;

    // 移动头指针向前
    _head = (_head + 1) % _array.Length;

    // 增加计数
    _count += 1;
  }

  protected abstract T Interpolate(T from, T to, float alpha); // 插值方法

  void SetTimeDialation(float dialation) {
    if (dialation == 0f) {
      _timeScale = 1f;
      _timeScaleTimer = 0f;
    } else {
      _timeScale = 1f + dialation;
      _timeScaleTimer = (1f / _rate) * 2;
    }
  }

  void Reset() {
    _head = 0;
    _tail = 0;
    _count = 0;
    _time = 0;

    _timeScale = 1;
    _timeScaleTimer = 0;

    _current = default;
    _insert = false;

    Array.Clear(_array, 0, _array.Length);
  }
}

public class SnapshotInterpolationTransform : SnapshotInterpolation<SnapshotInterpolationTransform.TransformData> {
  public struct TransformData {
    public Vector3 Position; // 位置
    public Quaternion Rotation; // 旋转
  }

  protected override TransformData Interpolate(TransformData from, TransformData to, float alpha) {
    TransformData result;
    result.Position = Vector3.Lerp(from.Position, to.Position, alpha);
    result.Rotation = Quaternion.Slerp(from.Rotation, to.Rotation, alpha);
    return result;
  }
}

}