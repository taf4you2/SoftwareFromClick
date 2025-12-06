using SoftwareFromClick.Data;
using SoftwareFromClick.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
