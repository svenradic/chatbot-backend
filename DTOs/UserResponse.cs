using Thread = chatbot_backend.Models.Thread;

namespace chatbot_backend.DTOs
{
    public class UserResponse
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public ICollection<ThreadResponse> Threads { get; set; }

    }
}
