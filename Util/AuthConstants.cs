namespace WebAPIProgram.Util;

public class AuthConstants
{
    // Invalid responses
    public const string invalidUser = "User not found";
    public const string invalidClient = "Invalid client credentials";
    public const string invalidPassword = "Invalid password";
    public const string invalidGrantType = "Invalid grant type";
    public const string invalidScope = "Invalid scope";
    
    // Grant types
    public const string clientCredentialsGrant = "client_credentials";
    public const string resourceOwnerGrant = "password";
    
    // Token type
    public const string bearerToken = "bearer";
    
    // Token expiration
    public const int oneHour = 1;
    public const int oneHourInMinutes = 60;
    public const int oneHourInSeconds = 3600;
    
    // Scopes
    public const string scope = "scope";
    public const string apiScope = "api";
    public const string readScope = "read";
    public const string writeScope = "write";
    
    // JWT
    public const string jwtKey = "Jwt:Key";
    public const string jwtIssuer = "Jwt:Issuer";
    public const string jwtAudience = "Jwt:Audience";
    
    // Role
    public const string role = "roke";
}