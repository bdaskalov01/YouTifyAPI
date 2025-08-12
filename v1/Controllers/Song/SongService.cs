using PSQLModels.Tables;
using WebAPIProgram.Models.DTO;

namespace WebAPIProgram.v1.Controllers.Song;

public class SongService: ISongService
{
    private readonly ISongRepository _songRepository;

        public SongService(ISongRepository songRepository)
        {
            _songRepository = songRepository;
        }

        public async Task<IEnumerable<SongsResponse>> GetAllSongsAsync()
        {
            var products = await _songRepository.GetAllAsync();
            
            return products.Select(song => new SongsResponse 
            { 
                Id = song.Id, 
                Title = song.Title, 
                ArtistId = song.Artist_Id,
                ReleaseDate = song.Release_Date,
                Likes = song.Likes,
            });
        }

        public async Task<SongsResponse> GetSongByIdAsync(int id)
        {
            var song = await _songRepository.GetByIdAsync(id);

            if (song == null)
                throw new KeyNotFoundException("Product not found");

            return new SongsResponse 
            { 
                Id = song.Id, 
                Title = song.Title, 
                ArtistId = song.Artist_Id,
                ReleaseDate = song.Release_Date,
                Likes = song.Likes,
            };
        }
        
        

        public async Task AddSongAsync(SongsResponse songDTO)
        {
            var song = new Songs
            {
                Title = songDTO.Title,
                Artist_Id = songDTO.ArtistId,
                Release_Date = DateTime.UtcNow.Date,
                Likes = 0
            };

            Console.WriteLine(DateTime.UtcNow.Date);
            await _songRepository.AddAsync(song);
        }

        public async Task UpdateSongLikes(int id, SongsResponse songDTO)
        {
            var song = await _songRepository.GetByIdAsync(id);

            if (song == null)
                throw new KeyNotFoundException("Song not found");

            song.Likes = songDTO.Likes;

            await _songRepository.UpdateAsync(song);
        }

        public async Task DeleteSongAsync(int id)
        {
            var song = await _songRepository.GetByIdAsync(id);
            
            if (song == null)
                throw new KeyNotFoundException("Song not found");

            await _songRepository.DeleteAsync(id);
        }
    }