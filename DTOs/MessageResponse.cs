using System.ComponentModel.DataAnnotations.Schema;

namespace chatbot_backend.DTOs
{
    public class MessageResponse
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public int? ThreadId { get; set; }
    }
}
