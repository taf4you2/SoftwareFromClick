using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftwareFromClick.Models;
using System.Text.RegularExpressions;

namespace SoftwareFromClick.Services
{
    public class TemplateParserService
    {
        // dla blocktext
        private static readonly Regex _textPattern = new Regex(@"\{\{(?<label>.*?)\}\}", RegexOptions.Compiled);
        // dla combobombo
        private static readonly Regex _choicePattern = new Regex(@"\{\((?<label>[^)]+)\)\((?<options>[^)]+)\)\}", RegexOptions.Compiled);
        // dla checkbox
        private static readonly Regex _boolPattern = new Regex(@"\{\<(?<label>.*?)\>\}", RegexOptions.Compiled);

        public List<TemplateField> ExtractFields(string systemPrompt, string userPrompt)
        {
            var fields = new List<TemplateField>();


            // System Port
            fields.AddRange(ParseText(systemPrompt));

            // User Port
            fields.AddRange(ParseText(userPrompt));

            // usuwamy duplikaty jezeli ktos wpadnie na pomysl zrobienia tego samego jezyka dwa razy powiedzmy w user i system niebędą rozroznialny
            return fields
                .GroupBy(f => f.Placeholder)
                .Select(g => g.First())
                .ToList();
        }

        public List<TemplateField> ParseText(string text)
        {
            var detectedFields = new List<TemplateField>();
            
            // szukamy tekstu w tekscie (trochę bez sensu z tego punktu widzenia ale jak to mówią "ok")
            var textMatches = _textPattern.Matches(text);

            foreach (Match match in textMatches)
            {
                detectedFields.Add(new TemplateField
                {
                    Label = match.Groups["label"].Value,
                    Placeholder = match.Value, // np "{{FunctionName}}"
                    Type = InputFieldType.Text
                });
            }

            // szukam combox w tekscie 
            var choiceMatches = _choicePattern.Matches(text);
            foreach (Match match in choiceMatches)
            {
                var label = match.Groups["label"].Value;
                var optionsString = match.Groups["options"].Value;

                // dzieli tekst przy przecinkach i usuwa spacje trimmmmmmmem
                var options = optionsString.Split(',')
                    .Select(o => o.Trim())
                    .Where(o => !string.IsNullOrEmpty(o))
                    .ToList();

                detectedFields.Add(new TemplateField
                {
                    Label = label,
                    Placeholder = match.Value, // np "{(jezyk)(python, c$)}"
                    Type = InputFieldType.Choice,
                    Options = options
                });
            }

            //szukam checkbox w tekscie
            var boolMatches = _boolPattern.Matches(text);
            foreach (Match match in boolMatches)
            {
                detectedFields.Add(new TemplateField
                {
                    Label = match.Groups["label"].Value,
                    Placeholder = match.Value, // np "{<czy komentowac>}"
                    Type = InputFieldType.Boolean
                });
            }


            return detectedFields;
        }
    }
}
