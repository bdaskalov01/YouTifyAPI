using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebAPIProgram.Models;
using WebAPIProgram.Models.Database.Tables;

namespace WebAPIProgram;

public class ApplicationDbContext : IdentityDbContext<IdentityUserExtended>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    public DbSet<Songs> Songs { get; set; } 
    public DbSet<UserLikedSongs> UserLikedSongs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OAuthClient> OAuthClients { get; set; }
    public DbSet<Followers> Followers { get; set; }
    public DbSet<Artists> Artists { get; set; }
    
     protected override void OnModelCreating(ModelBuilder modelBuilder)
     { 
         base.OnModelCreating(modelBuilder);
         
        // Songs Table
        // Primary key
        modelBuilder.Entity<Songs>()
            .HasKey(key => new { key.Id });
        // Foreign keys

        
        // RefreshToken Table
        // Primary key
        modelBuilder.Entity<RefreshToken>()
            .HasKey(key => new { key.Token });
        
        // OAuthClient Table
        // Primary key
        modelBuilder.Entity<OAuthClient>()
            .HasKey(key => new { key.Id });
        
        // Followers Table
        // Primary key
        modelBuilder.Entity<Followers>()
            .HasKey(key => new { key.UserId, key.ArtistId });
        
        // UserLikedSongs Table
        // Primary key
        modelBuilder.Entity<UserLikedSongs>()
            .HasKey(key => new { key.UserId, key.SongId });
        //Foreign Keys
        modelBuilder.Entity<UserLikedSongs>()
            .HasOne(ul => ul.IdentityUserExtended)
            .WithMany()
            .HasForeignKey(ul => ul.UserId);
        modelBuilder.Entity<UserLikedSongs>()
            .HasOne(ul => ul.Song)
            .WithMany()
            .HasForeignKey(ul => ul.SongId);
        
        // Artists Table
        // Primary key
        modelBuilder.Entity<Artists>()
            .HasKey(key => new { key.Id });
        // Foreign Key
        modelBuilder.Entity<Artists>()
            .HasOne(user => user.User)
            .WithMany()
            .HasForeignKey(user => user.Id);
     }
}