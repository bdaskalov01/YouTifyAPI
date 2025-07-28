namespace WebAPIProgram.Models;

public class OAuthClient
{
    public string Id { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<string> AllowedScopes { get; set; } = new();
}