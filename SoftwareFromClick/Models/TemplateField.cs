using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    public enum InputFieldType
    {
        Text,           // odpowiada za textblock
        Choice,         // odpowiada za combobox
        Boolean,        // odpowiada za checkbox
        ParameterList   // odpowiaza za parametry (trzeba to będzie ustandaryzować do normalnego użytku)
    }

    public class TemplateField
    {
        public string Placeholder { get; set; } = string.Empty; // z szablonu np: {{opis dzialania}} lub {(combo)(z,argumentami)}
        public string Label { get; set; } = string.Empty;       // opis dzialania (bez{()})
        public InputFieldType Type { get; set; }                // typ (text,combo)
        public List<string> Options { get; set; } = new();      // lista opcji (tylko) dla combox

    }

    public enum ParameterType
    {
        Int,
        Double,
        Void,
        String,
        Char
    }

    public class ParameterItem
    {
        public string Type { get; set; } = "int";
        public string Name { get; set; } = string.Empty;
    }
}
