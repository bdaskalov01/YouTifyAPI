using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram.Repositories;

public interface IArtistsRepository
{
    Task AddAsync(Artists artist);
    Task<IEnumerable<Artists>> GetByNameAsync(string name);
    
}