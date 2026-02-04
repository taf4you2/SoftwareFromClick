using SoftwareFromClick.Data;
using SoftwareFromClick.Models;
using SoftwareFromClick.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;

namespace SoftwareFromClick.Services
{
    public class MainService
    {
        // Pobiera listę języków programowania
        public List<Language> GetLanguages()
        {
            using (var context = new AppDbContext())
            {
                return context.Languages.OrderBy(l => l.Name).ToList();
            }
        }

        // Pobiera listę modeli AI
        public List<AiModel> GetAiModels()
        {
            using (var context = new AppDbContext())
            {
                // Include(m => m.Provider) przydałoby się
                return context.AiModels.OrderBy(m => m.ModelName).ToList();
            }
        }

        public List<Question> GetHistory()
        {
            using (var context = new AppDbContext())
            {
                // Pobieramy pytania wraz z wynikami, sortujemy od najnowszych
                return context.Queries
                    .Include(q => q.Results)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToList();
            }
        }

        // Metoda pomocnicza do wyciągania kodu z zapisanego pliku JSON
        public string GetCodeFromResult(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return "Error: File not found.";

                // Wczytaj treść pliku
                string content = File.ReadAllText(filePath);
                string extension = Path.GetExtension(filePath).ToLower();

                // Jeśli to plik JSON, próbujemy dopasować go do znanych formatów (OpenAI lub Gemini)
                if (extension == ".json")
                {
                    // Opcje deserializacji (ignorowanie wielkości liter dla pewności)
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // 1. Próba deserializacji jako OpenAI
                    try
                    {
                        var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(content, options);
                        if (openAiResponse?.Choices != null && openAiResponse.Choices.Count > 0)
                        {
                            return openAiResponse.Choices[0].Message.Content;
                        }
                    }
                    catch { /* To nie jest format OpenAI, idziemy dalej */ }

                    // 2. Próba deserializacji jako Gemini
                    try
                    {
                        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(content, options);
                        if (geminiResponse?.Candidates != null && geminiResponse.Candidates.Count > 0)
                        {
                            // Ścieżka do tekstu w Gemini: Candidates[0] -> Content -> Parts[0] -> Text
                            var candidate = geminiResponse.Candidates[0];
                            if (candidate.Content?.Parts != null && candidate.Content.Parts.Count > 0)
                            {
                                return candidate.Content.Parts[0].Text;
                            }
                        }
                    }
                    catch { /* To nie jest format Gemini, idziemy dalej */ }
                }

                // Domyślnie: Zwróć surową zawartość (dla plików .cpp, .cs lub nierozpoznanych JSON-ów)
                return content;
            }
            catch (Exception ex)
            {
                return $"Error reading history: {ex.Message}";
            }
        }

        // obsługa usuwania z historii
        public void DeleteQuestion(int id)
        {
            using (var context = new AppDbContext())
            {
                var question = context.Queries.Find(id);

                if (question != null)
                {
                    // usuwanie kaskadowe wszystkich rekordów powiązanych
                    context.Queries.Remove(question); // trzeba docelowo zrobić tak żeby nie usuwało kaskadowo (ale to trzeba zmienić AppDbContext)
                    context.SaveChanges();
                }
            }
        }

    }
}
