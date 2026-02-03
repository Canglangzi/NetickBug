using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public static class PerformanceTester
{
    private static readonly Dictionary<string, List<long>> testResults = new Dictionary<string, List<long>>();
    private static PerformanceConfig config = new PerformanceConfig(); // 全局配置
    private static readonly object lockObj = new object(); // 用于线程安全

    public static PerformanceConfig Config => config; // 公开的 Config 属性

    private static TimingPrint currentTiming;


    public class PerformanceConfig
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Info;
        public bool AutoSave { get; set; } = false;
        public string FileName { get; set; } = "performance_log.csv";
        public SaveFormat Format { get; set; } = SaveFormat.CSV; // 默认 CSV 格式
        public string FilePath => Path.Combine(Application.persistentDataPath, FileName);
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    public enum SaveFormat
    {
        CSV,
        JSON
    }

    public delegate void CustomResultHandler(string label, long elapsedMilliseconds);

    private static CustomResultHandler customResultHandler;

    public static void Configure(Action<PerformanceConfig> configure)
    {
        configure?.Invoke(config);
    }

    public static void RegisterCustomResultHandler(CustomResultHandler handler)
    {
        customResultHandler = handler;
    }

    // 同步操作测量（支持泛型）
    public static T Measure<T>(string label, Func<T> func)
    {
        return Measure(label, () => func(), config.AutoSave);
    }

    // 同步操作测量（支持 void Action）
    public static void Measure(string label, Action action)
    {
        Measure(label, action, config.AutoSave);
    }

    // 同步操作测量（支持带自定义保存参数）
    public static T Measure<T>(string label, Func<T> func, bool autoSave)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        T result = func();
        stopwatch.Stop();

        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        RecordResult(label, elapsedMilliseconds);

        if (autoSave)
        {
            AutoSaveResults(config.FileName, config.Format);
        }
        return result;
    }
    public static void Measure(string label, Action action, bool autoSave)
    {
        var timing = new TimingPrint(label);
        currentTiming.AddChild(timing); // Add child timing

        Stopwatch stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();

        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        RecordResult(label, elapsedMilliseconds);

        if (autoSave)
        {
            AutoSaveResults(config.FileName, config.Format);
        }

        timing.AddTime(elapsedMilliseconds);
    }

    public static void EndTiming()
    {
        if (currentTiming != null)
        {
            string result = currentTiming.Print(); // 获取最终的输出
            UnityEngine.Debug.Log(result); // 只在这里输出结果
            currentTiming = null; // 重置
        }
    }

    public static void StartTiming(string label)
    {
        currentTiming = new TimingPrint(label);
    }

    public static void PrintResults()
    {
        currentTiming?.Print();
    }

    // 异步操作测量（支持 Func<UniTask> 和 Func<UniTask<T>>）
    public static async UniTask<T> MeasureAsync<T>(string label, Func<UniTask<T>> asyncFunc)
    {
        return await MeasureAsync(label, asyncFunc, config.AutoSave);
    }

    public static async UniTask<long> MeasureAsync(string label, Func<UniTask> asyncAction)
    {
        return await MeasureAsync(label, asyncAction, config.AutoSave);
    }

    public static async UniTask<T> MeasureAsync<T>(string label, Func<UniTask<T>> asyncFunc, bool autoSave)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        T result = await asyncFunc();
        stopwatch.Stop();

        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        RecordResult(label, elapsedMilliseconds);

        if (autoSave)
        {
            AutoSaveResults(config.FileName, config.Format);
        }

        return result;
    }

    public static async UniTask<long> MeasureAsync(string label, Func<UniTask> asyncAction, bool autoSave)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        await asyncAction();
        stopwatch.Stop();

        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        RecordResult(label, elapsedMilliseconds);

        if (autoSave)
        {
            AutoSaveResults(config.FileName, config.Format);
        }

        return elapsedMilliseconds;
    }

    // 记录测试结果（线程安全）
    private static void RecordResult(string label, long elapsedMilliseconds)
    {
        lock (lockObj)
        {
            if (!testResults.ContainsKey(label))
            {
                testResults[label] = new List<long>();
            }

            testResults[label].Add(elapsedMilliseconds);
        }

        Log(LogLevel.Info, $"[Performance Test] {label} took {elapsedMilliseconds} ms to execute.");

        // 调用自定义结果处理程序
        customResultHandler?.Invoke(label, elapsedMilliseconds);
    }

    // 自动保存结果
    public static void AutoSaveResults(string fileName, SaveFormat format)
    {
        try
        {
            switch (format)
            {
                case SaveFormat.CSV:
                    SaveResultsToCsv(fileName);
                    break;
                case SaveFormat.JSON:
                    SaveResultsToJson(fileName);
                    break;
                default:
                    Log(LogLevel.Warning, $"[AutoSave] Unsupported save format: {format}");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"[AutoSave] Failed to auto-save results. Error: {ex.Message}");
        }
    }

    // 保存结果到 CSV 文件
    private static void SaveResultsToCsv(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(CreateTableHeader());
                writer.WriteLine(CreateTableSeparator());

                foreach (var kvp in testResults)
                {
                    string label = kvp.Key;
                    List<long> results = kvp.Value;
                    double averageTime = GetAverageExecutionTime(label);
                    long minTime = GetMinExecutionTime(label);
                    long maxTime = GetMaxExecutionTime(label);
                    writer.WriteLine(CreateTableRow(label, minTime, maxTime, averageTime, results));
                    writer.WriteLine(CreateTableSeparator()); // 行分隔符
                }
            }

            Log(LogLevel.Info, $"[CSV Save] Performance log successfully saved at: {filePath}");
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"[CSV Save] Failed to save results to CSV. Error: {ex.Message}");
        }
    }

    // 保存结果到 JSON 文件
    private static void SaveResultsToJson(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        var jsonData = new Dictionary<string, object>();

        foreach (var kvp in testResults)
        {
            string label = kvp.Key;
            List<long> results = kvp.Value;
            jsonData[label] = new
            {
                MinTime = GetMinExecutionTime(label),
                MaxTime = GetMaxExecutionTime(label),
                AverageTime = GetAverageExecutionTime(label),
                ExecutionTimes = results,
                NumberOfTests = results.Count
            };
        }

        try
        {
            File.WriteAllText(filePath, JsonUtility.ToJson(jsonData, true));
            Log(LogLevel.Info, $"[JSON Save] Performance log successfully saved at: {filePath}");
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"[JSON Save] Failed to save results to JSON. Error: {ex.Message}");
        }
    }

    // 获取平均执行时间
    public static double GetAverageExecutionTime(string label)
    {
        if (!testResults.ContainsKey(label) || testResults[label].Count == 0)
        {
            Log(LogLevel.Warning, $"[Stats] No results found for label: {label}");
            return -1;
        }

        long total = testResults[label].Sum();
        return total / (double)testResults[label].Count;
    }

    // 获取最小执行时间
    public static long GetMinExecutionTime(string label)
    {
        if (!testResults.ContainsKey(label) || testResults[label].Count == 0)
        {
            Log(LogLevel.Warning, $"[Stats] No results found for label: {label}");
            return -1;
        }

        return testResults[label].Min();
    }

    // 获取最大执行时间
    public static long GetMaxExecutionTime(string label)
    {
        if (!testResults.ContainsKey(label) || testResults[label].Count == 0)
        {
            Log(LogLevel.Warning, $"[Stats] No results found for label: {label}");
            return -1;
        }

        return testResults[label].Max();
    }

    // 清空测试结果
    public static void ClearResults()
    {
        lock (lockObj)
        {
            testResults.Clear();
        }

        Log(LogLevel.Info, "[Clear] Test results have been cleared.");
    }

    // 日志输出
    private static void Log(LogLevel level, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string formattedMessage = $"[{timestamp}] {message}";

        if (level >= config.LogLevel)
        {
            switch (level)
            {
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
        }
    }

    private static string CreateTableHeader()
    {
        return "Label".PadRight(30) + "| " +
               "Min Time (ms)".PadRight(17) + "| " +
               "Max Time (ms)".PadRight(17) + "| " +
               "Average Time (ms)".PadRight(21) + "| " +
               "Execution Times (ms)".PadRight(35) + "| " +
               "Number of Tests".PadRight(20);
    }

    private static string CreateTableSeparator()
    {
        return new string('-', 30) + "+ " +
               new string('-', 17) + "+ " +
               new string('-', 17) + "+ " +
               new string('-', 21) + "+ " +
               new string('-', 35) + "+ " +
               new string('-', 20);
    }

    private static string CreateTableRow(string label, long minTime, long maxTime, double averageTime, List<long> results)
    {
        string formattedTimes = string.Join("; ", results.Select(t => t.ToString()).ToArray());
        return label.PadRight(30) + "| " +
               minTime.ToString().PadLeft(17) + "| " +
               maxTime.ToString().PadLeft(17) + "| " +
               averageTime.ToString("0.00").PadLeft(21) + "| " +
               formattedTimes.PadRight(35) + "| " +
               results.Count.ToString().PadLeft(20);
    }
}

}