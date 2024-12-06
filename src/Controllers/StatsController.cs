using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    string USER_AGENT = Environment.GetEnvironmentVariable("USER_AGENT") ?? "";
    private readonly IStatsRepository _repository;
    private readonly StateManager _stateManager;
    private readonly IHttpClientFactory _httpClientFactory;

    public StatsController(IStatsRepository repository, IHttpClientFactory httpClientFactory, StateManager stateManager)
    {
        _repository = repository;
        _stateManager = stateManager;
        _httpClientFactory = httpClientFactory;
    }
    [HttpGet]
    public async Task<ActionResult<StatsResponse>> GetStats([FromQuery] string subreddit)
    {
        if (string.IsNullOrEmpty(subreddit)) return new StatsResponse();
        var lastSubreddit = _stateManager.GetStateValue("subreddit");
        if (lastSubreddit != subreddit)
        {
            _stateManager.SaveState("subreddit", subreddit);
            _stateManager.SaveState("lastId", "");
            _repository.ClearPosts();
        }
        var client = _httpClientFactory.CreateClient();

        var lastId = _stateManager.GetStateValue("lastId");
        var after = !string.IsNullOrEmpty(lastId) ? $"&after={lastId}" : string.Empty;

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://oauth.reddit.com/r/{subreddit}/new.json?limit=100{after}");
        request.Headers.Add("User-Agent", USER_AGENT);
        
        var accessToken = _stateManager.GetStateValue("accessToken");
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        
        foreach (var header in request.Headers)
        {
            Console.WriteLine($"{header.Key}: {header.Value}");
        }

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        RedditResponse redditResponse = JsonSerializer.Deserialize<RedditResponse>(json);
        _stateManager.SaveState("lastId", redditResponse.data.after);
        var posts = redditResponse.data.children.Select(c => new Post
        {
            Id = c.data.id,
            Title = c.data.title,
            Likes = c.data.ups,
            Author = c.data.author

        }).ToList();
        posts.ForEach(_repository.AddPost);
        return Ok(new StatsResponse
        {
            TopPosts = _repository.GetTopPosts(),
            TopUsers = _repository.GetTopUsers(),
            TotalPosts = _repository.GetTotalPosts()
        });
    }

    // [HttpGet]
    // public async Task<ActionResult<StatsResponse>> GetStats([FromQuery] string subreddit)
    // {
    //     if (string.IsNullOrEmpty(subreddit)) return new StatsResponse();
    //     var lastSubreddit = _stateManager.GetStateValue("subreddit");
    //     if (lastSubreddit != subreddit)
    //     {
    //         _stateManager.SaveState("subreddit", subreddit);
    //         _stateManager.SaveState("lastId", "");
    //         _repository.ClearPosts();
    //     }
    //     var client = _httpClientFactory.CreateClient();
    //     client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
    //     var accessToken = _stateManager.GetStateValue("accessToken");
    //     client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
    //     Console.WriteLine($"accessToken: {accessToken}");
    //     var lastId = _stateManager.GetStateValue("lastId");
    //     var after = !string.IsNullOrEmpty(lastId) ? $"&after={lastId}" : "";
    //     var response = await client.GetAsync($"https://www.reddit.com/r/{subreddit}/new.json?limit=100{after}");
    //     response.EnsureSuccessStatusCode();

    //     var json = await response.Content.ReadAsStringAsync();
    //     RedditResponse redditResponse = JsonSerializer.Deserialize<RedditResponse>(json);
    //     _stateManager.SaveState("lastId", redditResponse.data.after);
    //     var posts = redditResponse.data.children.Select(c => new Post
    //     {
    //         Id = c.data.id,
    //         Title = c.data.title,
    //         Likes = c.data.ups,
    //         Author = c.data.author

    //     }).ToList();
    //     posts.ForEach(_repository.AddPost);
    //     return Ok(new StatsResponse
    //     {
    //         TopPosts = _repository.GetTopPosts(),
    //         TopUsers = _repository.GetTopUsers(),
    //         TotalPosts = _repository.GetTotalPosts()
    //     });
    // }
}
