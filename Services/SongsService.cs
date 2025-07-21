using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Models.DTO;
using WebAPIProgram.Repositories;

namespace WebAPIProgram.Services;

public class SongsService: ISongsService
{
    private readonly ISongsRepository _songsRepository;

        public SongsService(ISongsRepository songsRepository)
        {
            _songsRepository = songsRepository;
        }

        public async Task<IEnumerable<SongsResponse>> GetAllSongsAsync()
        {
            var products = await _songsRepository.GetAllAsync();
            
            return products.Select(song => new SongsResponse 
            { 
                id = song.Id, 
                title = song.Title, 
                artist_id = song.Artist_Id,
                release_date = song.Release_Date,
                likes = song.Likes,
            });
        }

        public async Task<SongsResponse> GetSongByIdAsync(int id)
        {
            var song = await _songsRepository.GetByIdAsync(id);

            if (song == null)
                throw new KeyNotFoundException("Product not found");

            return new SongsResponse 
            { 
                id = song.Id, 
                title = song.Title, 
                artist_id = song.Artist_Id,
                release_date = song.Release_Date,
                likes = song.Likes,
            };
        }
        
        

        public async Task AddSongAsync(SongsResponse songDTO)
        {
            var song = new Songs
            {
                Title = songDTO.title,
                Artist_Id = songDTO.artist_id,
                Release_Date = DateTime.UtcNow.Date,
                Likes = 0
            };

            Console.WriteLine(DateTime.UtcNow.Date);
            await _songsRepository.AddAsync(song);
        }

        public async Task UpdateSongLikes(int id, SongsResponse songDTO)
        {
            var song = await _songsRepository.GetByIdAsync(id);

            if (song == null)
                throw new KeyNotFoundException("Song not found");

            song.Likes = songDTO.likes;

            await _songsRepository.UpdateAsync(song);
        }

        public async Task DeleteSongAsync(int id)
        {
            var song = await _songsRepository.GetByIdAsync(id);
            
            if (song == null)
                throw new KeyNotFoundException("Song not found");

            await _songsRepository.DeleteAsync(id);
        }
    }