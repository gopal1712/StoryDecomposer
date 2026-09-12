using Newtonsoft.Json.Linq;
using StoryDecomposer.Models;

namespace StoryDecomposer.Services;

public class StoryDecompositionService
{
    private const string PromptFileName = "story-analysis-prompt.txt";
    private readonly ILLMService _llmService;
    private readonly RAGService _ragService;
    
    public StoryDecompositionService(ILLMService llmService, RAGService ragService)
    {
        _llmService = llmService;
        _ragService = ragService;
    }
    
    public async System.Threading.Tasks.Task<DecompositionResult> DecomposeStory(Story story)
    {
        // Get context from similar acceptance criteria in RAG
        var context = await _ragService.GetContext(story.AcceptanceCriteria);
        
        var prompt = await LoadPrompt(PromptFileName, story.AcceptanceCriteria, context);
        
        var response = await _llmService.Generate(prompt, context);
        
        // Parse JSON response
        var result = LlmJsonParser.ParseObject(response);
        
        // Convert to DecompositionResult
        var tasks = ConvertToTasks(result["tasks"]);

        return new DecompositionResult
        {
            Tasks = tasks,
            Questions = ConvertToQuestions(result["questions"]),
            Reasoning = ConvertToReasoning(result["reasoning"]),
            EstimatedStoryPoints = ParseEstimate(result["estimatedStoryPoints"])
        };
    }

    private static List<string> ConvertToQuestions(JToken? questionsToken)
    {
        return (questionsToken as JArray ?? [])
            .Values<string>()
            .Where(question => !string.IsNullOrWhiteSpace(question))
            .ToList()!;
    }

    private static int ParseEstimate(JToken? estimateToken)
    {
        return estimateToken?.Type switch
        {
            JTokenType.Integer => estimateToken.Value<int>(),
            JTokenType.String when int.TryParse(estimateToken.Value<string>(), out var estimate) => estimate,
            _ => 0
        };
    }

    private static List<string> ConvertToReasoning(JToken? reasoningToken)
    {
        if (reasoningToken is JObject reasoningObject)
        {
            return reasoningObject.Properties()
                .Select(property => property.Value.Type == JTokenType.String
                    ? property.Value.Value<string>()
                    : property.Value.ToString(Newtonsoft.Json.Formatting.None))
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList()!;
        }

        if (reasoningToken is JArray reasoningArray)
        {
            return reasoningArray.Values<string>()
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList()!;
        }

        var reasoning = reasoningToken?.Value<string>();
        return string.IsNullOrWhiteSpace(reasoning) ? [] : [reasoning];
    }

    private static async System.Threading.Tasks.Task<string> LoadPrompt(
        string fileName,
        string acceptanceCriteria,
        string context)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "prompts", fileName);
        var template = await File.ReadAllTextAsync(path);
        return template
            .Replace("{acceptanceCriteria}", acceptanceCriteria)
            .Replace("{context}", context);
    }
    
    private List<StoryDecomposer.Models.Task> ConvertToTasks(JToken? tasksArray)
    {
        var tasks = new List<StoryDecomposer.Models.Task>();
        foreach (var taskJson in tasksArray as JArray ?? new JArray())
        {
            tasks.Add(new StoryDecomposer.Models.Task
            {
                Title = taskJson.Value<string>("title") ?? string.Empty,
                Description = taskJson.Value<string>("description") ?? string.Empty,
                AreaOfChange = taskJson.Value<string>("areaOfChange") ?? string.Empty
            });
        }
        return tasks;
    }
}