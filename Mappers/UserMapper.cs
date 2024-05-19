using chatbot_backend.DTOs;
using chatbot_backend.Models;

namespace chatbot_backend.Mappers
{
    public static class UserMapper
    {
        public static UserResponse ToUserResponse(this ApplicationUser user)
        {
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                
            };
        }
    }
}
