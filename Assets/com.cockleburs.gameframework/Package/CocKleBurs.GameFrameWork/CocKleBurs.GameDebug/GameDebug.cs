using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using CockleBurs.GameFramework.Core;
using CockleBurs.GameFramework.Utility;
using Netick.Unity;

using UnityEditor;
using Debug = UnityEngine.Debug;

namespace  CockleBurs.GameFramework.Core
{

    public class GameDebugLogger : SandboxSingleton<GameDebugLogger>
    {
        private static List<LogItem> logs = new List<LogItem>();

        public static event Action<LogItem> OnLogAdded;

        // 各种日志类型的开关
        private static bool eventLoggingEnabled = true;
        private static bool networkingLoggingEnabled = true;
        private static bool clientLoggingEnabled = true;
        private static bool serverLoggingEnabled = true;
        private static bool hostLoggingEnabled = true;
        private static bool databaseLoggingEnabled = true;
        private static bool uiLoggingEnabled = true;
        private static bool inputLoggingEnabled = true;
        private static bool audioLoggingEnabled = true;
        private static bool performanceLoggingEnabled = true;
        private static bool debugLoggingEnabled = true;

        // 全局日志类型开关
        private static bool logInfoEnabled = true;
        private static bool logWarningEnabled = true;
        private static bool logErrorEnabled = true;

        public static bool UnityDebugVisible = true;
        public static bool debugGUIVisible = true;

        // 控制每条日志显示时间的变量（单位：秒）
        public static float logDisplayTime = 5.0f;

        private static Font cachedFont;

        public enum LogType
        {
            Info,
            Warning,
            Error
        }
        [Serializable]
        public struct LogItem
        {
            public string Message;
            public LogType Type;
            public string Timestamp;
            public float CreationTime;

            public LogItem(string prefix, string message, LogType type)
            {
                Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                Message = $"[{Timestamp}] {message}"; // 这里只保留时间戳
                Type = type;
                CreationTime = Time.time;
            }
        }

        private static string GetColorForLogType(LogType type)
        {
            switch (type)
            {
                case LogType.Info:
                    return "#00FF00"; // 绿色
                case LogType.Warning:
                    return "#FFFF00"; // 黄色
                case LogType.Error:
                    return "#FF0000"; // 红色
                default:
                    return "#FFFFFF"; // 默认白色
            }
        }

        private static string GetColorForPrefix(string prefix)
        {
            switch (prefix)
            {
                case "Event":
                    return "#FF5722"; // 鳄梨橙（非常鲜艳的橙色）
                case "Networking":
                    return "#03A9F4"; // 天蓝色（明亮的蓝色）
                case "Client":
                    return "#8BC34A"; // 豆绿色（亮绿色）
                case "Server":
                    return "#FFC107"; // 亮金色（鲜艳的金色）
                case "Host":
                    return "#F44336"; // 红色（亮红色）
                case "Database":
                    return "#9C27B0"; // 紫色（鲜艳的紫色）
                case "UI":
                    return "#4CAF50"; // 绿色（鲜亮的绿色）
                case "Input":
                    return "#FF9800"; // 橙色（鲜亮的橙色）
                case "Audio":
                    return "#607D8B"; // 蓝灰色（沉稳的蓝灰色）
                case "Performance":
                    return "#3F51B5"; // 深蓝色（非常醒目的蓝色）
                case "Debug":
                    return "#009688"; // 蓝绿色（清新的蓝绿色）
                case "Warning":
                    return "#FFEB3B"; // 亮黄色（警告色）
                case "Custom":
                    return "#795548"; // 咖啡色（温暖的咖啡色）
                case "Trace":
                    return "#9E9E9E"; // 中灰色（适合追踪日志）
                case "Network":
                    return "#D32F2F"; // 深红色（适合网络日志）
                default:
                    return "#E0E0E0"; // 亮灰色（较为中性的颜色）
                case "Initialize":
                    return "#FF00FF"; // 荧光红绿色，表示成功的初始化

            }
        }
      
        public static class TextBuilderPool
        {
            [ThreadStatic] // 每个线程独立池
            private static Stack<HyperString.TextBuilder> _pool;

            public static HyperString.TextBuilder Get()
            {
                _pool ??= new Stack<HyperString.TextBuilder>(4);
                return _pool.Count > 0 ? _pool.Pop() : new HyperString.TextBuilder();
            }

