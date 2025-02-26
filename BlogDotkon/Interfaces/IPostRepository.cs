using BlogDotkon.Models;

namespace BlogDotkon.Interfaces
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<List<Post>> GetAllAsync();
        Task<Post?> GetByIdAsync(int id);
        Task UpdateAsync(Post post);
        Task DeleteAsync(int id);
    }

}
