using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;


namespace CockleBurs.GameFramework.Utility
{
public class HttpClientService : MonoBehaviour
{
    private static readonly HttpClient httpClient = new HttpClient();

    // 发送 POST 请求
    public async Task<T> PostAsync<T>(string url, object data)
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseData);
        }
        else
        {
            Debug.LogError($"HTTP Error: {response.StatusCode}");
            return default;
        }
    }
}

}