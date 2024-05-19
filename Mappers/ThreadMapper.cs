using chatbot_backend.DTOs;

namespace chatbot_backend.Mappers
{
    public static class ThreadMapper
    {
        public static ThreadResponse ToThreadResponse(this Models.Thread thread)
        {
            return new ThreadResponse
            {
                Id = thread.Id,
                Name = thread.Name,
                CreatedAt = thread.CreatedAt,
              
            };
        }
    }
}
