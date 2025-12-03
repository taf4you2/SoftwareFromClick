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
        public Models Model { get; set; } = null!;

        public int LanguageId { get; set; }
        public Languages Language { get; set; } = null!;

        // Relacje
        public ICollection<Prompts> Prompts { get; set; } = new List<Prompts>();
        public ICollection<Results> Results { get; set; } = new List<Results>();

        // Relacja wiele-do-wielu
        public ICollection<PromptTemplatesUsed> PromptTemplatesUsed { get; set; } = new List<PromptTemplatesUsed>();
    }
}
