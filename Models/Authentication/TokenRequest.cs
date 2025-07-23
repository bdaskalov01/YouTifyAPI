using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.Models;

public class TokenRequest
{
    [FromForm(Name = "grant_type")]
    public string GrantType { get; set; }

    [FromForm(Name = "username")]
    public string? Username { get; set; }

    [FromForm(Name = "password")]
    public string? Password { get; set; }

    [FromForm(Name = "client_id")]
    public string? ClientId { get; set; }

    [FromForm(Name = "client_secret")]
    public string? ClientSecret { get; set; }

    [FromForm(Name = "scope")]
    public string? Scope { get; set; }
}