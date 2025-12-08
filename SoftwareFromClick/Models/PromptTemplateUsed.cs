using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SoftwareFromClick.Models
{
    public class PromptTemplateUsed
    {
        public int Id { get; set; }
        
        public int QueryId { get; set; }
        public Question Query { get; set; } = null!;

        public int PromptTemplateId { get; set; }
        public PromptTemplate PromptTemplate { get; set; } = null!;

        public int ProviderTemplateId { get; set; }
        public ProviderTemplate ProviderTemplate { get; set; } = null!;
    }
}
