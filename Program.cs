using System.Text;
using System.Text.Json.Serialization;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PSQLModels.Tables;
using WebAPIProgram;
using WebAPIProgram.Filters.Action;
using WebAPIProgram.Handlers;
using StackExchange.Redis;
using WebAPIProgram.Requirements;
using WebAPIProgram.Services;
using WebAPIProgram.Util;
using WebAPIProgram.v1.Controllers.Artist;
using WebAPIProgram.v1.Controllers.Auth;
using WebAPIProgram.v1.Controllers.Song;
using WebAPIProgram.v1.Controllers.User;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<IdentityUserExtended, IdentityRole>()
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
    options.AddPolicy(AppConstants.apiPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AppConstants.scope, AppConstants.apiScope);
    });
    options.AddPolicy(AppConstants.artistPolicy, policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireClaim(AppConstants.role, AppConstants.artistRole);
            policy.RequireClaim(AppConstants.role, AppConstants.adminRole);

        } 
    );
    options.AddPolicy(AppConstants.adminPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim(AppConstants.role, AppConstants.adminRole);
    });
    options.AddPolicy(AppConstants.confirmedEmailPolicy, policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new EmailConfirmedRequirement());
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
            ValidIssuer = configuration[AppConstants.jwtIssuer],
            ValidAudience = configuration[AppConstants.jwtAudience],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration[AppConstants.jwtKey]))
        };
    });

// Mass Transit
builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();
    busConfig.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(builder.Configuration["RabbitMQ:Host"]!), h =>
        {
            h.Username(builder.Configuration["RabbitMQ:User"]!);
            h.Password(builder.Configuration["RabbitMQ:Password"]!);
        });
        configurator.ConfigureEndpoints(context);
    });
    

});

// DI
builder.Services.AddScoped<ISongRepository, SongRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();

builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddScoped<IAuthorizationHandler, EmailConfirmedHandler>(); 

builder.Services.AddScoped<ArtistClickedFilter>();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = builder.Configuration.GetConnectionString("Redis") ?? builder.Configuration["Redis:ConnectionString"];
    return ConnectionMultiplexer.Connect(config);
});

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
    
    string[] roleNames = { AppConstants.adminRole, AppConstants.userRole, AppConstants.artistRole };

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