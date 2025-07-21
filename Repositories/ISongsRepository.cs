using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram.Repositories;

public interface ISongsRepository
{
    Task<IEnumerable<Songs>> GetAllAsync();
    Task<Songs> GetByNameAsync(string title);
    Task<Songs> GetByIdAsync(int id);
    Task AddAsync(Songs song);
    Task UpdateAsync(Songs song);
    Task DeleteAsync(int id);
}