using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebAPIProgram.Models.Database.Tables;

public class UserLikedSongs
{
    public string UserId { get; set; }
    public string SongId { get; set; }
    public DateOnly AddedAt { get; set; }
    public IdentityUserExtended IdentityUserExtended { get; set; }
    public Songs Song { get; set; }
}