using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public class Queries
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Klucze obce
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int ModelId { get; set; }
        public Model Model { get; set; } = null!;

        public int LanguageId { get; set; }
        public Language Language { get; set; } = null!;

        // Relacje
        public ICollection<Prompt> Prompts { get; set; } = new List<Prompt>();
        public ICollection<Result> Results { get; set; } = new List<Result>();

        // Relacja wiele-do-wielu
        public ICollection<PromptTemplateUsed> PromptTemplatesUsed { get; set; } = new List<PromptTemplateUsed>();
    }
}
