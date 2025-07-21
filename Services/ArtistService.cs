using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Repositories;

namespace WebAPIProgram.Services;

public class ArtistService: IArtistsService
{
    private readonly IArtistsRepository artistRepository;

    public ArtistService(IArtistsRepository repository)
    {
        artistRepository = repository;
    }

    public async Task<IEnumerable<Artists>> GetByName(string name)
    {
        return await artistRepository.GetByNameAsync(name);
    }

   public async Task AddAsync(Artists artist)
    {
        await artistRepository.AddAsync(artist);
    }
}