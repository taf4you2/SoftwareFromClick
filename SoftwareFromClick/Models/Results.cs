using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SoftwareFromClick.Models
{
    public class Results
    {
        public int Id { get; set; }
        public int? TokensUsed { get; set; }
        public int? ProcessingTime { get; set; } // ms
        public int? ResponseTime { get; set; }   // ms
        public string Status { get; set; } = string.Empty; // "success", "failed"
        public string JsonFilePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Klucze obce
        public int QueryId { get; set; }
        public Query Query { get; set; } = null!;

        // Relacje
        public ICollection<EditedResults> EditedResults { get; set; } = new List<EditedResults>();
    }
}
