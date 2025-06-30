using Microsoft.AspNetCore.Mvc;
using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Repositories;
using WebAPIProgram.Services;

namespace WebAPIProgram.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ArtistController : ControllerBase
{
    private readonly IArtistsService artistsService;

    public ArtistController(IArtistsService service)
    {
        artistsService = service;
    }
    
    [HttpGet("Get artist by name")]
    public async Task<IActionResult> GetByArtistId(string name)
    {
        var list = await artistsService.GetByName(name);
        if (list == null)
        {
            return NotFound();
        }
        Console.WriteLine(string.Join(",", list));
        return Ok(list);
    }

    [HttpPost("Add artist")]
    public async Task<IActionResult> AddAsync([FromBody] Artists artist)
    {
        await artistsService.AddAsync(artist);
        return Ok();
    }
}