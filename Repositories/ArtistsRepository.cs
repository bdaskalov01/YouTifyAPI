using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram.Repositories;

public class ArtistsRepository: IArtistsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ArtistsRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    async Task IArtistsRepository.AddAsync(Artists artist)
    {
        await _dbContext.Artists.AddAsync(artist);
        await _dbContext.SaveChangesAsync();
    }
    async Task<IEnumerable<Artists>> IArtistsRepository.GetByNameAsync(string name)
    {
        return await _dbContext.Artists.Where( artist => artist.Name.StartsWith(name)).ToListAsync();
    }
}