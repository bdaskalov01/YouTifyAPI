using WebAPIProgram.Models.DTO;

namespace WebAPIProgram.v1.Controllers.Song;

public interface ISongService
{
    Task<IEnumerable<SongsResponse>> GetAllSongsAsync();
    Task<SongsResponse> GetSongByIdAsync(int id);
    Task AddSongAsync(SongsResponse songDto);
    Task UpdateSongLikes(int id, SongsResponse songDto);
    Task DeleteSongAsync(int id);
}