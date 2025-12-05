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
    public class OpenAiService
    {
        private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
        private const string ConfigFileName = "JsonFile1.json";

        public async Task<string> GetCodeFromAiAsync(string userFunctionalityDescription)
        {
            // Wczytanie klucza z JSON
            string Key = LoadApiKey();

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

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return $"Error: {response.StatusCode}. Details: {errorContent}";
                    }

                    var responseBody = await response.Content.ReadFromJsonAsync<OpenAiResponse>();
                    return responseBody?.Choices?[0]?.Message?.Content ?? "No content returned";
                }
                catch (Exception ex)
                {
                    return $"Connection Error: {ex.Message}";
                }
            }
        }

        // Metoda pomocnicza do czytania pliku
        private string LoadApiKey()
        {
            try
            {
                if (!File.Exists(ConfigFileName))
                    return null;

                string jsonContent = File.ReadAllText(ConfigFileName);
                var config = JsonSerializer.Deserialize<AppConfig>(jsonContent);
                return config?.Key;
            }
            catch
            {
                return null;
            }
        }
    }

    // --- Klasa pomocnicza do odczytu klucza ---
    public class AppConfig
    {
        public string Key { get; set; }
    }

    // --- Poniżej klasy odpowiedzi OpenAI ---
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
    }
}