using Microsoft.AspNetCore.Identity;

namespace WebAPIProgram.Models.Database.Tables;

public class IdentityUserExtended : IdentityUser
{
    public Boolean isArtist { get; set; }
    
}