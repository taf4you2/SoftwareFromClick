using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public class TemplateCategories
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Relacje
        public ICollection<PromptTemplates> PromptTemplates { get; set; } = new List<PromptTemplates>();
    }
}
