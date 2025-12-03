using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public class Sessions
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Klucze obce
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}
