using chatbot_backend.Models;

namespace chatbot_backend.DTOs
{
    public class ThreadResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } 
        public ICollection<MessageResponse> Messages { get; set; }
    }
}
