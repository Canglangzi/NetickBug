using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


namespace CockleBurs.GameFramework.Utility
{
public class TokenService : MonoBehaviour
{
    private HttpClientService httpClientService;

    private void Awake()
    {
        httpClientService = gameObject.AddComponent<HttpClientService>();
    }

    // 生成新令牌
    public async Task<string> GenerateToken(string serviceName)
    {
        var data = new Dictionary<string, string>
        {
            { "service", serviceName }
        };

        var tokenResponse = await httpClientService.PostAsync<TokenResponse>("https://yourapi.example.com/get-token", data);
        return tokenResponse?.Token; // 返回令牌
    }

    // 验证令牌
    public async Task<bool> ValidateToken(long steamId, string token)
    {
        var data = new Dictionary<string, object>
        {
            { "steamid", steamId },
            { "token", token }
        };

        var response = await httpClientService.PostAsync<ValidateAuthTokenResponse>("https://services.facepunch.com/sbox/auth/token", data);
        return response != null && response.Status == "ok" && response.SteamId == steamId;
    }

    // 令牌响应类
    private class TokenResponse
    {
        public string Token { get; set; }
    }

    // 验证令牌响应类
    private class ValidateAuthTokenResponse
    {
        public long SteamId { get; set; }
        public string Status { get; set; }
    }
}

}