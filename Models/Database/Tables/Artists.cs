using Microsoft.AspNetCore.Identity;

namespace WebAPIProgram.Models.Database.Tables;

public class Artists
{
    public string Id { get; set; }
    public int ProfileViews { get; set; }
    public IdentityUserExtended User { get; set; }
}