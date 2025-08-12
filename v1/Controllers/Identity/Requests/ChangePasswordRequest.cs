using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.v1.Controllers.User.Requests;

public class ChangePasswordRequest
{
    [FromForm(Name = "current_password")]
    [Required]
    public string CurrentPassword { get; set; }
    [FromForm(Name = "new_password")]
    [Required]
    public string NewPassword { get; set; }
    
}