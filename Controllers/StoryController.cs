using Microsoft.AspNetCore.Mvc;
using StoryDecomposer.Models;
using StoryDecomposer.Services;

namespace StoryDecomposer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryController : ControllerBase
{
    private readonly StoryDecompositionService _decompositionService;
    
    public StoryController(
        StoryDecompositionService decompositionService)
    {
        _decompositionService = decompositionService;
    }
    
    [HttpPost("decompose")]
    public async Task<IActionResult> DecomposeStory([FromBody] Story story)
    {
        var decomposition = await _decompositionService.DecomposeStory(story);
        return Ok(decomposition);
    }
}
