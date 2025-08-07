using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram.Repositories;

public class SongRepository : ISongRepository
{
    private readonly ApplicationDbContext _context;
    
    public SongRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Songs>> GetAllAsync()
    {
        return await _context.Songs.ToListAsync();
    }

    public async Task<Songs> GetByNameAsync(string title)
    {
        return await _context.Songs.FirstOrDefaultAsync(song =>
            song.Title == title);
    }

    public async Task<Songs> GetByIdAsync(int id)
    {
        return await _context.Songs.FindAsync(id);
    }

    public async Task AddAsync(Songs song)
    {
        await _context.Songs.AddAsync(song);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Songs song)
    {
        _context.Songs.Update(song);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Songs.FindAsync(id);

        if (product != null)
        {
            _context.Songs.Remove(product);

            await _context.SaveChangesAsync();
        }
    }
}