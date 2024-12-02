using System.ComponentModel;
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
        var lastSubreddit = _stateManager.GetStateValue("subreddit");
        if (lastSubreddit != subreddit)
        {
            _stateManager.SaveState("subreddit", subreddit);
            _stateManager.SaveState("lastId", "");
            _repository.ClearPosts();
        }
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
        var accessToken = _stateManager.GetStateValue("accessToken");
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        var lastId = _stateManager.GetStateValue("lastId");
        var after = !string.IsNullOrEmpty(lastId) ? $"&after={lastId}" : "";
        var response = await client.GetAsync($"https://www.reddit.com/r/{subreddit}/new.json?limit=100{after}");
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
        // _repository.AddPosts(posts);
        posts.ForEach(_repository.AddPost);
        return Ok(new StatsResponse
        {
            TopPosts = _repository.GetTopPosts(),
            TopUsers = _repository.GetTopUsers(),
            TotalPosts = _repository.GetTotalPosts()
        });
    }
}

public interface IStatsRepository
{
    void AddPost(Post post); 
    void ClearPosts();
    List<Post> GetTopPosts();
    List<User> GetTopUsers();
    int GetTotalPosts();
}

public class StatsRepository : IStatsRepository
{
    private readonly List<Post> _posts;
    private readonly List<User> _users;

    public StatsRepository()
    {
        _posts = new List<Post>();
        _users = new List<User>();
    }

    public void AddPost(Post post) 
    {
        if (_posts.Any(p => p.Id == post.Id)) 
            return;
        _posts.Add(post);
    }

    public void ClearPosts() 
    {
        _posts.Clear();
    }
    public List<Post> GetTopPosts()
    {
        return _posts
            .OrderByDescending(p => p.Likes)
            .Take(5)
            .Select(p => new Post { Id = p.Id, Title = p.Title, Likes = p.Likes, Author = p.Author })
            .ToList();
    }

    public List<User> GetTopUsers()
    {
        return _posts
            .GroupBy(p => p.Author)
            .Select(g => new User { Name = g.Key, Posts = g.Count() })
            .OrderByDescending(u => u.Posts)
            .Take(5)
            .ToList();
    }

    public int GetTotalPosts()
    {
        return _posts.Count();
    }
}

public class StatsResponse
{
    public List<Post> TopPosts { get; set; }
    public List<User> TopUsers { get; set; }
    public int TotalPosts { get; set; }
}

public class Post
{
    public string Id { get; set; }
    public string Title { get; set; }
    public int Likes { get; set; }

    public string Author { get; set; }
}

public class User
{
    public string Name { get; set; }
    public int Posts { get; set; }
}


public class RedditResponse 
{
    public string kind { get; set; }
    public RedditData data { get; set; }
}

public class RedditData 
{   
    public string after { get; set; }
    public int dist { get; set; }
    public List<RedditPost> children { get; set; }
}

public class RedditPost
{
    public RedditPostData data { get; set; }
}

public class RedditPostData
{
    public string id { get; set; }  
    public string title { get; set; }
    public int ups { get; set; }
    public string author { get; set; }
}