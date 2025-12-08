using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public class Provider
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        // Relacje
        public ICollection<ApiKeys> ApiKeys { get; set; } = new List<ApiKeys>();
        public ICollection<AiModel> Models { get; set; } = new List<AiModel>();

        public ICollection<ProviderTemplate> ProviderTemplates { get; set; } = new List<ProviderTemplate>();
    }
}
