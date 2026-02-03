using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// 高级InputActionReference静态扩展方法
/// 提供丰富的输入处理功能
/// </summary>
public static class InputActionReferenceExtensions
{
    #region 基本输入状态检查
    
    /// <summary>
    /// 检查InputAction是否刚刚按下（Triggered）
    /// </summary>
    public static bool WasPressed(this InputActionReference actionRef)
    {
        return actionRef?.action?.triggered ?? false;
    }
    
    /// <summary>
    /// 检查InputAction是否正在按住
    /// </summary>
    public static bool IsPressed(this InputActionReference actionRef)
    {
        return actionRef?.action?.IsPressed() ?? false;
    }
    
    /// <summary>
    /// 检查InputAction是否刚刚释放
    /// </summary>
    public static bool WasReleased(this InputActionReference actionRef)
    {
        return actionRef?.action?.WasReleasedThisFrame() ?? false;
    }
    


// 如果你想要更多的取消检查选项：
    public static bool IsCanceled(this InputActionReference actionRef)
    {
        // 方法2：使用 IsCanceled() 检查是否处于取消状态
        return actionRef?.action?.phase == InputActionPhase.Canceled;
    }

    public static bool WasCanceledInLastFrame(this InputActionReference actionRef, int frames = 1)
    {
        // 方法3：检查最近几帧内是否取消（需要自己实现帧计数逻辑）
        return actionRef?.action?.phase == InputActionPhase.Canceled;
    }
    /// <summary>
    /// 检查InputAction是否在特定时间内被按下过
    /// </summary>
    public static bool WasPressedWithin(this InputActionReference actionRef, float seconds)
    {
        return GetPressTime(actionRef) <= seconds;
    }
    
    #endregion

    #region 高级输入状态检查
    
    /// <summary>
    /// 检查是否是双击（在指定时间内按下两次）
    /// </summary>
    public static bool IsDoubleClick(this InputActionReference actionRef, float doubleClickTime = 0.3f)
    {
        if (!actionRef.IsValid()) return false;
        
        var history = InputHistoryManager.GetHistory(actionRef);
        if (history.Count < 2) return false;
        
        var lastPress = history[history.Count - 1];
        var previousPress = history[history.Count - 2];
        
        return lastPress.type == InputEventType.Pressed && 
               previousPress.type == InputEventType.Pressed &&
               (lastPress.time - previousPress.time) <= doubleClickTime;
    }
    
    /// <summary>
    /// 检查是否是长按（按住超过指定时间）
    /// </summary>
    public static bool IsLongPress(this InputActionReference actionRef, float longPressTime = 0.5f)
    {
        if (!actionRef.IsPressed()) return false;
        
        var history = InputHistoryManager.GetHistory(actionRef);
        if (history.Count == 0) return false;
        
        var lastPress = history.FindLast(e => e.type == InputEventType.Pressed);
        return !IsDefault(lastPress) && (Time.time - lastPress.time) >= longPressTime;
    }
    
    /// <summary>
    /// 获取按键按住时间
    /// </summary>
    public static float GetHoldTime(this InputActionReference actionRef)
    {
        if (!actionRef.IsPressed()) return 0f;
        
        var history = InputHistoryManager.GetHistory(actionRef);
        var lastPress = history.FindLast(e => e.type == InputEventType.Pressed);
        return !IsDefault(lastPress) ? Time.time - lastPress.time : 0f;
    }
    
    /// <summary>
    /// 检查是否是按住后释放（用于区分点击和拖拽）
    /// </summary>
    public static bool WasHoldAndRelease(this InputActionReference actionRef, float minHoldTime = 0.2f)
    {
        if (!actionRef.WasReleased()) return false;
        
        var history = InputHistoryManager.GetHistory(actionRef);
        if (history.Count < 2) return false;
        
        var releaseEvent = history[history.Count - 1];
        var pressEvent = history.FindLast(e => e.type == InputEventType.Pressed);
        
        return !IsDefault(pressEvent) && 
               (releaseEvent.time - pressEvent.time) >= minHoldTime;
    }
    
    #endregion

    #region 输入值获取与处理
    
