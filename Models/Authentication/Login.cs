using System.ComponentModel.DataAnnotations;

namespace WebAPIProgram.Models;

public class Login
{
    [Required(ErrorMessage = "Username is missing.")]
    public string username { get; set; }
    [Required(ErrorMessage = "Password is missing.")]
    public string password { get; set; }
}