            public static void Return(ref HyperString.TextBuilder builder)
            {
                builder.Dispose(); // 重置状态
                _pool.Push(builder);
            }
        }
        private static void LogWithPrefix(string prefix, string message, LogType type, object debugObject = null)
        {
            if (!IsLoggingEnabledForPrefix(prefix) || !IsLoggingEnabledForLogType(type)) 
                return;

            LogItem item = new LogItem(prefix, message, type);
            logs.Add(item);

            // 使用 HyperString 构建带颜色的日志消息
            var builder = TextBuilderPool.Get();
            try
            {
                // 添加前缀颜色
                builder.Append("<color=");
                builder.Append(GetColorForPrefix(prefix));
                builder.Append(">[");
                builder.Append(prefix);
                builder.Append("]</color> ");

                // 添加箭头颜色
                builder.Append("<color=#00FFFF>-->> </color>");

                // 添加消息颜色
                builder.Append("<color=");
                builder.Append(GetColorForLogType(type));
                builder.Append(">");
                builder.Append(message);
                builder.Append("</color>");

                string formattedMessage = builder.ToString();
                UnityEngine.Object unityDebugObject = debugObject as UnityEngine.Object;

                switch (type)
                {
                    case LogType.Info:
#if DEVELOPER
                        if (UnityDebugVisible)
                            Debug.Log(formattedMessage, unityDebugObject);
#endif
                        break;
                    case LogType.Warning:
#if DEVELOPER
                        if (UnityDebugVisible)
                            Debug.LogWarning(formattedMessage, unityDebugObject);
#endif
                        break;
                    case LogType.Error:
#if DEVELOPER
                        if (UnityDebugVisible)
                            Debug.LogError(formattedMessage, unityDebugObject);
#endif
                        break;
                }
            }
            finally
            {
                builder.Dispose(); // 确保释放缓冲区
            }

            OnLogAdded?.Invoke(item);
        }

        private static bool IsLoggingEnabledForPrefix(string prefix)
        {
            switch (prefix)
            {
                case "Event":
                    return eventLoggingEnabled;
                case "Networking":
                    return networkingLoggingEnabled;
                case "Client":
                    return clientLoggingEnabled;
                case "Server":
                    return serverLoggingEnabled;
                case "Host":
                    return hostLoggingEnabled;
                case "Database":
                    return databaseLoggingEnabled;
                case "UI":
                    return uiLoggingEnabled;
                case "Input":
                    return inputLoggingEnabled;
                case "Audio":
                    return audioLoggingEnabled;
                case "Performance":
                    return performanceLoggingEnabled;
                case "Debug":
                    return debugLoggingEnabled;
                default:
                    return true;
            }
        }

        private static bool IsLoggingEnabledForLogType(LogType type)
        {
            switch (type)
            {
                case LogType.Info:
                    return logInfoEnabled;
                case LogType.Warning:
                    return logWarningEnabled;
                case LogType.Error:
                    return logErrorEnabled;
                default:
                    return true;
            }
        }
        // public static void Log(this NetworkSandbox sandbox, object message, UnityEngine.Object context)
        // {
        //    Debug.Log($"[{sandbox.Name}] {message}", context);
        // }
        // public static void LogWarning(this NetworkSandbox sandbox, object message, UnityEngine.Object context)
        // {
        //    Debug.LogWarning($"[{sandbox.Name}] {message}", context);
        // }
        // public static void LogError(this NetworkSandbox sandbox, object message, UnityEngine.Object context)
        // {
        //    Debug.LogError($"[{sandbox.Name}] {message}", context);
        // }
        public static void Log(string message, object debugObject = null)
        {
            LogWithPrefix("Info", message, LogType.Info, debugObject);
        }
        public static void Log(string message, params object[] values)
        {
            LogWithPrefix("Info", message, LogType.Info, values);
        }
        public static void LogWarning(string message, object debugObject = null)
        {
            LogWithPrefix("Warning", message, LogType.Warning, debugObject);
        }

        public static void LogError(string message, object debugObject = null)
        {
            LogWithPrefix("Error", message, LogType.Error, debugObject);
        }

        public static void LogEvent(string eventName, object value = null, object debugObject = null)
        {
            LogWithPrefix("Event", $"Event '{eventName}' triggered, Value: {value}", LogType.Info, debugObject);
        }

        public static void LogClient(string message, object debugObject = null)
        {
            LogWithPrefix("Client", message, LogType.Info, debugObject);
        }

