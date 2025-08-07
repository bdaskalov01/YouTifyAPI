using Microsoft.AspNetCore.Mvc;
using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Repositories;
using WebAPIProgram.Services;

namespace WebAPIProgram.Controllers;

[Route("v1/api/[controller]")]
[ApiController]
public class ArtistController : ControllerBase
{
    private readonly IArtistService _artistService;

    public ArtistController(IArtistService service)
    {
        _artistService = service;
    }
    
    [HttpGet("Get artist by name")]
    public async Task<IActionResult> GetByArtistId(string name)
    {
        var list = await _artistService.GetByName(name);
        if (list == null)
        {
            return NotFound();
        }
        Console.WriteLine(string.Join(",", list));
        return Ok(list);
    }
}