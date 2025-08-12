using Microsoft.AspNetCore.Mvc;
using WebAPIProgram.Services;

namespace WebAPIProgram.v1.Controllers.Artist;

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