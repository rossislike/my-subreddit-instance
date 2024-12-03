public interface IStatsRepository
{
    void AddPost(Post post); 
    void ClearPosts();
    List<Post> GetTopPosts();
    List<User> GetTopUsers();
    int GetTotalPosts();
}
