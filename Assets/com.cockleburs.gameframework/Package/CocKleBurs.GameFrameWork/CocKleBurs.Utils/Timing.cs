using System.Collections.Generic;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public class TimingPrint
{
    public string Name { get; }
    public long ElapsedMilliseconds { get; private set; }
    public List<TimingPrint> Children { get; }

    public TimingPrint(string name)
    {
        Name = name;
        Children = new List<TimingPrint>();
    }

    public void AddChild(TimingPrint child)
    {
        Children.Add(child);
    }

    public void AddTime(long time)
    {
        ElapsedMilliseconds += time;
    }

    public string Print(int indent = 0)
    {
        // 设置缩进和格式化输出
        string indentString = new string(' ', indent);
        string formattedTime = $"{ElapsedMilliseconds,6}ms"; // 右对齐时间
        string output = $"{indentString}- {Name.PadRight(40)}: {formattedTime}";

        // 处理子计时并收集结果
        List<string> childOutputs = new List<string> { output };
        foreach (var child in Children)
        {
            childOutputs.Add(child.Print(indent + 4)); // 增加缩进
        }

        // 拼接所有输出结果，用换行符分隔
        return string.Join("\n", childOutputs);
    }
}

}