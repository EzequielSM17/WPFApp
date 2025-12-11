

namespace DTOs
{
    public class AgentResponse
    {
        public string Tool { get; set; } = string.Empty;
        public object? Result { get; set; }
        public string RouterUsed { get; set; } = string.Empty;
    }
}