        public static void LogServer(string message, object debugObject = null)
        {
            LogWithPrefix("Server", message, LogType.Info, debugObject);
        }

        public static void LogHost(string message, object debugObject = null)
        {
            LogWithPrefix("Host", message, LogType.Info, debugObject);
        }

        //TODO:或许这里应该使用Netick的沙盒对于所有网络Debug
        public static void LogNetworking(string message, object debugObject = null)
        {
            LogWithPrefix("Networking", message, LogType.Info, debugObject);
        }

        public static void LogDatabase(string message, object debugObject = null)
        {
            LogWithPrefix("Database", message, LogType.Info, debugObject);
        }

        public static void LogUI(string message, object debugObject = null)
        {
            LogWithPrefix("UI", message, LogType.Info, debugObject);
        }

        public static void LogInput(string message, object debugObject = null)
        {
            LogWithPrefix("Input", message, LogType.Info, debugObject);
        }

        public static void LogAudio(string message, object debugObject = null)
        {
            LogWithPrefix("Audio", message, LogType.Info, debugObject);
        }

        public static void LogPerformance(string message, object debugObject = null)
        {
            LogWithPrefix("Performance", message, LogType.Info, debugObject);
        }

        public static void LogDebug(string message, object debugObject = null)
        {
            LogWithPrefix("Debug", message, LogType.Info, debugObject);
        }
        public static void LogInitialize(string message, object debugObject = null)
        {
            LogWithPrefix("Initialize", message, LogType.Info, debugObject);
        }

        // 新增的日志 API
        public static void LogInfo(string prefix, string message)
        {
            LogWithPrefix(prefix, message, LogType.Info);
        }

        public static void LogWarning(string prefix, string message)
        {
            LogWithPrefix(prefix, message, LogType.Warning);
        }

        public static void LogError(string prefix, string message)
        {
            LogWithPrefix(prefix, message, LogType.Error);
        }

        public static void LogCustom(string prefix, string message, LogType type)
        {
            LogWithPrefix(prefix, message, type);
        }

        public static void EnableLoggingForPrefix(string prefix, bool enable)
        {
            switch (prefix)
            {
                case "Event":
                    eventLoggingEnabled = enable;
                    break;
                case "Networking":
                    networkingLoggingEnabled = enable;
                    break;
                case "Client":
                    clientLoggingEnabled = enable;
                    break;
                case "Server":
                    serverLoggingEnabled = enable;
                    break;
                case "Host":
                    hostLoggingEnabled = enable;
                    break;
                case "Database":
                    databaseLoggingEnabled = enable;
                    break;
                case "UI":
                    uiLoggingEnabled = enable;
                    break;
                case "Input":
                    inputLoggingEnabled = enable;
                    break;
                case "Audio":
                    audioLoggingEnabled = enable;
                    break;
                case "Performance":
                    performanceLoggingEnabled = enable;
                    break;
                case "Debug":
                    debugLoggingEnabled = enable;
                    break;
            }
        }

        public static void EnableLogType(LogType type, bool enable)
        {
            switch (type)
            {
                case LogType.Info:
                    logInfoEnabled = enable;
                    break;
                case LogType.Warning:
                    logWarningEnabled = enable;
                    break;
                case LogType.Error:
                    logErrorEnabled = enable;
                    break;
            }
        }

        public static List<LogItem> GetLogItems()
        {
            return logs;
        }

        public static void ClearLogs()
        {
            logs.Clear();
        }

        public static void ToggleDebug(bool visible)
        {
            debugGUIVisible = visible;
        }

        public static void DrawDebugGUI()
        {
            if (!debugGUIVisible || logs.Count == 0) return;

            // Check if the font is already cached
            if (cachedFont == null)
            {
                // Load the font from Resources and cache it
                cachedFont = Resources.Load<Font>("Oswald-Header");

                // Check if font is loaded
                if (cachedFont == null)
                {
                    LogError("Custom font 'Oswald-Header' not found in Resources.");
                    return;
                }
            }

            GUIStyle style = new GUIStyle
            {
                font = cachedFont, // Apply the cached font
                normal = { textColor = new Color(0.2f, 0.6f, 1f) }, // 更亮的蓝色
                fontSize = 12
            };

            GUIStyle titleStyle = new GUIStyle(style)
            {
                fontSize = 14, // Slightly larger font size for titles
                fontStyle = FontStyle.Bold
            };

            // Begin GUI group
            GUI.BeginGroup(new Rect(10, 10, 300, Screen.height - 20)); // Define a rectangular area for the GUI

            float y = 10; // Start y position
            float currentTime = Time.time;

            // Draw logs with display time control
            for (int i = logs.Count - 1; i >= 0; i--)
            {
                var log = logs[i];
                if (currentTime - log.CreationTime <= logDisplayTime)
                {
                    GUI.Label(new Rect(10, y, 270, 20), log.Message, style); // Draw each log message
                    y += 20; // Increment y position for the next log
                }
                else
                {
                    logs.RemoveAt(i); // Remove old logs
                }
            }

            // End GUI group
            GUI.EndGroup();
        }

