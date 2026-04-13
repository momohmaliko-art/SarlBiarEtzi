namespace SarlBiarEtzi.Services;

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class GroqService
{
    private readonly IConfiguration _config;

    public GroqService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> SendMessage(string message)
    {
        var apiKey = _config["Groq:ApiKey"];

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var body = new
        {
            model = "llama-3.1-8b-instant",
            messages = new[]
            {
                new { role = "system", content = "You are a support assistant" },
                new { role = "user", content = message }
            }
        };

        var json = JsonSerializer.Serialize(body);

        var response = await client.PostAsync(
            "https://api.groq.com/openai/v1/chat/completions",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var jsonResponse = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonResponse);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content;
    }
}
