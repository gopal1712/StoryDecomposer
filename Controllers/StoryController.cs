using Microsoft.AspNetCore.Mvc;
using StoryDecomposer.Models;
using StoryDecomposer.Services;

namespace StoryDecomposer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoryController : ControllerBase
{
    private readonly StoryDecompositionService _decompositionService;
    private readonly PlanningQuestionsService _questionsService;
    
    public StoryController(
        StoryDecompositionService decompositionService,
        PlanningQuestionsService questionsService)
    {
        _decompositionService = decompositionService;
        _questionsService = questionsService;
    }
    
    [HttpPost("decompose")]
    public async Task<IActionResult> DecomposeStory([FromBody] Story story)
    {
        var decomposition = await _decompositionService.DecomposeStory(story);
        
        var questions = await _questionsService.GenerateQuestions(
            story, 
            decomposition.Tasks);
        
        decomposition.Questions = questions;
        
        return Ok(decomposition);
    }
}
