using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram.Services;

public interface IArtistsService
{
    Task<IEnumerable<Artists>> GetByName(string name);
    Task AddAsync(Artists artist);
}