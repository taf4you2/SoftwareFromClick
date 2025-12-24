using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SoftwareFromClick.Models
{
    public class Prompt
    {
        public int Id { get; set; }
        public string JsonFilePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Klucze obce
        public int QueryId { get; set; }
        public Question Query { get; set; } = null!;
    }
}