        public static void ClearFontCache()
        {
            cachedFont = null;
        }


        public static void DrawScreenText(string text, Vector2 position, Color color, int fontSize = 14)
        {
            GUIStyle style = new GUIStyle
            {
                fontSize = fontSize,
                normal = new GUIStyleState { textColor = color }
            };
#if UNITY_EDITOR
            Handles.BeginGUI();
#endif
            GUILayout.BeginArea(new Rect(position.x, position.y, 200, 50));
            GUILayout.Label(text, style);
            GUILayout.EndArea();
#if UNITY_EDITOR
            Handles.EndGUI();
#endif
        }

        // 断言
        public static void AssertNotNull(object obj, string message)
        {
            if (obj == null)
            {
                Debug.LogError(message);
                Debug.Assert(obj != null, message);
                return;
            }
        }
        public static void Assert(bool condition)
        {
            if (!condition)
                throw new ApplicationException("GAME ASSERT FAILED");
        }

        public static void Assert(bool condition, string msg)
        {
            if (!condition)
                throw new ApplicationException("GAME ASSERT FAILED : " + msg);
        }

        public static void Assert<T>(bool condition, string format, T arg1)
        {
            if (!condition)
                throw new ApplicationException("GAME ASSERT FAILED : " + string.Format(format, arg1));
        }

        public static void Assert<T1, T2>(bool condition, string format, T1 arg1, T2 arg2)
        {
            if (!condition)
                throw new ApplicationException("GAME ASSERT FAILED : " + string.Format(format, arg1, arg2));
        }

        public static void Assert<T1, T2, T3>(bool condition, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            if (!condition)
                throw new ApplicationException("GAME ASSERT FAILED : " + string.Format(format, arg1, arg2, arg3));
        }

        public static void Assert<T1, T2, T3, T4>(bool condition, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (!condition)
                throw new ApplicationException("GAME ASSERT FAILED : " + string.Format(format, arg1, arg2, arg3, arg4));
        }

        public static void Assert<T1, T2, T3, T4, T5>(bool condition, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (!condition)
                throw new ApplicationException("GAME ASSERT FAILED : " + string.Format(format, arg1, arg2, arg3, arg4, arg5));

        }
        public enum DebugType
        {
            Log,
            Warning,
            Error
        }

        private static string logMessage = "";
        private static string warningMessage = "";
        private static string errorMessage = "";

        public static void Log(DebugType type, string message, object context = null)
        {
            // 根据不同的类型添加消息
            switch (type)
            {
                case DebugType.Log:
                    logMessage += message + " "; // 普通日志
                    break;
                case DebugType.Warning:
                    warningMessage += "<color=yellow>" + message + "</color> "; // 警告
                    break;
                case DebugType.Error:
                    errorMessage += "<color=red>" + message + "</color> "; // 错误
                    break;
            }

            // 输出所有消息
            DisplayLogs();
        }

        private static void DisplayLogs()
        {
            // 输出到控制台
            Debug.Log(logMessage);
            Debug.Log(warningMessage);
            Debug.Log(errorMessage);
        }

        public static void Clear()
        {
            logMessage = ""; // 清空日志
            warningMessage = ""; // 清空警告
            errorMessage = ""; // 清空错误
        }
        public static string PrintTimings(TimingPrint timing)
        {
            if (timing == null)
                return string.Empty;

            string result = timing.Print(); // 获取 Timing 的输出
            return result; // 返回字符串结果
        }
        public static string GetLog(LogItem log)
        {
            return log.Message;
        }

        // 获取所有日志
        public static List<LogItem> GetLogs()
        {
            return logs;
        }

    }
}
