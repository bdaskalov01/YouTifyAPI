using WebAPIProgram.Models.DTO;

namespace WebAPIProgram.Services;

public interface ISongsService
{
    Task<IEnumerable<SongsResponse>> GetAllSongsAsync();
    Task<SongsResponse> GetSongByIdAsync(int id);
    Task AddSongAsync(SongsResponse songDto);
    Task UpdateSongLikes(int id, SongsResponse songDto);
    Task DeleteSongAsync(int id);
}