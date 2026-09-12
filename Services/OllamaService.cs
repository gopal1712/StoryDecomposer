using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace StoryDecomposer.Services;

public sealed class OllamaService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    public OllamaService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
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
            model = _configuration["Ollama:Model"] ?? "tinyllama",
            prompt = fullPrompt,
            stream = false,
            format = "json",
            temperature = 0.2,
            top_p = 0.9,
            keep_alive = _configuration["Ollama:KeepAlive"] ?? "10m",
            options = new
            {
                num_predict = _configuration.GetValue("Ollama:NumPredict", 384)
            },
                top_k = 40,
                repeat_penalty = 1.1,
                num_predict = 800,      // Phi is efficient, can handle larger outputs
                num_ctx = 2048,         // Phi supports 2K context
                num_gpu = -1            // Use GPU if available, else CPU
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            $"{_configuration["Ollama:BaseUrl"] ?? "http://localhost:11434"}/api/generate", 
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
