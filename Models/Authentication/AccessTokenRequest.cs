using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.Models;

public class AccessTokenRequest
{
    [FromForm(Name = "grant_type")]
    [Required(ErrorMessage = "Grant type is missing.")]
    public string GrantType { get; set; }

    [FromForm(Name = "username")]
    [Required(ErrorMessage = "Username is missing.")]
    public string? Username { get; set; }

    [FromForm(Name = "password")]
    [Required(ErrorMessage = "Password is missing.")]
    public string? Password { get; set; }

    [FromForm(Name = "client_id")]
    [Required(ErrorMessage = "ClientID is missing.")]
    public string? ClientId { get; set; }

    [FromForm(Name = "client_secret")]
    [Required(ErrorMessage = "ClientSecret is missing.")]
    public string? ClientSecret { get; set; }

    [FromForm(Name = "scope")]
    [Required(ErrorMessage = "Scope is missing.")]
    public string? Scope { get; set; }
}