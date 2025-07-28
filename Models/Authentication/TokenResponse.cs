namespace WebAPIProgram.Models;

public class TokenResponse
{
    public required string TokenType {get; set;}
    public required string AccessToken {get; set;}
    public required int ExpiresIn {get; set;}
    public string? RefreshToken{get; set;}
}