    /// <summary>
    /// 获取Vector2输入值
    /// </summary>
    public static Vector2 GetVector2(this InputActionReference actionRef)
    {
        return actionRef?.action?.ReadValue<Vector2>() ?? Vector2.zero;
    }
    
    /// <summary>
    /// 获取带死区的Vector2输入值
    /// </summary>
    public static Vector2 GetVector2WithDeadzone(this InputActionReference actionRef, float deadzone = 0.1f)
    {
        Vector2 input = actionRef.GetVector2();
        return ApplyDeadzone(input, deadzone);
    }
    
    /// <summary>
    /// 获取平滑的Vector2输入值（使用移动平均滤波）
    /// </summary>
    public static Vector2 GetSmoothedVector2(this InputActionReference actionRef, int bufferSize = 5)
    {
        if (!actionRef.IsValid()) return Vector2.zero;
        
        var buffer = InputBufferManager.GetVector2Buffer(actionRef, bufferSize);
        buffer.Add(actionRef.GetVector2());
        
        Vector2 sum = Vector2.zero;
        foreach (var sample in buffer.GetBuffer())
        {
            sum += sample;
        }
        
        return sum / buffer.Count;
    }
    
    /// <summary>
    /// 获取float输入值
    /// </summary>
    public static float GetFloat(this InputActionReference actionRef)
    {
        return actionRef?.action?.ReadValue<float>() ?? 0f;
    }
    
    /// <summary>
    /// 获取归一化的float输入值（0-1）
    /// </summary>
    public static float GetNormalizedFloat(this InputActionReference actionRef)
    {
        return Mathf.Clamp01(actionRef.GetFloat());
    }
    
    /// <summary>
    /// 获取int输入值
    /// </summary>
    public static int GetInt(this InputActionReference actionRef)
    {
        return actionRef?.action?.ReadValue<int>() ?? 0;
    }
    
    /// <summary>
    /// 获取bool输入值
    /// </summary>
    public static bool GetBool(this InputActionReference actionRef, float threshold = 0.5f)
    {
        return actionRef.GetFloat() > threshold;
    }
    
    /// <summary>
    /// 获取Vector3输入值（将2D输入转换为3D）
    /// </summary>
    public static Vector3 GetVector3(this InputActionReference actionRef, Vector3 upAxis)
    {
        Vector2 input = actionRef.GetVector2();
        
        // 根据不同的游戏类型转换
        if (upAxis == Vector3.up) // 水平移动
            return new Vector3(input.x, 0, input.y);
        else if (upAxis == Vector3.forward) // 2D游戏
            return new Vector3(input.x, input.y, 0);
        
        return new Vector3(input.x, input.y, 0);
    }
    
    /// <summary>
    /// 获取八方向输入（用于格斗游戏或2D游戏）
    /// </summary>
    public static Vector2Int Get8Direction(this InputActionReference actionRef, float threshold = 0.7f)
    {
        Vector2 input = actionRef.GetVector2();
        
        if (input.magnitude < threshold) 
            return Vector2Int.zero;
        
        float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        angle = (angle + 360) % 360;
        
        if (angle >= 337.5f || angle < 22.5f) return Vector2Int.right;      // 右
        if (angle >= 22.5f && angle < 67.5f) return new Vector2Int(1, 1);   // 右上
        if (angle >= 67.5f && angle < 112.5f) return Vector2Int.up;          // 上
        if (angle >= 112.5f && angle < 157.5f) return new Vector2Int(-1, 1); // 左上
        if (angle >= 157.5f && angle < 202.5f) return Vector2Int.left;       // 左
        if (angle >= 202.5f && angle < 247.5f) return new Vector2Int(-1, -1);// 左下
        if (angle >= 247.5f && angle < 292.5f) return Vector2Int.down;       // 下
        if (angle >= 292.5f && angle < 337.5f) return new Vector2Int(1, -1); // 右下
        
        return Vector2Int.zero;
    }
    
    /// <summary>
    /// 获取输入方向角度（度）
    /// </summary>
    public static float GetDirectionAngle(this InputActionReference actionRef)
    {
        Vector2 input = actionRef.GetVector2();
        return Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
    }
    
