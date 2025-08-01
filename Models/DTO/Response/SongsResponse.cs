namespace WebAPIProgram.Models.DTO;

public class SongsResponse
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string ArtistId { get; set; }
    public DateTime ReleaseDate { get; set; }
    public int Likes { get; set; }
}