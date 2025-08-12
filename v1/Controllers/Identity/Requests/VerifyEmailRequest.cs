namespace WebAPIProgram.v1.Controllers.User.Requests;

public class VerifyEmailRequest
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
}