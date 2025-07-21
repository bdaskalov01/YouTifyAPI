using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Models.DTO;
using WebAPIProgram.Repositories;
using WebAPIProgram.Services;

namespace WebAPIProgram.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    
    private readonly ISongsService _songsService;
    

    public SongsController(ISongsService songsService)
    {
        _songsService = songsService;
    }
    
    [HttpGet("Get by id")]
    public async Task<IActionResult> Get(string id)
    {
        var title = await _songsService.GetSongByIdAsync(int.Parse(id));
        if (title == null)
        {
            return BadRequest();
        }
        return Ok(title);
    }
    
    [HttpGet("Get all")]
    public async Task<IActionResult> Get()
    {
        var title = await _songsService.GetAllSongsAsync();
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
            await _songsService.AddSongAsync(song);
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
        return Ok();
    }
}