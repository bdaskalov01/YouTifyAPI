namespace WebAPIProgram.Models.Database.Tables;

public class RefreshToken
{
    public string Token { get; set; }
    public string ClientId { get; set; }
    public string? UserId { get; set; }
    public string Scope { get; set; }
    public string GrantType { get; set; }
    public DateTime ExpiryTime { get; set; }
}