    /// <summary>
    /// 获取输入方向向量（归一化）
    /// </summary>
    public static Vector2 GetDirection(this InputActionReference actionRef)
    {
        return actionRef.GetVector2().normalized;
    }
    
    /// <summary>
    /// 获取输入强度（0-1）
    /// </summary>
    public static float GetMagnitude(this InputActionReference actionRef)
    {
        return Mathf.Clamp01(actionRef.GetVector2().magnitude);
    }
    
    #endregion

    #region 输入设备检测
    
    /// <summary>
    /// 获取激活此InputAction的设备列表
    /// </summary>
    public static List<InputDevice> GetActiveDevices(this InputActionReference actionRef)
    {
        if (!actionRef.IsValid()) return new List<InputDevice>();
        
        return actionRef.action.controls
            .Select(control => control.device)
            .Distinct()
            .ToList();
    }
    
    /// <summary>
    /// 检查是否是键盘输入
    /// </summary>
    public static bool IsKeyboardInput(this InputActionReference actionRef)
    {
        return actionRef.GetActiveDevices().Any(d => d is Keyboard);
    }
    
    /// <summary>
    /// 检查是否是鼠标输入
    /// </summary>
    public static bool IsMouseInput(this InputActionReference actionRef)
    {
        return actionRef.GetActiveDevices().Any(d => d is Mouse);
    }
    
    /// <summary>
    /// 检查是否是手柄输入
    /// </summary>
    public static bool IsGamepadInput(this InputActionReference actionRef)
    {
        return actionRef.GetActiveDevices().Any(d => d is Gamepad);
    }
    
    /// <summary>
    /// 检查是否是触摸输入
    /// </summary>
    public static bool IsTouchInput(this InputActionReference actionRef)
    {
        return actionRef.GetActiveDevices().Any(d => d is Touchscreen);
    }
    
    /// <summary>
    /// 获取主要输入设备类型
    /// </summary>
    public static InputDeviceType GetPrimaryDeviceType(this InputActionReference actionRef)
    {
        var devices = actionRef.GetActiveDevices();
        
        if (devices.Any(d => d is Gamepad)) return InputDeviceType.Gamepad;
        if (devices.Any(d => d is Keyboard)) return InputDeviceType.Keyboard;
        if (devices.Any(d => d is Mouse)) return InputDeviceType.Mouse;
        if (devices.Any(d => d is Touchscreen)) return InputDeviceType.Touch;
        
        return InputDeviceType.Unknown;
    }
    
    /// <summary>
    /// 检查是否所有绑定都来自同一设备类型
    /// </summary>
    public static bool IsDeviceConsistent(this InputActionReference actionRef)
    {
        var devices = actionRef.GetActiveDevices();
        if (devices.Count == 0) return true;
        
        var firstType = devices[0].GetType();
        return devices.All(d => d.GetType() == firstType);
    }
    
    #endregion

    #region 输入绑定信息
    
    /// <summary>
    /// 获取所有绑定的显示字符串
    /// </summary>
    public static List<string> GetBindingDisplayStrings(this InputActionReference actionRef)
    {
        if (!actionRef.IsValid()) return new List<string>();
        
        return actionRef.action.bindings
            .Select(b => b.ToDisplayString())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }
    
    /// <summary>
    /// 获取当前活动绑定的显示字符串
    /// </summary>
    public static string GetActiveBindingDisplayString(this InputActionReference actionRef)
    {
        if (!actionRef.IsValid()) return string.Empty;
        
        var activeControl = actionRef.action.activeControl;
        if (activeControl == null) return string.Empty;
        
        var binding = actionRef.action.bindings.FirstOrDefault(b => b.path == activeControl.path);
        return binding.ToDisplayString();
    }
    
    /// <summary>
    /// 获取绑定数量
    /// </summary>
    public static int GetBindingCount(this InputActionReference actionRef)
    {
        return actionRef?.action?.bindings.Count ?? 0;
    }
    
