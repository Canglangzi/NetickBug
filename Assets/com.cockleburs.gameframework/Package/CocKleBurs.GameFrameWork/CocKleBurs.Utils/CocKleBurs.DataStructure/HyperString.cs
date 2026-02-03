using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public static class HyperString
{
    // 核心结构体 - 100% 安全代码
    //public ref struct TextBuilder
    public  struct TextBuilder : IDisposable
    {
        // 使用固定大小内联缓冲区避免堆分配
        private const int InlineBufferSize = 128;
        private InlineCharBuffer _buffer;
        private char[] _heapBuffer;
        private int _length;
        
        // 结构初始化改为属性初始化
        // public TextBuilder()
        // {
        //  
        //     _buffer = default;
        //     _heapBuffer = null;
        //     _length = 0;
        // }
        //
        // 内联缓冲区结构体
        [StructLayout(LayoutKind.Sequential, Size = InlineBufferSize * sizeof(char))]
        private struct InlineCharBuffer
        {
            // 固定大小缓冲区
            private unsafe fixed char _buffer[InlineBufferSize];
            
            public Span<char> AsSpan()
            {
                unsafe
                {
                    fixed(char* ptr = _buffer)
                    {
                        return new Span<char>(ptr, InlineBufferSize);
                    }
                }
            }
        }

        // 获取当前有效缓冲区
        private Span<char> CurrentBuffer
        {
            get
            {
                if (_heapBuffer != null) 
                    return _heapBuffer.AsSpan();
                
                return _buffer.AsSpan();
            }
        }
        
        #region 核心追加方法
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(ReadOnlySpan<char> value)
        {
            if (value.IsEmpty) return;
            
            if (!HasCapacity(value.Length))
            {
                Grow(value.Length);
            }
            
            value.CopyTo(CurrentBuffer.Slice(_length));
            _length += value.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(string value) => Append(value.AsSpan());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(UnityEngine.Object obj) => 
            Append(obj != null ? obj.name : "null");
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(char c)
        {
            if (!HasCapacity(1))
            {
                Grow(1);
            }
            
            CurrentBuffer[_length++] = c;
        }
        #endregion

        #region 高性能值类型追加
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(int value)
        {
            // 确保有足够空间处理32位整数
            if (!HasCapacity(16))
            {
                Grow(16);
            }
            
            Span<char> buffer = CurrentBuffer.Slice(_length);
            if (value.TryFormat(buffer, out int charsWritten, default, null))
            {
                _length += charsWritten;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(float value)
        {
            // 确保有足够空间处理浮点数
            if (!HasCapacity(32))
            {
                Grow(32);
            }
            
            Span<char> buffer = CurrentBuffer.Slice(_length);
            if (value.TryFormat(buffer, out int charsWritten, "F2", null))
            {
                _length += charsWritten;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(bool value) => Append(value ? "True" : "False");
        #endregion

        #region 缓冲区管理
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasCapacity(int required) => 
            _length + required <= (_heapBuffer != null ? _heapBuffer.Length : InlineBufferSize);
        
        private void Grow(int additional)
        {
            int newSize = Math.Max(_length + additional, Math.Max(_length * 2, InlineBufferSize * 2));
            
            // 迁移到堆缓冲区
            char[] newBuffer = ArrayPool<char>.Shared.Rent(newSize);
            
            // 复制现有数据
            CurrentBuffer.Slice(0, _length).CopyTo(newBuffer);
            
            // 释放原有堆缓冲区（如果有）
            if (_heapBuffer != null)
            {
                ArrayPool<char>.Shared.Return(_heapBuffer);
            }
            
            _heapBuffer = newBuffer;
        }
        #endregion

        #region 输出处理
        public string ToString()
        {
            if (_length == 0) return string.Empty;
            
            return _heapBuffer != null ? 
                new string(_heapBuffer, 0, _length) : 
                new string(CurrentBuffer.Slice(0, _length).ToArray()); // 安全转换为新数组
        }

        public void Dispose()
        {
            // TODO 在此释放托管资源
        }

        #endregion
    }

    // ================ 智能API封装 ================
    public static class Concat
    {
        public static string Strings(string a, string b)
        {
            var builder = new TextBuilder();
            builder.Append(a);
            builder.Append(b);
            string result = builder.ToString();
            builder.Dispose();
            return result;
        }
        
        public static string WithInt(string prefix, int value)
        {
            var builder = new TextBuilder();
            builder.Append(prefix);
            builder.Append(value);
            string result = builder.ToString();
            builder.Dispose();
            return result;
        }
        // 在 HyperString.Concat 类中添加
        public static string Build(params object[] args)
        {
            if (args == null || args.Length == 0) 
                return string.Empty;
    
            var builder = new TextBuilder();
            try
            {
                foreach (object arg in args)
                {
                    switch (arg)
                    {
                        case string str:
                            builder.Append(str);
                            break;
                        case int i:
                            builder.Append(i);
                            break;
                        case float f:
                            builder.Append(f);
                            break;
                        case bool b:
                            builder.Append(b);
                            break;
                        case UnityEngine.Object obj:
                            builder.Append(obj);
                            break;
                        case char c:
                            builder.Append(c);
                            break;
                        case IStringConvertible custom:
                            custom.AppendTo(ref builder);
                            break;
                        default:
                            builder.Append(arg?.ToString() ?? "null");
                            break;
                    }
                }
                return builder.ToString();
            }
            finally
            {
                builder.Dispose();
            }
        }

// 自定义字符串转换接口（添加在 HyperString 命名空间内）
        public interface IStringConvertible
        {
            void AppendTo(ref TextBuilder builder);
        }
    }
    
    public static class Log
    {
        public static void Colored(string message, Color color)
        {
            var builder = new TextBuilder();
            AppendColorHeader(ref builder, color);
            builder.Append(message);
            builder.Append("</color>");
            Debug.Log(builder.ToString());
            builder.Dispose();
        }

        public static void AppendColorHeader(ref TextBuilder builder, Color color)
        {
            builder.Append("<color=#");
            
            // 安全的方式处理颜色转换
            ColorToHex(ref builder, color);
            builder.Append('>');
        }
        
        private static void ColorToHex(ref TextBuilder builder, Color color)
        {
            // 逐个字符处理确保安全
            AppendHexChar(ref builder, (int)(color.r * 255) >> 4);
            AppendHexChar(ref builder, (int)(color.r * 255) & 0x0F);
            AppendHexChar(ref builder, (int)(color.g * 255) >> 4);
            AppendHexChar(ref builder, (int)(color.g * 255) & 0x0F);
            AppendHexChar(ref builder, (int)(color.b * 255) >> 4);
            AppendHexChar(ref builder, (int)(color.b * 255) & 0x0F);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AppendHexChar(ref TextBuilder builder, int value)
        {
            builder.Append((char)(value < 10 ? '0' + value : 'A' + value - 10));
        }
    }
}
}