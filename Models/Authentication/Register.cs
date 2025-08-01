using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.Models;

public class Register
{
    [FromForm(Name = "username")]
    [Required(ErrorMessage = "Username is missing.")]
    public string Username { get; set; }
    [FromForm(Name = "password")]
    [Required(ErrorMessage = "Password is missing.")]
    public string Password { get; set; }
    [FromForm(Name = "email")]
    [Required(ErrorMessage = "Email is missing.")]
    public string Email { get; set; }
}