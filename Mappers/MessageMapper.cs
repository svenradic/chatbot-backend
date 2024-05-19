using chatbot_backend.DTOs;
using chatbot_backend.Models;

namespace chatbot_backend.Mappers
{
    public static class MessageMapper
    {
        public static MessageResponse ToMessageResponse(this Message message)
        {
            return new MessageResponse
            {
                Role = message.Role,
                Content = message.Content,
                ThreadId = message.ThreadId,
            };
        }
    }
}
