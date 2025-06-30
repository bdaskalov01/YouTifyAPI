using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPIProgram.Models.Database.Tables;
using WebAPIProgram.Repositories;

namespace WebAPIProgram.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SongsController : ControllerBase
{
    
    private readonly ISongsRepository _songsRepository;

    public SongsController(ISongsRepository songsRepository)
    {
        _songsRepository = songsRepository;
    }
    
    [HttpGet("Get by id")]
    public async Task<IActionResult> Get(string id)
    {
        var title = await _songsRepository.GetByIdAsync(int.Parse(id));
        if (title == null)
        {
            return BadRequest();
        }
        return Ok(title);
    }
    
    [HttpGet("Get all")]
    public async Task<IActionResult> Get()
    {
        var title = await _songsRepository.GetAllAsync();
        if (title == null)
        {
            return BadRequest();
        }
        return Ok(title);
    }

    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] Songs songs)
    {
        try
        {
            _songsRepository.AddAsync(songs);
        }
        catch (Exception ex)
        {
            return BadRequest();
        }
        return Ok();
    }
}