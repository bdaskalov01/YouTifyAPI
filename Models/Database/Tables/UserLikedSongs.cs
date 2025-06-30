namespace WebAPIProgram.Models.Database.Tables;

public class UserLikedSongs
{
    public int UserId { get; set; }
    public int SongId { get; set; }
    public DateOnly AddedAt { get; set; }  // SQL: DATE
}