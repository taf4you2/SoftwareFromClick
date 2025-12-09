using Microsoft.EntityFrameworkCore;
using SoftwareFromClick.Data;
using SoftwareFromClick.Models;
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
    // DTO pomocnicze do wczytywania szablonu promptu (Generator)
    public class GeneratorTemplateDto
    {
        public string System { get; set; }
        public string User { get; set; }
    }

    // DTO do wysyłania żądania (Request do OpenAI)
    public class OpenAiRequestDto
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-4o-mini";

        [JsonPropertyName("messages")]
        public List<MessageDto> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.2;

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }
    }

    public class MessageDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    public class OpenAiService
    {
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

        // Folder historii (tylko do zapisu nowych plików)
        private readonly string _historyFolder = Path.Combine(AppContext.BaseDirectory, "History", "Prompts");
        private readonly string _resultsFolder = Path.Combine(AppContext.BaseDirectory, "History", "Results");

        public OpenAiService()
        {
            if (!Directory.Exists(_historyFolder)) Directory.CreateDirectory(_historyFolder);
            if (!Directory.Exists(_resultsFolder)) Directory.CreateDirectory(_resultsFolder);
        }


        

        // --- KROK 2 (Zmodyfikowany): Logika oparta wyłącznie o DB ---
        public async Task<string> ProcessFunctionRequestAsync(FunctionRequestDto request)
        {
            using (var context = new AppDbContext())
            {
                // 1. ZNAJDŹ UŻYTKOWNIKA
                var user = context.Users.FirstOrDefault();
                if (user == null) return "Error: No user found in database.";

                // 2. UTWÓRZ REKORD ZAPYTANIA (QUESTION)
                var newQuestion = new Question
                {
                    Title = request.FunctionName,
                    Notes = "Generated from Function View via DB Logic",
                    CreatedAt = DateTime.Now,
                    UserId = user.Id,
                    ModelId = request.SelectedModel.Id,
                    LanguageId = request.SelectedLanguage.Id
                };

                context.Queries.Add(newQuestion);
                await context.SaveChangesAsync();

                // 3. POBIERZ PROMPT TEMPLATE Z BAZY (Strict DB)
                // Szukamy szablonu dla wybranego języka i typu "Function"
                // Upewnij się, że w bazie w kolumnie TemplateType masz wpisane "Function" lub "function"
                var promptTemplate = context.PromptTemplates
                    .FirstOrDefault(pt => pt.LanguageId == request.SelectedLanguage.Id
                                       && pt.TemplateType == "Function" // WAŻNE: Musi pasować do tego co w bazie
                                       && pt.IsActive);

                if (promptTemplate == null)
                    return $"Error: No active 'Function' prompt template found for language ID {request.SelectedLanguage.Id}.";

                if (!File.Exists(promptTemplate.JsonFilePath))
                    return $"Error: Template file defined in DB does not exist: {promptTemplate.JsonFilePath}";

                // 4. WCZYTAJ I WYPEŁNIJ SZABLON
                string templateContent = await File.ReadAllTextAsync(promptTemplate.JsonFilePath);
                var templateDto = JsonSerializer.Deserialize<GeneratorTemplateDto>(templateContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (templateDto == null) return "Error: Failed to parse prompt template JSON.";

                string finalUserContent = templateDto.User
                    .Replace("{{FunctionName}}", request.FunctionName)
                    .Replace("{{FunctionType}}", request.FunctionType)
                    .Replace("{{ReturnType}}", request.ReturnType)
                    .Replace("{{InputParameters}}", request.InputParameters)
                    .Replace("{{FunctionalityDescription}}", request.Functionalities);

                // 5. ZAPISZ HISTORIĘ (Jako nowy plik JSON)
                string historyFileName = $"{newQuestion.Id}_prompt.json";
                string savedPromptPath = Path.Combine(_historyFolder, historyFileName);

                var filledPromptData = new { System = templateDto.System, User = finalUserContent };
                await File.WriteAllTextAsync(savedPromptPath, JsonSerializer.Serialize(filledPromptData, new JsonSerializerOptions { WriteIndented = true }));

                // 6. UTWÓRZ REKORD PROMPT W BAZIE
                var newPrompt = new Prompt
                {
                    QueryId = newQuestion.Id,
                    JsonFilePath = savedPromptPath, // Ścieżka do utworzonego właśnie pliku historii
                    CreatedAt = DateTime.Now
                };
                context.Prompts.Add(newPrompt);

                // 7. POBIERZ PROVIDER TEMPLATE Z BAZY (Strict DB)
                // ProviderId bierzemy z modelu wybranego przez użytkownika
                int providerId = request.SelectedModel.ProviderId;

                var providerTemplate = context.ProviderTemplates
                    .FirstOrDefault(pt => pt.ProviderId == providerId);

                if (providerTemplate == null)
                    return $"Error: No provider configuration template found for Provider ID {providerId}.";

                if (!File.Exists(providerTemplate.JsonFilePath))
                    return $"Error: Provider config file defined in DB does not exist: {providerTemplate.JsonFilePath}";

                // 8. ZAREJESTRUJ UŻYCIE SZABLONÓW
                var usedTemplates = new PromptTemplateUsed
                {
                    QueryId = newQuestion.Id,
                    PromptTemplateId = promptTemplate.Id,
                    ProviderTemplateId = providerTemplate.Id
                };
                context.PromptTemplatesUsed.Add(usedTemplates);

                await context.SaveChangesAsync();

                // 9. WYŚLIJ DO AI (Przekazujemy ścieżkę do konfigu providera z bazy)
                return await SendRequestToAi(
                    filledPromptData.System,
                    filledPromptData.User,
                    newQuestion.Id,
                    providerTemplate.JsonFilePath // <--- Używamy ścieżki z bazy!
                );
            }
        }

        // --- KROK 3: Wyślij Request (Używając konfigu z DB) ---
        private async Task<string> SendRequestToAi(string systemMsg, string userMsg, int queryId, string providerConfigPath)
        {
            string apiKey = LoadApiKey();
            if (string.IsNullOrEmpty(apiKey)) return "Error: API Key not found.";

            OpenAiRequestDto requestData;

            // Wczytujemy konfig providera ze ścieżki podanej w argumencie (pochodzącej z bazy)
            try
            {
                string jsonContent = await File.ReadAllTextAsync(providerConfigPath);
                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip, // Ignorujemy komentarze // w JSON
                    PropertyNameCaseInsensitive = true
                };
                requestData = JsonSerializer.Deserialize<OpenAiRequestDto>(jsonContent, options);
            }
            catch (Exception ex)
            {
                return $"Error loading provider config from {providerConfigPath}: {ex.Message}";
            }

            if (requestData == null) return "Error: Provider config is empty.";

            // Ustawiamy wiadomości
            requestData.Messages = new List<MessageDto>
            {
                new MessageDto { Role = "system", Content = systemMsg },
                new MessageDto { Role = "user", Content = userMsg }
            };

            // Wykonanie zapytania HTTP
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.Timeout = TimeSpan.FromSeconds(90); // Dłuższy czas na kod

                try
                {
                    var response = await client.PostAsJsonAsync(ApiUrl, requestData);
                    string responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return $"Error: {response.StatusCode}. Details: {responseString}";
                    }

                    // Deserializacja odpowiedzi
                    var responseBody = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
                    string generatedCode = responseBody?.Choices?[0]?.Message?.Content ?? "No content returned";

                    // KROK 4: Zapis wyniku
                    await SaveResultAsync(queryId, responseString, generatedCode);

                    return generatedCode;
                }
                catch (Exception ex)
                {
                    return $"Connection Error: {ex.Message}";
                }
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

        private string LoadApiKey()
        {
            using (var context = new AppDbContext())
            {
                // Pobieramy pierwszy aktywny klucz dla dostawcy OpenAI
                // Zakładamy, że Provider o nazwie "OpenAI" lub "OpenAI (GPT)" ma Id, które możemy znaleźć
                // Lub po prostu bierzemy pierwszy aktywny klucz użytkownika dla uproszczenia

                var keyEntity = context.ApiKeys
                    .Include(k => k.Provider)
                    .Where(k => k.IsActive && k.Provider.Name.Contains("OpenAI"))
                    .OrderByDescending(k => k.CreatedAt)
                    .FirstOrDefault();

                return keyEntity?.ApiKey;
            }
        }
    }

    public class OpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }


}