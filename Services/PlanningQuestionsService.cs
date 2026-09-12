using Newtonsoft.Json.Linq;
using StoryDecomposer.Models;

namespace StoryDecomposer.Services;

public class PlanningQuestionsService
{
    private const string PromptFileName = "questions-generation-prompt.txt";
    private readonly ILLMService _llmService;
    private readonly RAGService _ragService;

    public PlanningQuestionsService(ILLMService llmService, RAGService ragService)
    {
        _llmService = llmService;
        _ragService = ragService;
    }
    
    public async System.Threading.Tasks.Task<List<string>> GenerateQuestions(
        Story story,
        List<StoryDecomposer.Models.Task> tasks)
    {
        var context = await _ragService.GetContext(story.AcceptanceCriteria);
        
        var tasksText = string.Join("\n", tasks.Select(t => $"- {t.Title}"));
        
        var prompt = await LoadPrompt(PromptFileName, story.AcceptanceCriteria, tasksText);
        
        var response = await _llmService.Generate(prompt, context);
        var result = LlmJsonParser.ParseObject(response);
        
        return (result?["questions"] as JArray ?? [])
            .Values<string>()
            .Where(question => !string.IsNullOrWhiteSpace(question))
            .ToList()!;
    }

    private static async System.Threading.Tasks.Task<string> LoadPrompt(
        string fileName,
        string acceptanceCriteria,
        string tasks)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "prompts", fileName);
        var template = await File.ReadAllTextAsync(path);
        return template
            .Replace("{acceptanceCriteria}", acceptanceCriteria)
            .Replace("{tasks}", tasks);
    }
}