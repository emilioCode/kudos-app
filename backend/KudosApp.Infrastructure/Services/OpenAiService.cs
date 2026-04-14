using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KudosApp.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace KudosApp.Infrastructure.Services;

public class OpenAiService(HttpClient httpClient, IConfiguration configuration) : IOpenAiService
{
    private readonly string _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey missing");
    private readonly string _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";

    public async Task<string> SuggestKudosMessageAsync(string recipientName, string categoryName)
    {
        try
        {
            var systemPrompt = "You are an assistant that generates warm, positive, and concise kudos messages for employees. Respond with only the message text.";
            var userPrompt = $"Write a kudos message for {recipientName} in the category '{categoryName}'.";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);

            var suggestion = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return suggestion?.Trim() ?? "Great job!";
        }
        catch
        {
            return "Great job!";
        }
    }

    public async Task<string> SuggestCategoryAsync(string message, List<string> availableCategories)
    {
        try
        {
            var categories = string.Join(", ", availableCategories);
            var systemPrompt = "You are an assistant that categorizes kudos messages. Given a message and a list of categories, respond with only the best matching category name from the list.";
            var userPrompt = $"Message: \"{message}\"\nAvailable categories: {categories}";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            var json = await JsonDocument.ParseAsync(stream);

            var suggestion = json.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return suggestion?.Trim() ?? availableCategories.FirstOrDefault() ?? "Teamwork";
        }
        catch
        {
            return availableCategories.FirstOrDefault() ?? "Teamwork";
        }
    }
}
