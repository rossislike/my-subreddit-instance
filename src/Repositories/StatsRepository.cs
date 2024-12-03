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
