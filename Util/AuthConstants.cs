namespace WebAPIProgram.Util;

public class AuthConstants
{
    // Responses
    public const string successfullRegistration = "User has been registered successfully";
    
    // Invalid responses
    public const string invalidUser = "User not found";
    public const string invalidClient = "Invalid client credentials";
    public const string invalidPassword = "Invalid password";
    public const string invalidGrantType = "Invalid grant type";
    public const string invalidScope = "Invalid scope";
    public const string invalidRefreshToken = "Invalid refresh token";
    
    // Grant types
    public const string grantType = "grant_type";
    public const string clientCredentialsGrant = "client_credentials";
    public const string resourceOwnerGrant = "password";
    
    // Token
    public const string bearerTokenType = "bearer";
    public const int oneHour = 1;
    public const int oneHourInMinutes = 60;
    public const int oneHourInSeconds = 3600;
    public const int sevenDays = 7;
    public const int thirtyMinutes = 30;
    
    // Scopes
    public const string scope = "scope";
    public const string apiScope = "api";
    public const string readScope = "read";
    public const string writeScope = "write";
    
    // JWT
    public const string jwtKey = "Jwt:Key";
    public const string jwtIssuer = "Jwt:Issuer";
    public const string jwtAudience = "Jwt:Audience";
    
    // Roles
    public const string role = "role";
    public const string userRole = "User";
    public const string adminRole = "Admin";
    public const string artistRole = "Artist";
    
    //Policies
    public const string apiPolicy = "api_policy";
    public const string artistPolicy = "artist_policy";
    public const string adminPolicy = "admin_policy";
    public const string confirmedEmailPolicy = "confirmed_email_policy";
    
    // Claims
    public const string idClaim = "id";
    public const string emailClaim = "email";
    public const string usernameClaim = "username";
    public const string nameClaim = "name";
    public const string emailVerifiedClaim = "email_verified";
    public const string clientIDClaim = "client_id";

}