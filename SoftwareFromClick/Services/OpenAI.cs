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
    // Główny serwis odpowiedzialny za komunikację z modelami sztucznej inteligencji.
    // Zarządza procesem przygotowania promptu, wysyłki żądania oraz zapisu historii.
    public class OpenAiService
    {
        // Ścieżki do folderów przechowywujących historię wygenerowanych promptów oraz otrzymanych wyników.
        private readonly string _historyFolder = Path.Combine(AppContext.BaseDirectory, "History", "Prompts");
        private readonly string _resultsFolder = Path.Combine(AppContext.BaseDirectory, "History", "Results");

        // Konstruktor podczas inicjalizacji sprawdza istnienie wymaganych katalogów i tworzy je, jeśli nie istnieją.
        public OpenAiService()
        {
            if (!Directory.Exists(_historyFolder)) Directory.CreateDirectory(_historyFolder);
            if (!Directory.Exists(_resultsFolder)) Directory.CreateDirectory(_resultsFolder);
        }

        // Główna metoda asynchroniczna przetwarzająca generowanie wszystkiego (pewnie będzie zmieniana jeszcze)

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
                // Walidacja uzytkownika
                var user = context.Users.FirstOrDefault();
                if (user == null) return "Error: no user found";

                // Tworzenie wpisu pytania do bazy
                var newQuestion = new Question
                {
                    Title = title,
                    CreatedAt = DateTime.Now,
                    UserId = user.Id,
                    ModelId = model.Id,
                    LanguageId = language.Id

                };
                
                context.Queries.Add( newQuestion );
                await context.SaveChangesAsync();

                // Pobranie szablonu prompta
                var promptTemplate = context.PromptTemplates.FirstOrDefault
                                        (pt => pt.LanguageId == language.Id
                                         && pt.TemplateType == templateType
                                         && pt.IsActive);
                if (promptTemplate == null) return $"Error: no templete {templateType} found for language {language.Name}";
                if (!File.Exists(promptTemplate.JsonFilePath)) return $"Error: template file not found {promptTemplate.JsonFilePath}";

                // Deserializacja szablonu json
                string templateContent = await File.ReadAllTextAsync(promptTemplate.JsonFilePath);
                var templateDto = JsonSerializer.Deserialize<GeneratorTemplateDto>(templateContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (templateDto == null) return "Error: failed to parse prompttemplate from JSON";

                // Zamiana placeholderów na wartości z formularza
                string finalUserContent = templateDto.User;
                string finalSystemContent = templateDto.System;

                foreach (var item in placeholders)
                {
                    string key = item.Key;            // Np. "{{ClassName}}" lub "{(Access)(public, private)}"
                    string valueToInsert = item.Value ?? string.Empty; // Np. "OrderManager" lub "public"

                    // 1. Próba prostej zamiany
                    finalSystemContent = finalSystemContent.Replace(key, valueToInsert);
                    finalUserContent = finalUserContent.Replace(key, valueToInsert);
                }

                // Zapis promptu do bazy i do pliku
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

                // Pobranie dostawcy
                int providerId = model.ProviderId;
                var providerTemplate = context.ProviderTemplates.FirstOrDefault(pt => pt.ProviderId == providerId);

                if (providerTemplate == null)
                    return $"Error: No provider configuration found for Provider ID {providerId}.";

                // Zapis w baze użytych szablonów
                var usedTemplates = new PromptTemplateUsed
                {
                    QueryId = newQuestion.Id,
                    PromptTemplateId = promptTemplate.Id,
                    ProviderTemplateId = providerTemplate.Id
                };
                context.PromptTemplatesUsed.Add(usedTemplates);

                await context.SaveChangesAsync();

                // Wywołanie AI //za jakie grzechy
                return await SendRequestToAi(
                    filledPromptData.System,
                    filledPromptData.User,
                    model.ModelName,
                    newQuestion.Id,
                    providerTemplate.JsonFilePath,
                    model.ProviderId
                );
            }
            return null;
        }

        // Metoda pomocnicza realizująca fizyczne połączenie HTTP z API dostawcy.
        // Odpowiada za przygotowanie JSON-a żądania, wysyłkę oraz odebranie odpowiedzi.
        private async Task<string> SendRequestToAi(string systemMsg, string userMsg, string modelName, int queryId, string providerConfigPath, int providerId)
        {
            // System pobiera dynamicznie klucz API oraz adres URL punktu końcowego dla wskazanego dostawcy.
            var (apiKey, apiUrl) = GetProviderDetails(providerId);

            // Sprawdzana jest poprawność pobranych danych uwierzytelniających.
            if (string.IsNullOrEmpty(apiKey)) return "Error: API Key not found for this provider. Please add it in Settings.";
            if (string.IsNullOrEmpty(apiUrl)) return "Error: Provider URL is missing in database.";

            OpenAiRequestDto requestData;

            try
            {
                // Wczytywana jest konfiguracja żądania z pliku JSON dostawcy.
                string jsonContent = await File.ReadAllTextAsync(providerConfigPath);

                var options = new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNameCaseInsensitive = true
                };
                requestData = JsonSerializer.Deserialize<OpenAiRequestDto>(jsonContent, options);
            }
            catch (Exception ex)
            {
                return $"Error loading provider config from {providerConfigPath}: {ex.Message}";
            }

            if (requestData == null) return "Error: Provider config is empty.";

            // Obiekt żądania jest aktualizowany o nazwę modelu wybraną przez użytkownika.
            requestData.Model = modelName;

            // Lista wiadomości w żądaniu jest nadpisywana treściami wygenerowanymi z szablonów promptu.
            requestData.Messages = new List<MessageDto>
            {
                new MessageDto { Role = "system", Content = systemMsg },
                new MessageDto { Role = "user", Content = userMsg }
            };

            using (HttpClient client = new HttpClient())
            {
                // Klient HTTP ustawia nagłówek autoryzacyjny oraz wydłużony czas oczekiwania na odpowiedź.
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                client.Timeout = TimeSpan.FromSeconds(90);

                try
                {
                    // Wysyłane jest żądanie POST pod dynamicznie pobrany adres URL (apiUrl).
                    var response = await client.PostAsJsonAsync(apiUrl, requestData);
                    string responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        return $"Error: {response.StatusCode}. Details: {responseString}";
                    }

                    // Odpowiedź JSON jest deserializowana, a wygenerowany kod wyciągany z właściwej struktury.
                    var responseBody = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
                    string generatedCode = responseBody?.Choices?[0]?.Message?.Content ?? "No content returned";

                    // Wynik działania jest zapisywany w historii.
                    await SaveResultAsync(queryId, responseString, generatedCode);

                    return generatedCode;
                }
                catch (Exception ex)
                {
                    return $"Connection Error: {ex.Message}";
                }
            }
        }

        // Metoda asynchroniczna zapisująca surową odpowiedź JSON oraz wyekstrahowany kod do pliku i bazy danych.
        private async Task SaveResultAsync(int queryId, string fullJson, string generatedCode)
        {
            using (var context = new AppDbContext())
            {
                string fileName = $"{queryId}_result.json";
                string filePath = Path.Combine(_resultsFolder, fileName);

                // Zapisywana jest pełna treść odpowiedzi JSON na dysku.
                await File.WriteAllTextAsync(filePath, fullJson);

                // W bazie danych tworzony jest wpis rejestrujący sukces operacji i ścieżkę do pliku wyniku.
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

        // Metoda pomocnicza pobierająca szczegóły dostawcy z bazy danych.
        // Zwraca krotkę (Tuple) zawierającą klucz API oraz adres URL.
        private (string ApiKey, string Url) GetProviderDetails(int providerId)
        {
            using (var context = new AppDbContext())
            {
                // Pobierany jest rekord dostawcy wraz z powiązanymi kluczami API.
                var provider = context.Providers
                    .Include(p => p.ApiKeys)
                    .FirstOrDefault(p => p.Id == providerId);

                if (provider == null) return (null, null);

                // System wybiera najnowszy aktywny klucz API dla danego dostawcy.
                var keyEntity = provider.ApiKeys
                    .Where(k => k.IsActive)
                    .OrderByDescending(k => k.CreatedAt)
                    .FirstOrDefault();

                return (keyEntity?.ApiKey, provider.Url);
            }
        }
    }

}