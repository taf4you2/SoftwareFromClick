using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public class PromptTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty; // 'function', 'class'
        public bool IsDefault { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public string JsonFilePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Klucze obce
        public int LanguageId { get; set; }
        public Language Language { get; set; } = null!;

        public int? CategoryId { get; set; }
        public TemplateCategories? Category { get; set; }

        // Relacja wiele-do-wielu
        public ICollection<PromptTemplateUsed> PromptTemplatesUsed { get; set; } = new List<PromptTemplateUsed>();
    }
}
