namespace StoryDecomposer.RAG;

using System.Net.Http.Json;
using System.Text.Json.Serialization;

public sealed class EmbeddingService
{
     private readonly HttpClient _httpClient;
    private readonly string _ollamaUrl = "http://localhost:11434";

    public EmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            model = "phi",
            input = text
        };

        var response = await _httpClient.PostAsJsonAsync($"{_ollamaUrl}/api/embeddings", requestBody);
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<EmbeddingResponse>();
        return responseData?.Embedding ?? Array.Empty<float>();
    }
}

public sealed class EmbeddingResponse
{
    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();
}
