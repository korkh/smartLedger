namespace API.DTOs
{
    public class OllamaChatRequest
    {
        public string model { get; set; } = "llama3.2:3b";
        public List<OllamaMessage> messages { get; set; }
        public bool stream { get; set; } = false;
        public Dictionary<string, object> options { get; set; } = new() { { "temperature", 0.1 } };
    }

    public class OllamaMessage
    {
        public string role { get; set; } // "system" или "user"
        public string content { get; set; }
    }
}
