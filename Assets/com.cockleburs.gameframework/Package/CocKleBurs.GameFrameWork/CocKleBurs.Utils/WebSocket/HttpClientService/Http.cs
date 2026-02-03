using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


namespace CockleBurs.GameFramework.Utility
{
public static class Http
{
    /// <summary>
    /// 发送 GET 请求并返回字符串响应
    /// </summary>
    public static async Task<string> RequestStringAsync(string url, Dictionary<string, string> headers = null)
    {
        using (var request = UnityWebRequest.Get(url))
        {
            return await SendRequestAsync(request, headers);
        }
    }

    /// <summary>
    /// 发送通用 HTTP 请求
    /// </summary>
    public static async Task<HttpResponse> RequestAsync(string url, string method, object content = null, Dictionary<string, string> headers = null)
    {
        using (var request = CreateRequest(url, method, content))
        {
            var responseText = await SendRequestAsync(request, headers);
            return new HttpResponse
            {
                Success = request.responseCode == 200,
                StatusCode = (int)request.responseCode,
                Data = responseText
            };
        }
    }

    /// <summary>
    /// 发送 POST 请求（忽略响应）
    /// </summary>
    public static async Task RequestAsync(string url, string method, HttpContent content, Dictionary<string, string> headers = null)
    {
        using (var request = CreateRequestWithHttpContent(url, method, content))
        {
            await SendRequestAsync(request, headers);
        }
    }

    /// <summary>
    /// 创建 JSON 内容
    /// </summary>
    public static HttpContent CreateJsonContent(object data)
    {
        return new JsonContent(data);
    }

    /// <summary>
    /// 创建表单内容
    /// </summary>
    public static HttpContent CreateFormContent(Dictionary<string, string> formData)
    {
        return new FormContent(formData);
    }

    private static UnityWebRequest CreateRequest(string url, string method, object content)
    {
        UnityWebRequest request = new UnityWebRequest(url, method);
        
        if (content != null)
        {
            string json = JsonUtility.ToJson(content);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }
        else
        {
            request.downloadHandler = new DownloadHandlerBuffer();
        }

        return request;
    }

    private static UnityWebRequest CreateRequestWithHttpContent(string url, string method, HttpContent content)
    {
        var request = new UnityWebRequest(url, method);
        
        if (content != null)
        {
            content.ApplyToRequest(request);
        }
        else
        {
            request.downloadHandler = new DownloadHandlerBuffer();
        }

        return request;
    }

    private static async Task<string> SendRequestAsync(UnityWebRequest request, Dictionary<string, string> headers)
    {
        // 添加 headers
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }

        // 发送请求并等待完成
        var operation = request.SendWebRequest();
        
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        // 检查错误
        if (request.result != UnityWebRequest.Result.Success)
        {
            throw new HttpException($"HTTP请求失败: {request.error}", (int)request.responseCode);
        }

        return request.downloadHandler?.text ?? string.Empty;
    }
    // HTTP 响应封装
    public class HttpResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Data { get; set; }

        public T GetData<T>() where T : class
        {
            if (string.IsNullOrEmpty(Data)) return null;
            return JsonUtility.FromJson<T>(Data);
        }
    }

// HTTP 内容抽象
    public abstract class HttpContent
    {
        public abstract void ApplyToRequest(UnityWebRequest request);
    }

// JSON 内容实现
    public class JsonContent : HttpContent
    {
        private readonly object _data;

        public JsonContent(object data)
        {
            _data = data;
        }

        public override void ApplyToRequest(UnityWebRequest request)
        {
            string json = JsonUtility.ToJson(_data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }
    }

// 表单内容实现
    public class FormContent : HttpContent
    {
        private readonly Dictionary<string, string> _formData;

        public FormContent(Dictionary<string, string> formData)
        {
            _formData = formData;
        }

        public override void ApplyToRequest(UnityWebRequest request)
        {
            var form = new WWWForm();
            foreach (var entry in _formData)
            {
                form.AddField(entry.Key, entry.Value);
            }
        
            request.uploadHandler = new UploadHandlerRaw(form.data);
            request.downloadHandler = new DownloadHandlerBuffer();
        
            foreach (var header in form.headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }
    }

// 自定义异常
    public class HttpException : Exception
    {
        public int StatusCode { get; }

        public HttpException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
}