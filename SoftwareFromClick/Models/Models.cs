using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SoftwareFromClick.Models
{
    public class Models
    {
        public int Id { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequiresApiKey { get; set; }

        // Klucze obce
        public int ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;

        // Relacje
        public ICollection<Query> Queries { get; set; } = new List<Query>();
    }
}
