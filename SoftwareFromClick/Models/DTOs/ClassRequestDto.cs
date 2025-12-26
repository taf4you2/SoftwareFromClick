using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public class ClassRequestDto
    {
        public string ClassName { get; set; } = string.Empty;
        public string AccessModifier { get; set; } = string.Empty;
        public string Properties { get; set; } = string.Empty;
        public string Methods { get; set; } = string.Empty;

        public AiModel SelectedModel { get; set; } = null;
        public Language SelectedLaunguage { get; set; } = null;
    }
}