    /// <summary>
    /// 获取特定设备类型的绑定
    /// </summary>
    public static IEnumerable<InputBinding> GetBindingsForDevice(this InputActionReference actionRef, Type deviceType)
    {
        if (!actionRef.IsValid()) return Enumerable.Empty<InputBinding>();
        
        return actionRef.action.bindings
            .Where(b => InputSystem.FindControl(b.path)?.device?.GetType() == deviceType);
    }
    
    #endregion

    #region 输入事件与回调
    
    /// <summary>
    /// 添加按下事件监听
    /// </summary>
    public static void AddPressedListener(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
    {
        if (actionRef?.action != null)
        {
            actionRef.action.started += callback;
            InputHistoryManager.AddHistory(actionRef, InputEventType.Started);
        }
    }
    
    /// <summary>
    /// 添加进行中事件监听
    /// </summary>
    public static void AddPerformedListener(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
    {
        if (actionRef?.action != null)
        {
            actionRef.action.performed += callback;
            InputHistoryManager.AddHistory(actionRef, InputEventType.Performed);
        }
    }
    
    /// <summary>
    /// 添加取消事件监听
    /// </summary>
    public static void AddCanceledListener(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
    {
        if (actionRef?.action != null)
        {
            actionRef.action.canceled += callback;
            InputHistoryManager.AddHistory(actionRef, InputEventType.Canceled);
        }
    }
    
    /// <summary>
    /// 移除所有事件监听
    /// </summary>
    public static void RemoveAllListeners(this InputActionReference actionRef)
    {
        if (actionRef?.action != null)
        {
            // 由于事件是委托，无法直接设置为null，我们需要记录回调并移除
            InputHistoryManager.ClearAllListeners(actionRef);
        }
    }
    
    /// <summary>
    /// 异步等待按键按下
    /// </summary>
    public static async Task WaitForPressAsync(this InputActionReference actionRef)
    {
        if (!actionRef.IsValid()) return;
        
        var tcs = new TaskCompletionSource<bool>();
        
        void OnPerformed(InputAction.CallbackContext ctx)
        {
            actionRef.action.performed -= OnPerformed;
            tcs.SetResult(true);
        }
        
        actionRef.action.performed += OnPerformed;
        await tcs.Task;
    }
    
    /// <summary>
    /// 添加条件事件监听（当条件满足时才触发）
    /// </summary>
    public static void AddConditionalListener(this InputActionReference actionRef, 
        Func<InputAction.CallbackContext, bool> condition, 
        Action<InputAction.CallbackContext> callback)
    {
        if (actionRef?.action != null)
        {
            actionRef.action.performed += ctx =>
            {
                if (condition(ctx))
                    callback(ctx);
            };
        }
    }
    
    #endregion

    #region 输入控制与管理
    
    /// <summary>
    /// 安全启用InputAction
    /// </summary>
    public static void SafeEnable(this InputActionReference actionRef)
    {
        if (actionRef?.action != null && !actionRef.action.enabled)
        {
            actionRef.action.Enable();
        }
    }
    
    /// <summary>
    /// 安全禁用InputAction
    /// </summary>
    public static void SafeDisable(this InputActionReference actionRef)
    {
        if (actionRef?.action != null && actionRef.action.enabled)
        {
            actionRef.action.Disable();
        }
    }
    
    /// <summary>
    /// 切换InputAction启用状态
    /// </summary>
    public static void Toggle(this InputActionReference actionRef)
    {
        if (actionRef?.action == null) return;
        
        if (actionRef.action.enabled)
            actionRef.SafeDisable();
        else
            actionRef.SafeEnable();
    }
    
    /// <summary>
    /// 临时禁用InputAction（在指定时间后自动启用）
    /// </summary>
    public static async void DisableTemporarily(this InputActionReference actionRef, float seconds)
    {
        actionRef.SafeDisable();
        await Task.Delay((int)(seconds * 1000));
        actionRef.SafeEnable();
    }
    
    /// <summary>
    /// 启用并设置超时（在指定时间后自动禁用）
    /// </summary>
    public static async void EnableWithTimeout(this InputActionReference actionRef, float seconds)
    {
        actionRef.SafeEnable();
        await Task.Delay((int)(seconds * 1000));
        actionRef.SafeDisable();
    }
    
    /// <summary>
    /// 检查InputAction是否已被赋值
    /// </summary>
    public static bool IsValid(this InputActionReference actionRef)
    {
        return actionRef != null && actionRef.action != null;
    }
    
    /// <summary>
    /// 检查InputAction是否已启用
    /// </summary>
    public static bool IsEnabled(this InputActionReference actionRef)
    {
        return actionRef?.action?.enabled ?? false;
    }
    
    /// <summary>
    /// 获取InputAction的名称
    /// </summary>
    public static string GetName(this InputActionReference actionRef)
    {
        return actionRef?.action?.name ?? string.Empty;
    }
    
    /// <summary>
    /// 获取InputAction的完整路径
    /// </summary>
    public static string GetPath(this InputActionReference actionRef)
    {
        return actionRef?.action?.id.ToString() ?? string.Empty;
    }
    
    #endregion

    #region 输入组合与序列
    
    /// <summary>
    /// 检查组合键（两个按键同时按下）
    /// </summary>
    public static bool IsComboWith(this InputActionReference actionRef, InputActionReference otherAction)
    {
        return actionRef.IsPressed() && otherAction.IsPressed();
    }
    
    /// <summary>
    /// 检查序列输入（按顺序按下多个按键）
    /// </summary>
    public static bool IsSequence(this InputActionReference actionRef, params InputActionReference[] sequence)
    {
        if (sequence.Length == 0) return false;
        
        var history = InputHistoryManager.GetSequenceHistory();
        var sequenceIds = sequence.Select(a => a.GetPath()).ToArray();
        
        // 检查历史中是否包含此序列
        for (int i = 0; i <= history.Count - sequenceIds.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < sequenceIds.Length; j++)
            {
                if (history[i + j].actionPath != sequenceIds[j])
                {
                    match = false;
                    break;
                }
            }
            if (match) return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 检查是否是蓄力输入（按住后达到一定强度）
    /// </summary>
    public static bool IsCharged(this InputActionReference actionRef, float chargeTime, float minIntensity = 0.8f)
    {
        return actionRef.GetHoldTime() >= chargeTime && 
               actionRef.GetMagnitude() >= minIntensity;
    }
    
    #endregion

    #region 输入分析与统计
    
    /// <summary>
    /// 获取输入总次数
    /// </summary>
    public static int GetPressCount(this InputActionReference actionRef)
    {
        var history = InputHistoryManager.GetHistory(actionRef);
        return history.Count(e => e.type == InputEventType.Pressed);
    }
    
    /// <summary>
    /// 获取输入频率（按键/秒）
    /// </summary>
    public static float GetPressFrequency(this InputActionReference actionRef, float timeWindow = 5f)
    {
        var history = InputHistoryManager.GetHistory(actionRef);
        var recentPresses = history.Where(e => 
            e.type == InputEventType.Pressed && 
            (Time.time - e.time) <= timeWindow);
        
        return recentPresses.Count() / timeWindow;
    }
    
    /// <summary>
    /// 获取平均按住时间
    /// </summary>
    public static float GetAverageHoldTime(this InputActionReference actionRef)
    {
        var history = InputHistoryManager.GetHistory(actionRef);
        float totalHoldTime = 0;
        int holdCount = 0;
        
        for (int i = 0; i < history.Count - 1; i++)
        {
            if (history[i].type == InputEventType.Pressed && 
                history[i + 1].type == InputEventType.Released)
            {
                totalHoldTime += history[i + 1].time - history[i].time;
                holdCount++;
            }
        }
        
        return holdCount > 0 ? totalHoldTime / holdCount : 0f;
    }
    
    /// <summary>
    /// 获取最近输入的时间
    /// </summary>
    public static float GetTimeSinceLastInput(this InputActionReference actionRef)
    {
        var history = InputHistoryManager.GetHistory(actionRef);
        if (history.Count == 0) return float.MaxValue;
        
        return Time.time - history.Last().time;
    }
    
    #endregion

    #region 输入变换与映射
    
    /// <summary>
    /// 应用曲线变换到输入值
    /// </summary>
    public static float ApplyCurve(this InputActionReference actionRef, AnimationCurve curve)
    {
        float value = actionRef.GetFloat();
        return curve.Evaluate(value);
    }
    
    /// <summary>
    /// 应用响应曲线（如：指数响应）
    /// </summary>
    public static float ApplyResponseCurve(this InputActionReference actionRef, float exponent = 2f)
    {
        float value = actionRef.GetFloat();
        return Mathf.Pow(value, exponent);
    }
    
    /// <summary>
    /// 将输入映射到特定范围
    /// </summary>
    public static float MapToRange(this InputActionReference actionRef, float min, float max)
    {
        float value = actionRef.GetFloat();
        return Mathf.Lerp(min, max, value);
    }
    
    /// <summary>
    /// 将2D输入映射到球面坐标
    /// </summary>
    public static Vector3 MapToSphere(this InputActionReference actionRef, float radius = 1f)
    {
        Vector2 input = actionRef.GetVector2();
        float magnitude = input.magnitude;
        
        if (magnitude == 0) return Vector3.zero;
        
        float angle = magnitude * Mathf.PI;
        float sinAngle = Mathf.Sin(angle);
        
        return new Vector3(
            input.x / magnitude * sinAngle * radius,
            input.y / magnitude * sinAngle * radius,
            Mathf.Cos(angle) * radius
        );
    }
    
    #endregion

    #region 辅助方法
    
    private static Vector2 ApplyDeadzone(Vector2 input, float deadzone)
    {
        float magnitude = input.magnitude;
        if (magnitude < deadzone)
        {
            return Vector2.zero;
        }
        
        // 应用径向死区
        float normalizedMagnitude = (magnitude - deadzone) / (1f - deadzone);
        return input.normalized * normalizedMagnitude;
    }
    
    private static bool IsDefault(InputHistoryEvent historyEvent)
    {
        return string.IsNullOrEmpty(historyEvent.actionPath) && historyEvent.time == 0 && historyEvent.value == 0;
    }
    
    private static float GetPressTime(InputActionReference actionRef)
    {
        if (actionRef?.action == null || !actionRef.action.IsPressed()) return float.MaxValue;
        
        // 简化实现
        var history = InputHistoryManager.GetHistory(actionRef);
        var lastPress = history.FindLast(e => e.type == InputEventType.Pressed);
        return !IsDefault(lastPress) ? Time.time - lastPress.time : float.MaxValue;
    }
    
    #endregion

    #region 静态批量操作
    
    /// <summary>
    /// 批量启用多个InputActionReference
    /// </summary>
    public static void EnableAll(params InputActionReference[] actionRefs)
    {
        foreach (var actionRef in actionRefs)
        {
            actionRef.SafeEnable();
        }
    }
    
    /// <summary>
    /// 批量禁用多个InputActionReference
    /// </summary>
    public static void DisableAll(params InputActionReference[] actionRefs)
    {
        foreach (var actionRef in actionRefs)
        {
            actionRef.SafeDisable();
        }
    }
    
    /// <summary>
    /// 批量检查是否有任意一个按键被按下
    /// </summary>
    public static bool AnyPressed(params InputActionReference[] actionRefs)
    {
        foreach (var actionRef in actionRefs)
        {
            if (actionRef.WasPressed())
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 批量检查是否所有按键都在按住状态
    /// </summary>
    public static bool AllPressed(params InputActionReference[] actionRefs)
    {
        foreach (var actionRef in actionRefs)
        {
            if (!actionRef.IsPressed())
                return false;
        }
        return true;
    }
    
    /// <summary>
    /// 批量检查是否有任意一个按键被释放
    /// </summary>
    public static bool AnyReleased(params InputActionReference[] actionRefs)
    {
        foreach (var actionRef in actionRefs)
        {
            if (actionRef.WasReleased())
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 批量获取输入值（Vector2）
    /// </summary>
    public static List<Vector2> GetAllValues(params InputActionReference[] actionRefs)
    {
        var values = new List<Vector2>();
        foreach (var actionRef in actionRefs)
        {
            values.Add(actionRef.GetVector2());
        }
        return values;
    }
    
    #endregion
}

#region 支持类与枚举

/// <summary>
/// 输入设备类型枚举
/// </summary>
public enum InputDeviceType
{
    Unknown,
    Keyboard,
    Mouse,
    Gamepad,
    Touch,
    VR,
    AR
}

/// <summary>
/// 输入事件类型
/// </summary>
public enum InputEventType
{
    Started,
    Pressed,
    Released,
    Performed,
    Canceled
}

/// <summary>
/// 输入历史记录结构
/// </summary>
public struct InputHistoryEvent
{
    public string actionPath;
    public InputEventType type;
    public float time;
    public float value;
    
    public InputHistoryEvent(string path, InputEventType eventType, float eventTime, float eventValue = 0)
    {
        actionPath = path;
        type = eventType;
        time = eventTime;
        value = eventValue;
    }
}

/// <summary>
/// 输入缓冲区
/// </summary>
public class InputBuffer<T>
{
    private readonly int _capacity;
    private readonly Queue<T> _buffer;
    
    public int Count => _buffer.Count;
    
    public InputBuffer(int capacity)
    {
        _capacity = capacity;
        _buffer = new Queue<T>(capacity);
    }
    
    public void Add(T item)
    {
        if (_buffer.Count >= _capacity)
        {
            _buffer.Dequeue();
        }
        _buffer.Enqueue(item);
    }
    
    public IEnumerable<T> GetBuffer()
    {
        return _buffer;
    }
    
    public void Clear()
    {
        _buffer.Clear();
    }
}

#endregion

#region 输入管理器类

/// <summary>
/// 输入历史管理器
/// </summary>
public static class InputHistoryManager
{
    private static readonly Dictionary<string, List<InputHistoryEvent>> _history = 
        new Dictionary<string, List<InputHistoryEvent>>();
    
    private static readonly List<InputHistoryEvent> _sequenceHistory = 
        new List<InputHistoryEvent>();
    
    private static readonly Dictionary<string, List<Delegate>> _eventListeners = 
        new Dictionary<string, List<Delegate>>();
    
    public static void AddHistory(InputActionReference actionRef, InputEventType eventType)
    {
        if (!actionRef.IsValid()) return;
        
        string path = actionRef.GetPath();
        if (!_history.ContainsKey(path))
        {
            _history[path] = new List<InputHistoryEvent>();
        }
        
        float value = 0;
        if (actionRef.action.activeControl?.valueType == typeof(float))
            value = actionRef.action.ReadValue<float>();
        
        var historyEvent = new InputHistoryEvent(path, eventType, Time.time, value);
        _history[path].Add(historyEvent);
        _sequenceHistory.Add(historyEvent);
        
        // 限制历史记录大小
        if (_history[path].Count > 100)
            _history[path].RemoveAt(0);
        
        if (_sequenceHistory.Count > 1000)
            _sequenceHistory.RemoveAt(0);
    }
    
    public static List<InputHistoryEvent> GetHistory(InputActionReference actionRef)
    {
        string path = actionRef?.GetPath();
        return path != null && _history.ContainsKey(path) ? 
            _history[path] : new List<InputHistoryEvent>();
    }
    
    public static List<InputHistoryEvent> GetSequenceHistory()
    {
        return _sequenceHistory;
    }
    
    public static void ClearHistory(InputActionReference actionRef)
    {
        string path = actionRef?.GetPath();
        if (path != null && _history.ContainsKey(path))
        {
            _history[path].Clear();
        }
    }
    
    public static void ClearAllListeners(InputActionReference actionRef)
    {
        string path = actionRef?.GetPath();
        if (path != null && _eventListeners.ContainsKey(path))
        {
            var listeners = _eventListeners[path];
            if (actionRef.action != null)
            {
                foreach (var listener in listeners)
                {
                    if (listener is Action<InputAction.CallbackContext> callback)
                    {
                        actionRef.action.started -= callback;
                        actionRef.action.performed -= callback;
                        actionRef.action.canceled -= callback;
                    }
                }
            }
            _eventListeners[path].Clear();
        }
    }
    
    public static void ClearAllHistory()
    {
        _history.Clear();
        _sequenceHistory.Clear();
    }
    
    public static void AddEventListener(InputActionReference actionRef, Delegate listener)
    {
        string path = actionRef?.GetPath();
        if (path != null)
        {
            if (!_eventListeners.ContainsKey(path))
            {
                _eventListeners[path] = new List<Delegate>();
            }
            _eventListeners[path].Add(listener);
        }
    }
}

/// <summary>
/// 输入缓冲区管理器
/// </summary>
public static class InputBufferManager
{
    private static readonly Dictionary<string, InputBuffer<Vector2>> _vector2Buffers = 
        new Dictionary<string, InputBuffer<Vector2>>();
    
    private static readonly Dictionary<string, InputBuffer<float>> _floatBuffers = 
        new Dictionary<string, InputBuffer<float>>();
    
    public static InputBuffer<Vector2> GetVector2Buffer(InputActionReference actionRef, int bufferSize)
    {
        string path = actionRef.GetPath();
        if (!_vector2Buffers.ContainsKey(path))
        {
            _vector2Buffers[path] = new InputBuffer<Vector2>(bufferSize);
        }
        return _vector2Buffers[path];
    }
    
    public static InputBuffer<float> GetFloatBuffer(InputActionReference actionRef, int bufferSize)
    {
        string path = actionRef.GetPath();
        if (!_floatBuffers.ContainsKey(path))
        {
            _floatBuffers[path] = new InputBuffer<float>(bufferSize);
        }
        return _floatBuffers[path];
    }
    
    public static void ClearAllBuffers()
    {
        _vector2Buffers.Clear();
        _floatBuffers.Clear();
    }
}

#endregion

#region 使用示例

/// <summary>
/// 使用示例
/// </summary>
public class InputExample : MonoBehaviour
{
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference attackAction;
    public InputActionReference blockAction;
    
    private void Start()
    {
        // 1. 基本输入检查
        if (moveAction.WasPressed())
        {
            Debug.Log("移动按键被按下");
        }
        
        // 2. 高级输入检查
        if (jumpAction.IsDoubleClick(0.3f))
        {
            Debug.Log("双击跳跃！执行二段跳");
        }
        
        if (attackAction.IsLongPress(1.0f))
        {
            Debug.Log("长按攻击！蓄力攻击");
        }
        
        // 3. 获取处理后的输入值
        Vector2 smoothedMove = moveAction.GetSmoothedVector2(10);
        Vector2Int direction = moveAction.Get8Direction();
        
        // 4. 设备检测
        if (moveAction.IsGamepadInput())
        {
            Debug.Log("当前使用手柄移动");
        }
        
        // 5. 组合键检查
        if (attackAction.IsComboWith(blockAction))
        {
            Debug.Log("攻击+格挡组合键！执行特殊技能");
        }
        
        // 6. 事件监听
        jumpAction.AddPerformedListener(ctx => 
        {
            Debug.Log($"跳跃触发，强度: {ctx.ReadValue<float>()}");
        });
        
        // 7. 异步等待输入
        WaitForInputAsync();
    }
    
    private async void WaitForInputAsync()
    {
        Debug.Log("等待攻击输入...");
        await attackAction.WaitForPressAsync();
        Debug.Log("攻击输入收到！");
    }
    
    private void Update()
    {
        // 8. 实时输入分析
        float pressFrequency = attackAction.GetPressFrequency();
        if (pressFrequency > 5)
        {
            Debug.Log("攻击频率过高！");
        }
        
        // 9. 输入变换
        float curvedInput = attackAction.ApplyCurve(AnimationCurve.EaseInOut(0, 0, 1, 1));
        
        // 10. 批量操作
        bool anyAction = InputActionReferenceExtensions.AnyPressed(
            moveAction, jumpAction, attackAction, blockAction);
            
        if (anyAction)
        {
            // 有任意按键按下
        }
    }
    
    private void OnDestroy()
    {
        // 清理事件监听
        moveAction?.RemoveAllListeners();
        jumpAction?.RemoveAllListeners();
        attackAction?.RemoveAllListeners();
        blockAction?.RemoveAllListeners();
    }
}

#endregion