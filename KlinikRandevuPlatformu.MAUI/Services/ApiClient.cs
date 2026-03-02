using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace KlinikRandevuPlatformu.MAUI.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly TokenStore _token;
    private static readonly JsonSerializerOptions _opts = new(JsonSerializerDefaults.Web);

    public ApiClient(HttpClient http, TokenStore token)
    {
        _http = http;
        _token = token;
    }

    private void AttachToken()
    {
        _http.DefaultRequestHeaders.Remove("X-Auth-Token");
        var t = _token.GetToken();
        if (!string.IsNullOrWhiteSpace(t))
            _http.DefaultRequestHeaders.Add("X-Auth-Token", t);
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        AttachToken();
        var res = await _http.GetAsync(url);
        var txt = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception(txt);
        return JsonSerializer.Deserialize<T>(txt, _opts);
    }

    public async Task<T?> PostAsync<T>(string url, object body)
    {
        AttachToken();
        var json = JsonSerializer.Serialize(body, _opts);
        var res = await _http.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        var txt = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode) throw new Exception(txt);
        return JsonSerializer.Deserialize<T>(txt, _opts);
    }
}
