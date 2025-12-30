using SoftwareFromClick.Data;
using SoftwareFromClick.Models;
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
        public string GetCodeFromResult(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath)) return "Error: File not found.";

                string jsonContent = File.ReadAllText(jsonFilePath);

                // Deserializujemy strukturę odpowiedzi OpenAI (korzystamy z klas z OpenAiService)
                // Uwaga: Upewnij się, że klasy OpenAiResponse są publiczne i dostępne
                var response = JsonSerializer.Deserialize<OpenAiResponse>(jsonContent);

                return response?.Choices?[0]?.Message?.Content ?? "No code found in history file.";
            }
            catch (System.Exception ex)
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
