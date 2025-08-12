
using PSQLModels.Tables;

namespace WebAPIProgram.v1.Controllers.Song;

public interface ISongRepository
{
    Task<IEnumerable<Songs>> GetAllAsync();
    Task<Songs> GetByNameAsync(string title);
    Task<Songs> GetByIdAsync(int id);
    Task AddAsync(Songs song);
    Task UpdateAsync(Songs song);
    Task DeleteAsync(int id);
}