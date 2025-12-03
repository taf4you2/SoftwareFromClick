using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public class ApiKeys
    {
        public int Id { get; set; }

        public string ApiKey { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastUsed { get; set; }
        public int UsageCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Klucze obce
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        // Relacje
        public int ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;
    }
}
