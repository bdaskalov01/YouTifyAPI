using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.v1.Controllers.User.Requests;

public class ChangeEmailRequest
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
}