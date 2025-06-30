using WebAPIProgram.Models.DTO;

namespace WebAPIProgram.Services;

public interface ISongsService
{
    Task<IEnumerable<SongsResponse>> GetAllSongsAsync();
    Task<SongsResponse> GetSongByIdAsync(int id);
    Task AddSongAsync(SongsResponse productDto);
    Task UpdateSongLikes(int id, SongsResponse productDto);
    Task DeleteSongAsync(int id);
}