using Microsoft.EntityFrameworkCore;
using SoftwareFromClick.Data;
using SoftwareFromClick.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace SoftwareFromClick.Services
{
    public class TemplateService
    {
        private readonly string _templatesFolder = Path.Combine(AppContext.BaseDirectory, "Templates", "PromptTemplates");

        public TemplateService() 
        {
            if(!Directory.Exists(_templatesFolder))
            {
                Directory.CreateDirectory(_templatesFolder);
            }
        }

        public List<PromptTemplate> GetAllTemplates()
        {
            using (var context = new AppDbContext())
            {
                return context.PromptTemplates
                    .Include(t => t.Language)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();
            }
        }

        public async Task AddTemplateAsync(string name, string type, int languageId, string systemPrompt, string userPrompt)
        {
            // Przygotowanie obiektu dto do zapisu w pliku
            var templateDto = new GeneratorTemplateDto
            {
                System = systemPrompt,
                User = userPrompt
            };

            // Do JSON
            string jsonString = JsonSerializer.Serialize(templateDto, new JsonSerializerOptions { WriteIndented = true }); // writeindented jest tylko po to żeby zapewnić użytkownikowi(developerowi) możliwość względnie łatwego i czytelnego dostępu do jsona

            // Generowanie bezpiecznej nazwy pliku
            string safeName = string.Join("_", name.Split(Path.GetInvalidFileNameChars()));

            // Tworzymy unikalną nazwę pliku
            string fileName = $"{type}_{safeName}_{DateTime.Now.Ticks}.json";
            string fullPath = Path.Combine(_templatesFolder, fileName);

            // Zapis na dysku
            await File.WriteAllTextAsync(fullPath, jsonString);

            // Zapis bazie danych
            using (var context = new AppDbContext())
            {
                var newTemplate = new PromptTemplate
                {
                    Name = name,
                    TemplateType = type,
                    LanguageId = languageId,
                    JsonFilePath = fullPath,
                    IsActive = true,
                    IsDefault = false,
                    CreatedAt = DateTime.Now
                };

                context.PromptTemplates.Add(newTemplate);
                await context.SaveChangesAsync();
            }
        }

        // Usuwanie szablonu
        public void DeleteTemplate(int id)
        {
            using (var context = new AppDbContext())
            {
                var template = context.PromptTemplates.Find(id);

                if (template != null)
                {
                    // Próbujemy usunąć plik z dysku
                    if (File.Exists(template.JsonFilePath))
                    {
                        try
                        {
                            File.Delete(template.JsonFilePath);
                        }
                        catch{}
                    }

                    // Usuwamy rekord z bazy danych
                    context.PromptTemplates.Remove(template);
                    context.SaveChanges();
                }
            }
        }

    }
}
