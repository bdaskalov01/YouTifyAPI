namespace WebAPIProgram.Models;

public class UpdateUserRolesRequest
{
    public string UserId { get; set; }
    public string Scopes { get; set; }
}