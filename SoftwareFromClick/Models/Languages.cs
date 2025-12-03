using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SoftwareFromClick.Models
{
    public class Languages
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;

        // Relacje
        public ICollection<Query> Queries { get; set; } = new List<Query>();
        public ICollection<PromptTemplates> PromptTemplates { get; set; } = new List<PromptTemplates>();
    }
}
