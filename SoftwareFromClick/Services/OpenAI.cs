using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SoftwareFromClick.Services
{
<<<<<<< HEAD
    public class OpenAiService
    {
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
        private const string ConfigFileName = "JsonFile1.json";

        public async Task<string> GetCodeFromAiAsync(string userFunctionalityDescription)
=======
    // Obiekt transferu danych (DTO) służący do przechowywania struktury szablonu promptu z pliku JSON.
    public class GeneratorTemplateDto
    {
        public string System { get; set; }
        public string User { get; set; }
    }

    // DTO reprezentujące strukturę żądania wysyłanego do API
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

    // Klasa wiadomość w konwersacji z AI (rola + treść).
    public class MessageDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }

    // Klasa konfiguracyjna używana do odczytu prostych ustawień z plików JSON (klucza API w starszych wersjach).
    public class AppConfig
    {
        public string Key { get; set; }
    }

    // Główny serwis odpowiedzialny za komunikację z modelami sztucznej inteligencji.
    // Zarządza procesem przygotowania promptu, wysyłki żądania oraz zapisu historii.
    public class OpenAiService
    {
        // Ścieżki do folderów przechowywujących historię wygenerowanych promptów oraz otrzymanych wyników.
        private readonly string _historyFolder = Path.Combine(AppContext.BaseDirectory, "History", "Prompts");
        private readonly string _resultsFolder = Path.Combine(AppContext.BaseDirectory, "History", "Results");

        // Konstruktor podczas inicjalizacji sprawdza istnienie wymaganych katalogów i tworzy je, jeśli nie istnieją.
        public OpenAiService()
>>>>>>> 3a59c1b (ddddddd)
        {
            // Wczytanie klucza z JSON
            string Key = LoadApiKey();

<<<<<<< HEAD
            if (string.IsNullOrEmpty(Key))
            {
                return "Error: API Key not found in JsonFile1.json";
            }

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Key}");

                var requestData = new
                {
                    model = "gpt-4o-mini", // dla testów
                    messages = new[]
                    {
                        new { role = "system", content = "Jesteś ekspertem C#. Generuj tylko czysty kod, bez komentarzy markdown." },
                        new { role = "user", content = $"Napisz funkcję C#, która robi to: {userFunctionalityDescription}" }
                    },
                    temperature = 0.7
                };

                try
                {
                    var response = await client.PostAsJsonAsync(ApiUrl, requestData);
=======
        // Główna metoda asynchroniczna przetwarzająca wygenerowanie funkcji.
        // Odpowiada za całą logikę biznesową: od zapisu zapytania w bazie, przez wypełnienie szablonów, aż po wywołanie API.
        public async Task<string> ProcessFunctionRequestAsync(FunctionRequestDto request)
        {
            using (var context = new AppDbContext())
            {
                // System pobiera pierwszego użytkownika z bazy.
                var user = context.Users.FirstOrDefault();
                if (user == null) return "Error: No user found in database.";

                // Tworzony jest nowy obiekt pytania (Question)
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

                // System wyszukuje w bazie aktywny szablon promptu typu "Function" dla wybranego języka programowania.
                var promptTemplate = context.PromptTemplates.FirstOrDefault
                                      (pt => pt.LanguageId == request.SelectedLanguage.Id
                                       && pt.TemplateType == "Function"
                                       && pt.IsActive);

                if (promptTemplate == null)
                    return $"Error: No active 'Function' prompt template found for language ID {request.SelectedLanguage.Id}. Check database 'PromptTemplates' table.";

                // Sprawdzana jest fizyczna obecność pliku szablonu na dysku.
                if (!File.Exists(promptTemplate.JsonFilePath))
                    return $"Error: Template file defined in DB does not exist: {promptTemplate.JsonFilePath}";

                // Następuje odczyt i deserializacja treści szablonu z pliku JSON.
                string templateContent = await File.ReadAllTextAsync(promptTemplate.JsonFilePath);
                var templateDto = JsonSerializer.Deserialize<GeneratorTemplateDto>(templateContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (templateDto == null) return "Error: Failed to parse prompt template JSON.";

                // System dokonuje dynamicznej podmiany placeholderów (np. {{FunctionName}}) na wartości dostarczone przez użytkownika.
                string finalUserContent = templateDto.User
                    .Replace("{{FunctionName}}", request.FunctionName)
                    .Replace("{{FunctionType}}", request.FunctionType)
                    .Replace("{{ReturnType}}", request.ReturnType)
                    .Replace("{{InputParameters}}", request.InputParameters)
                    .Replace("{{FunctionalityDescription}}", request.Functionalities);

                // Wygenerowany prompt jest zapisywany do pliku historii na dysku w celach audytowych.
                string historyFileName = $"{newQuestion.Id}_prompt.json";
                string savedPromptPath = Path.Combine(_historyFolder, historyFileName);
                var filledPromptData = new { System = templateDto.System, User = finalUserContent };
                await File.WriteAllTextAsync(savedPromptPath, JsonSerializer.Serialize(filledPromptData, new JsonSerializerOptions { WriteIndented = true }));

                // Informacja o zapisanym pliku promptu trafia do bazy danych.
                var newPrompt = new Prompt
                {
                    QueryId = newQuestion.Id,
                    JsonFilePath = savedPromptPath,
                    CreatedAt = DateTime.Now
                };
                context.Prompts.Add(newPrompt);

                // System identyfikuje dostawcę (Providera) na podstawie wybranego modelu i pobiera jego szablon konfiguracyjny.
                int providerId = request.SelectedModel.ProviderId;
                var providerTemplate = context.ProviderTemplates
                    .FirstOrDefault(pt => pt.ProviderId == providerId);

                if (providerTemplate == null)
                    return $"Error: No provider configuration template found for Provider ID {providerId}. Check 'ProviderTemplates' table.";

                if (!File.Exists(providerTemplate.JsonFilePath))
                    return $"Error: Provider config file defined in DB does not exist: {providerTemplate.JsonFilePath}";

                // Rejestrowane jest użycie konkretnych szablonów w tabeli łączącej, co pozwala śledzić, z jakich ustawień powstał wynik.
                var usedTemplates = new PromptTemplateUsed
                {
                    QueryId = newQuestion.Id,
                    PromptTemplateId = promptTemplate.Id,
                    ProviderTemplateId = providerTemplate.Id
                };
                context.PromptTemplatesUsed.Add(usedTemplates);

                await context.SaveChangesAsync();

                // Następuje wywołanie metody wysyłającej żądanie do AI.
                // Metoda przekazuje teraz również ProviderId, aby dynamicznie ustalić adres URL i klucz API.
                return await SendRequestToAi(
                    filledPromptData.System,
                    filledPromptData.User,
                    request.SelectedModel.ModelName,
                    newQuestion.Id,
                    providerTemplate.JsonFilePath,
                    request.SelectedModel.ProviderId 
                );
            }
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
>>>>>>> 3a59c1b (ddddddd)

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return $"Error: {response.StatusCode}. Details: {errorContent}";
                    }

<<<<<<< HEAD
                    var responseBody = await response.Content.ReadFromJsonAsync<OpenAiResponse>();
                    return responseBody?.Choices?[0]?.Message?.Content ?? "No content returned";
=======
                    // Odpowiedź JSON jest deserializowana, a wygenerowany kod wyciągany z właściwej struktury.
                    var responseBody = JsonSerializer.Deserialize<OpenAiResponse>(responseString);
                    string generatedCode = responseBody?.Choices?[0]?.Message?.Content ?? "No content returned";

                    // Wynik działania jest zapisywany w historii.
                    await SaveResultAsync(queryId, responseString, generatedCode);

                    return generatedCode;
>>>>>>> 3a59c1b (ddddddd)
                }
                catch (Exception ex)
                {
                    return $"Connection Error: {ex.Message}";
                }
            }
        }

<<<<<<< HEAD
        // Metoda pomocnicza do czytania pliku
        private string LoadApiKey()
=======
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
>>>>>>> 3a59c1b (ddddddd)
        {
            try
            {
<<<<<<< HEAD
                if (!File.Exists(ConfigFileName))
                    return null;

                string jsonContent = File.ReadAllText(ConfigFileName);
                var config = JsonSerializer.Deserialize<AppConfig>(jsonContent);
                return config?.Key;
            }
            catch
            {
                return null;
=======
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
>>>>>>> 3a59c1b (ddddddd)
            }
        }
    }

<<<<<<< HEAD
    // --- Klasa pomocnicza do odczytu klucza ---
    public class AppConfig
    {
        public string Key { get; set; }
    }

    // --- Poniżej klasy odpowiedzi OpenAI ---
=======
    // Reprezentuje główny obiekt odpowiedzi z API (OpenAI).
>>>>>>> 3a59c1b (ddddddd)
    public class OpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    // Reprezentuje pojedynczy wybór/wariant odpowiedzi zwrócony przez model.
    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    // Treść wiadomości zwrotnej.
    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}