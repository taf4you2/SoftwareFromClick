using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SoftwareFromClick.Models
{
    public class GeneratorTemplateDto
    {
        // Klasa odwzorowująca strukturę pliku JSON z szablonem promptu
        public string System { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        
    }
}
