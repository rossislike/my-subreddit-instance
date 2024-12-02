using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<StateManager>();
builder.Services.AddSingleton<IStatsRepository, StatsRepository>();
builder.Services.AddControllers();
builder.Services.AddCors(options => 
{
    options.AddPolicy("ReactPolicy", policy => 
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactPolicy");
app.UseHttpsRedirection();
app.MapControllers();

string CLIENT_ID = Environment.GetEnvironmentVariable("CLIENT_ID") ?? "";  
string CLIENT_SECRET = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? "";  
string REDIRECT_URI = Environment.GetEnvironmentVariable("REDIRECT_URI") ?? "";
string USER_AGENT = Environment.GetEnvironmentVariable("USER_AGENT") ?? "";
string WEB_HOST_URL = Environment.GetEnvironmentVariable("WEB_HOST_URL") ?? "";
string ALB_DNS = Environment.GetEnvironmentVariable("ALB_DNS") ?? "";

builder.WebHost.UseUrls(WEB_HOST_URL);
var stateManager = app.Services.GetRequiredService<StateManager>();
Console.WriteLine($"WEB_HOST_URL {WEB_HOST_URL}" );
// app.MapGet("/stats", (IStatsRepository statsRepository) => statsRepository.GetTopPosts());
app.MapGet("/", async context => {
    var state = Guid.NewGuid().ToString();
    stateManager.SaveState("query", state);
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
    // await context.Response.WriteAsync($"<a href=\"{authUrl}\">Authenticate with Reddit</a>");
    await context.Response.WriteAsync(JsonSerializer.Serialize(new { authUrl}));
})
.WithOpenApi();

app.MapGet("/reddit_callback", async (HttpContext context, IHttpClientFactory httpClientFactory) => 
{
    var query = context.Request.Query;
    var state = query["state"].ToString() ?? string.Empty;
    var code = query["code"].ToString() ?? string.Empty;
    var error = query["error"].ToString() ?? string.Empty;

    if (!string.IsNullOrEmpty(error))
    {
        await context.Response.WriteAsync($"Error: {error}");
        return;
    }

    if (!stateManager.IsValidState(state))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsync("Invalid state");
        return;
    }

    try
    {
        // Exchange authorization code for access token
        var accessToken = await GetAccessTokenAsync(httpClientFactory, code);
        stateManager.SaveState("accessToken", accessToken);
        // Retrieve Reddit username
        var username = await GetUsernameAsync(httpClientFactory, accessToken);

        // await context.Response.WriteAsync($"Your Reddit username is: {username}");
        context.Response.Redirect(ALB_DNS);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync($"An error occurred: {ex.Message}");
    }
});

app.Run();

async Task<string> GetAccessTokenAsync(IHttpClientFactory httpClientFactory, string code)
{
    var client = httpClientFactory.CreateClient();
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

async Task<string> GetUsernameAsync(IHttpClientFactory httpClientFactory, string accessToken)
{
    var client = httpClientFactory.CreateClient();
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

public class StateManager
{
    private readonly Dictionary<string, string> _states = new();

    public void SaveState(string state, string value)
    {
        if (_states.ContainsKey(state)) _states[state] = value;
        else _states.Add(state, value);
    }

    public bool IsValidState(string value) 
    {
        return _states.ContainsValue(value);
    }

    public string GetStateValue(string state)
    {
        if (_states.TryGetValue(state, out var value))
        {
            return value;
        }
        return string.Empty;
    }
}