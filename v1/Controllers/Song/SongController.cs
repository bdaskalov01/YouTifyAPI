using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIProgram.Models.DTO;
using WebAPIProgram.Services;

namespace WebAPIProgram.v1.Controllers.Song;

[ApiController]
[Route("v1/api/[controller]")]
public class SongController : ControllerBase
{
    
    private readonly ISongService _songService;
    

    public SongController(ISongService songService)
    {
        _songService = songService;
    }
    
    [HttpGet("Get by id")]
    public async Task<IActionResult> Get(string id)
    {
        var title = await _songService.GetSongByIdAsync(int.Parse(id));
        if (title == null)
        {
            return BadRequest();
        }
        return Ok(title);
    }
    
    [HttpGet("Get all")]
    [Authorize(Policy = "Api")]
    public async Task<IActionResult> Get()
    {
        var title = await _songService.GetAllSongsAsync();
        if (title == null)
        {
            return BadRequest();
        }
        return Ok(title);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] SongsResponse song)
    {
        try
        {
            await _songService.AddSongAsync(song);
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
        return Ok();
    }
}