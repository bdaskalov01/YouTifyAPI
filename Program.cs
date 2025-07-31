using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPIProgram;
using WebAPIProgram.Models;
using WebAPIProgram.Repositories;
using WebAPIProgram.Services;
using WebAPIProgram.Util;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    });

builder.Services.AddOpenApi();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthConstants.apiPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthConstants.scope, AuthConstants.apiScope);
    });
    options.AddPolicy(AuthConstants.artistPolicy, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AuthConstants.role, AuthConstants.artistRole);
            policy.RequireClaim(AuthConstants.role, AuthConstants.adminRole);

        } 
    );
    options.AddPolicy(AuthConstants.adminPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthConstants.role, AuthConstants.adminRole);
    });
    options.AddPolicy(AuthConstants.confirmedEmailPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AuthConstants.emailClaim, AuthConstants.confirmedEmailPolicy);
    });

});
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration[AuthConstants.jwtIssuer],
            ValidAudience = configuration[AuthConstants.jwtAudience],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration[AuthConstants.jwtKey]))
        };
    });

// DI
builder.Services.AddScoped<ISongsRepository, SongsRepository>();
builder.Services.AddScoped<ISongsService, SongsService>();
builder.Services.AddScoped<IArtistsRepository, ArtistsRepository>();
builder.Services.AddScoped<IArtistsService, ArtistService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Background services
builder.Services.AddHostedService<ExpiredTokenCleanupService>();

// CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    string[] roleNames = { AuthConstants.adminRole, AuthConstants.userRole, AuthConstants.artistRole };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAsync(services);
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();