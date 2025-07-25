namespace WebAPIProgram.Models.Database.Tables;

public class RefreshToken
{
    public string Token { get; set; }
    public string UserId { get; set; }
    public DateTime ExpiryTime { get; set; }
}