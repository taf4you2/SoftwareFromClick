using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoftwareFromClick.Models
{
    // Reprezentuje główny obiekt odpowiedzi z API (OpenAI)
    public class OpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; }
    }

    // Reprezentuje pojedynczy wybór/wariant odpowiedzi zwrócony przez model
    public class Choice
    {
        [JsonPropertyName("message")]
        public Message Message { get; set; }
    }

    // Treść wiadomości zwrotnej
    public class Message
    {
        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }
}
