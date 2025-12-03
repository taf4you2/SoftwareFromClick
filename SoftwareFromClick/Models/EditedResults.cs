using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public class EditedResults
    {
        public int Id { get; set; }

        // Klucze obce
        public int ResultId { get; set; }
        public Results Result { get; set; } = null!;

        public string EditedCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relacje
        public int EditedBy { get; set; }
        public User Editor { get; set; } = null!;


    }
}
