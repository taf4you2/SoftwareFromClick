using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    // klasa potrzebna do przesyłania danych z functionView dalej do llm
    public class FunctionRequestDto
    {
        public string FunctionName { get; set; } = string.Empty;
        public string FunctionType { get; set; } = "public";
        public string Functionalities { get; set; } = string.Empty;
        public string InputParameters { get; set; } = string.Empty;
        public string ReturnType { get; set; } = "void";


        public AiModel SelectedModel { get; set; } = null!;
        public Language SelectedLanguage { get; set; } = null!;
    }
}
