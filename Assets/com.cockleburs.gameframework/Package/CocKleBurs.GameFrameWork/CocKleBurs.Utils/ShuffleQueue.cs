using System;
using System.Collections.Generic;


namespace CockleBurs.GameFramework.Utility
{
public class ShuffleQueue<T>
{
    //存储元素
    private Queue<T> _sequence;
    // 存储上次打乱顺序的元素
    private List<T> _lastShuffled;
    private Random _random;

    // 构造函数，接受一个 IEnumerable<T> 类型的参数来初始化队列
    public ShuffleQueue(IEnumerable<T> data) {
        _sequence = new Queue<T>(data);
        _lastShuffled = new List<T>();
        _random = new Random();
    }

    // 定义一个 GetNext 方法用于取出并删除队首元素
    public T GetNext() {
        // 如果队列为空，则调用 Shuffle 方法来打乱顺序并重新填充队列
        if (_sequence.Count == 0)
            Shuffle();
        // 取出并删除队首元素
        T next = _sequence.Dequeue();
        // 将取出的元素添加到上次打乱顺序的元素列表中
        _lastShuffled.Add(next);
        return next;
    }

    // 定义一个 Shuffle 方法用于打乱顺序并重新填充队列
    private void Shuffle() {
        // 获取上次打乱顺序的元素列表的元素个数
        int count = _lastShuffled.Count;
        // 获取上次打乱顺序的末尾元素
        T lastElement = _lastShuffled[count - 1];

        // 使用 Knuth-Durstenfeld Shuffle算法来随机打乱临时列表中的元素顺序
        do
        {
            for (int i = count - 1; i > 0; i--)
            {
                int randomIndex = _random.Next(0, count);
                // 使用元组进行析构交换元素
                (_lastShuffled[randomIndex], _lastShuffled[i]) = (_lastShuffled[i], _lastShuffled[randomIndex]);
            }
            // 检查随机打乱后的首位元素是否与上次打乱顺序的末尾元素相同，如果相同，则重新打乱顺序
        } while (_lastShuffled[0].Equals(lastElement));

        // 将临时列表中的元素依次添加到队列中
        foreach (T item in _lastShuffled)
        {
            _sequence.Enqueue(item);
        }

        // 清空上次打乱顺序的元素列表
        _lastShuffled.Clear();
    }
}
}