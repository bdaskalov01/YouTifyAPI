using Microsoft.AspNetCore.Identity;

namespace WebAPIProgram.Models.Database.Tables;

public class User : IdentityUser
{
    public Boolean isArtist { get; set; }
    
}