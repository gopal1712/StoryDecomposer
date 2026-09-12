using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace StoryDecomposer.Services;

public sealed class OllamaService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl = "http://localhost:11434";
    private readonly string _model = "phi";
    
    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> Generate(string prompt, string context = "")
    {
        var fullPrompt = $@"
Context:
{context}

Task:
{prompt}

Response:";
        
        var request = new
        {
            model = _model,
            prompt = fullPrompt,
            stream = false,
            format = "json",
            temperature = 0.7,
            top_p = 0.95
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            $"{_ollamaUrl}/api/generate", 
            request);
        
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GenerateResponse>();
        return result?.Response ?? string.Empty;
    }
}

public class GenerateResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;
}
