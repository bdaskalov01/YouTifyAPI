using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPIProgram.Models;
using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<Songs> Songs { get; set; }
    public DbSet<Artists> Artists { get; set; }
    public DbSet<UserLikedSongs> UserLikedSongs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OAuthClient> OAuthClients { get; set; }
    
     protected override void OnModelCreating(ModelBuilder modelBuilder)
     { 
         base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<UserLikedSongs>()
            .HasKey(key => new { key.UserId, key.SongId });
        modelBuilder.Entity<Artists>()
            .HasKey(key => new { key.Id });
        modelBuilder.Entity<Songs>()
            .HasKey(key => new { key.Id });
        modelBuilder.Entity<RefreshToken>()
            .HasKey(key => new { key.Token });
        modelBuilder.Entity<OAuthClient>()
            .HasKey(key => new { key.Id });
     }
}