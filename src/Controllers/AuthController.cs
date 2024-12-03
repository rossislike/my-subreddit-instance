using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase 
{
    string USER_AGENT = Environment.GetEnvironmentVariable("USER_AGENT") ?? "";
    private readonly StateManager _stateManager;
    private readonly IHttpClientFactory _httpClientFactory;
    public AuthController(IHttpClientFactory httpClientFactory, StateManager stateManager)
    {
        _stateManager = stateManager;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        string CLIENT_ID = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "";
        string REDIRECT_URI = Environment.GetEnvironmentVariable("REDIRECT_URI") ?? "";

        var state = Guid.NewGuid().ToString();
        _stateManager.SaveState("state", state);
        var queryParams = new Dictionary<string, string>
        {
            { "client_id", CLIENT_ID },
            { "response_type", "code" },
            { "state", state },
            { "redirect_uri", REDIRECT_URI },
            { "duration", "temporary" },
            { "scope", "identity" }
        };

        var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
        var authUrl = $"https://www.reddit.com/api/v1/authorize?{queryString}";

        var response = new { authUrl };
        return Ok(response);
    }


    [HttpGet("reddit_callback")]
    public async Task<IActionResult> RedditCallback(string code, string state)
    {
        string SITE_URL = Environment.GetEnvironmentVariable("SITE_URL") ?? "";  
        if (!_stateManager.IsValidState(state))
        {
            return StatusCode(403, "Invalid state");
        }

        var accessToken = await GetAccessTokenAsync(code);
        _stateManager.SaveState("accessToken", accessToken);
        var username = await GetUsernameAsync(accessToken);

        _stateManager.SaveState("lastId", "");
        return Redirect($"{SITE_URL}?username={username}");
    }

    async Task<string> GetAccessTokenAsync(string code)
    {
        string CLIENT_ID = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "";  
        string CLIENT_SECRET = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "";  
        string REDIRECT_URI = Environment.GetEnvironmentVariable("REDIRECT_URI") ?? "";

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{CLIENT_ID}:{CLIENT_SECRET}"));

        var postData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", REDIRECT_URI }
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token")
        {
            Content = postData
        };
        request.Headers.Add("Authorization", $"Basic {authHeader}");

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(json);
        var accessToken = tokenResponse.GetProperty("access_token").GetString();
        if (accessToken is null) return string.Empty;
        return accessToken;
    }

    async Task<string> GetUsernameAsync(string accessToken)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

        var response = await client.GetAsync("https://oauth.reddit.com/api/v1/me");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var userResponse = JsonSerializer.Deserialize<JsonElement>(json);

        var name = userResponse.GetProperty("name").GetString();
        if (name is null) return string.Empty;
        return name;
    }

}