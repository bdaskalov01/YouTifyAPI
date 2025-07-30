using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.Models;

public class RefreshTokenRequest
{
    [FromForm(Name = "access_token")]
    [Required(ErrorMessage = "Access token is required")]
    public required string AccessToken {get; set;}
    [FromForm(Name = "refresh_token")]
    [Required(ErrorMessage = "Refresh token is required")]
    public required string RefreshToken {get; set;}
}