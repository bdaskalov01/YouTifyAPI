using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.Models;

public class Login
{
    [FromForm(Name = "username")]
    [Required(ErrorMessage = "Username is missing.")]
    public string username { get; set; }
    [FromForm(Name = "password")]
    [Required(ErrorMessage = "Password is missing.")]
    public string password { get; set; }
}