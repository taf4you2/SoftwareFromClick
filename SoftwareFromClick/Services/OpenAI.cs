using Microsoft.EntityFrameworkCore;
using SoftwareFromClick.Data;
using SoftwareFromClick.Models;
// Jeśli twoje OpenAiRequestDto jest w osobnym namespace, odkomentuj poniższą linię:
using SoftwareFromClick.Models.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoftwareFromClick.Services
{
    public class OpenAiService
    {
        private readonly string _historyFolder = Path.Combine(AppContext.BaseDirectory, "History", "Prompts");
        private readonly string _resultsFolder = Path.Combine(AppContext.BaseDirectory, "History", "Results");

        public OpenAiService()
        {
            if (!Directory.Exists(_historyFolder)) Directory.CreateDirectory(_historyFolder);
            if (!Directory.Exists(_resultsFolder)) Directory.CreateDirectory(_resultsFolder);
        }

        public async Task<string> ProcessGenerationRequestAsync(
            string title,
            string templateType,
            Language language,
            AiModel model,
            Dictionary<string, string> placeholders
            )
        {
            using (var context = new AppDbContext())
            {
                var user = context.Users.FirstOrDefault();
                if (user == null) return "Error: no user found";

                var newQuestion = new Question
                {
                    Title = title,
                    CreatedAt = DateTime.Now,
                    UserId = user.Id,
                    ModelId = model.Id,
                    LanguageId = language.Id
                };

                context.Queries.Add(newQuestion);
                await context.SaveChangesAsync();

                var promptTemplate = context.PromptTemplates.FirstOrDefault(pt => pt.LanguageId == language.Id
                                         && pt.TemplateType == templateType
                                         && pt.IsActive);

                if (promptTemplate == null) return $"Error: no template {templateType} found for language {language.Name}";
                if (!File.Exists(promptTemplate.JsonFilePath)) return $"Error: template file not found {promptTemplate.JsonFilePath}";

                string templateContent = await File.ReadAllTextAsync(promptTemplate.JsonFilePath);
                var templateDto = JsonSerializer.Deserialize<GeneratorTemplateDto>(templateContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (templateDto == null) return "Error: failed to parse prompttemplate from JSON";

                string finalUserContent = templateDto.User;
                string finalSystemContent = templateDto.System;

                foreach (var item in placeholders)
                {
                    string key = item.Key;
                    string valueToInsert = item.Value ?? string.Empty;

                    finalSystemContent = finalSystemContent.Replace(key, valueToInsert);
                    finalUserContent = finalUserContent.Replace(key, valueToInsert);
                }

                string historyFileName = $"prompt_{newQuestion.Id}.json";
                string savedPromptPath = Path.Combine(_historyFolder, historyFileName);
                var filledPromptData = new { System = finalSystemContent, User = finalUserContent };

                await File.WriteAllTextAsync(savedPromptPath, JsonSerializer.Serialize(filledPromptData, new JsonSerializerOptions { WriteIndented = true }));

                var newPrompt = new Prompt
                {
                    QueryId = newQuestion.Id,
                    JsonFilePath = savedPromptPath,
                    CreatedAt = DateTime.Now
                };
                context.Prompts.Add(newPrompt);

                int providerId = model.ProviderId;
                var providerTemplate = context.ProviderTemplates.FirstOrDefault(pt => pt.ProviderId == providerId);

                if (providerTemplate == null)
                    return $"Error: No provider configuration found for Provider ID {providerId}.";

                var usedTemplates = new PromptTemplateUsed
                {
                    QueryId = newQuestion.Id,
                    PromptTemplateId = promptTemplate.Id,
                    ProviderTemplateId = providerTemplate.Id
                };
                context.PromptTemplatesUsed.Add(usedTemplates);

                await context.SaveChangesAsync();

                return await SendRequestToAi(
                    filledPromptData.System,
                    filledPromptData.User,
                    model.ModelName,
                    newQuestion.Id,
                    providerTemplate.JsonFilePath,
                    model.ProviderId
                );
            }
        }

        private async Task<string> SendRequestToAi(string systemMsg, string userMsg, string modelName, int queryId, string providerConfigPath, int providerId)
        {
            var (apiKey, apiUrl) = GetProviderDetails(providerId);

            // Zabezpieczenie przed białymi znakami (np. spacja na końcu linku)
            apiKey = apiKey?.Trim();
            apiUrl = apiUrl?.Trim();

            if (string.IsNullOrEmpty(apiKey)) return "Error: API Key not found for this provider. Please add it in Settings.";
            if (string.IsNullOrEmpty(apiUrl)) return "Error: Provider URL is missing in database.";

            // Wykrywanie Gemini po URL
            if (apiUrl.Contains("google") || apiUrl.Contains("generativelanguage"))
            {
                return await SendRequestToGemini(systemMsg, userMsg, modelName, queryId, apiKey, apiUrl);
            }

            // --- Logika dla OpenAI ---
            OpenAiRequestDto requestData;
            try
            {
                string jsonContent = await File.ReadAllTextAsync(providerConfigPath);
                var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, PropertyNameCaseInsensitive = true };
                requestData = JsonSerializer.Deserialize<OpenAiRequestDto>(jsonContent, options);
            }
            catch (Exception ex)
            {
                return $"Error loading provider config from {providerConfigPath}: {ex.Message}";
            }

            if (requestData == null) return "Error: Provider config is empty.";

            requestData.Model = modelName;
            requestData.Messages = new List<MessageDto>
            {
                new MessageDto { Role = "system", Content = systemMsg },
                new MessageDto { Role = "user", Content = userMsg }
            };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.Timeout = TimeSpan.FromSeconds(90);

                try
                {
                    var response = await client.PostAsJsonAsync(apiUrl, requestData);
                    string responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return $"Error ({response.StatusCode}): {responseString}";
                    }

                    var responseBody = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
                    string generatedCode = responseBody?.Choices?[0]?.Message?.Content ?? "No content returned";

                    await SaveResultAsync(queryId, responseString, generatedCode);
                    return generatedCode;
                }
                catch (Exception ex)
                {
                    return $"Connection Error: {ex.Message}";
                }
            }
        }

        private async Task<string> SendRequestToGemini(string systemMsg, string userMsg, string modelName, int queryId, string apiKey, string baseUrl)
        {
            string requestUrl = "";
            try
            {
                // Budowanie URL: BaseUrl + ModelName + :generateContent?key=API_KEY
                string cleanBaseUrl = baseUrl.TrimEnd('/');

                // Usunięcie prefiksu "models/", jeśli użytkownik go wpisał w bazie
                string cleanModelName = modelName.StartsWith("models/") ? modelName.Replace("models/", "") : modelName;
                cleanModelName = cleanModelName.Trim();

                requestUrl = $"{cleanBaseUrl}/{cleanModelName}:generateContent?key={apiKey}";

                // Budowanie Body dla Gemini
                var requestData = new GeminiRequestDto
                {
                    Contents = new List<GeminiContent>
                    {
                        new GeminiContent
                        {
                            Role = "user",
                            Parts = new List<GeminiPart>
                            {
                                // Łączymy System i User Prompt
                                new GeminiPart { Text = $"{systemMsg}\n\n---\n\n{userMsg}" }
                            }
                        }
                    }
                };

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(90);

                    var response = await client.PostAsJsonAsync(requestUrl, requestData);
                    string responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        // Zwracamy URL w błędzie (z ukrytym kluczem), żeby łatwiej debugować
                        string maskedUrl = requestUrl.Replace(apiKey, "***");
                        return $"Gemini Error ({response.StatusCode}). URL: {maskedUrl}. Details: {responseString}";
                    }

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var responseBody = JsonSerializer.Deserialize<GeminiResponse>(responseString, options);

                    string generatedCode = responseBody?.Candidates?[0]?.Content?.Parts?[0]?.Text;

                    if (string.IsNullOrEmpty(generatedCode))
                    {
                        generatedCode = "No content returned from Gemini.";
                    }

                    await SaveResultAsync(queryId, responseString, generatedCode);
                    return generatedCode;
                }
            }
            catch (Exception ex)
            {
                return $"Gemini Connection Error: {ex.Message}";
            }
        }

        private async Task SaveResultAsync(int queryId, string fullJson, string generatedCode)
        {
            using (var context = new AppDbContext())
            {
                string fileName = $"{queryId}_result.json";
                string filePath = Path.Combine(_resultsFolder, fileName);

                await File.WriteAllTextAsync(filePath, fullJson);

                var result = new Result
                {
                    QueryId = queryId,
                    Status = "Success",
                    JsonFilePath = filePath,
                    CreatedAt = DateTime.Now
                };

                context.Results.Add(result);
                await context.SaveChangesAsync();
            }
        }

        private (string ApiKey, string Url) GetProviderDetails(int providerId)
        {
            using (var context = new AppDbContext())
            {
                var provider = context.Providers
                    .Include(p => p.ApiKeys)
                    .FirstOrDefault(p => p.Id == providerId);

                if (provider == null) return (null, null);

                var keyEntity = provider.ApiKeys
                    .Where(k => k.IsActive)
                    .OrderByDescending(k => k.CreatedAt)
                    .FirstOrDefault();

                return (keyEntity?.ApiKey, provider.Url);
            }
        }
    }

    // --- KLASY DTO DLA GEMINI ---
    public class GeminiRequestDto
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = new();
    }

    public class GeminiContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = new();
    }

    public class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate> Candidates { get; set; }
    }

    public class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string FinishReason { get; set; }
    }
}