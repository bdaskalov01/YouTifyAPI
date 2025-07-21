namespace WebAPIProgram.Models.DTO;

public class SongsResponse
{
    public int id { get; set; }
    public string title { get; set; }
    public int artist_id { get; set; }
    public DateTime release_date { get; set; }
    public int likes { get; set; }
}