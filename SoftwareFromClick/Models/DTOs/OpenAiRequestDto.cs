using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SoftwareFromClick.Models
{
    // DTO reprezentujące strukturę żądania wysyłanego do API
    public class OpenAiRequestDto
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-4o-mini";

        [JsonPropertyName("messages")]
        public List<MessageDto> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.2;

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; set; }
    }

    // Klasa wiadomość w konwersacji z AI
    public class MessageDto
    {
        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }
    }
}
