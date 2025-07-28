using Microsoft.AspNetCore.Mvc;

namespace WebAPIProgram.Models;

public class ClientCreationRequest
{
    [FromForm(Name = "client_id")]
    public string ClientId { get; set; }
    [FromForm(Name = "client_secret")]
    public string ClientSecret { get; set; }
    [FromForm(Name = "roles")]
    public List<string> Roles { get; set; }
    [FromForm(Name = "allowed_scopes")]
    public List<string> AllowedScopes { get; set; }
}