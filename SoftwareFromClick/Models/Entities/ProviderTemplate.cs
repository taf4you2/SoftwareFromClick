using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    /*
     
    Table templateProvider {
        id int [primary key]
        idProvider int [not null]
        jsonFilePath varchar
}
     */

    public class ProviderTemplate
    {
        public int Id { get; set; }
        public string JsonFilePath { get; set; } = string.Empty;

        // Klucz obcy do Provider
        public int ProviderId { get; set; }
        public Provider Provider { get; set; } = null!;

        // Relacja: ProviderTemplate jest używany w wielu PromptTemplatesUsed
        public ICollection<PromptTemplateUsed> PromptTemplatesUsed { get; set; } = new List<PromptTemplateUsed>();
    }

}
