using System.ComponentModel.DataAnnotations;

namespace WebAPIProgram.Models;

public class Register
{
    [Required(ErrorMessage = "Username is missing.")]
    public string username { get; set; }
    [Required(ErrorMessage = "Password is missing.")]
    public string password { get; set; }
    [Required(ErrorMessage = "Email is missing.")]
    public string email { get; set; }
    
}