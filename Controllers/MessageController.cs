using System.Security.Claims;
using chatbot_backend.Data;
using chatbot_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace chatbot_backend.Controllers
{
    [Route("api/message/")]
    [ApiController]
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly OpenAIHelper openAIHelper;

        public MessageController(
            ApplicationDbContext db, 
            UserManager<ApplicationUser> userManager, 
            SignInManager<ApplicationUser> signInManager,
            OpenAIHelper openAIHelper)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.openAIHelper = openAIHelper;
        }

        public void SaveMessageTODb(Message message)
        {
            db.Messages.Add(message);
        }

        [HttpGet("current-user")]
        public async Task<ActionResult<ApplicationUser>> GetUser()
        {
            try
            {
                if (userManager == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "UserManager is not initialized");
                }
                if (!signInManager.IsSignedIn(User))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "User not signed in");
                }
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    // This should not happen if user is authenticated, but handle it just in case
                    return StatusCode(StatusCodes.Status500InternalServerError, "UserId not found");
                }
                var currentUser = await userManager.FindByIdAsync(userId); ;
                if(currentUser == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "User not found");
                }
                var startDate = DateTime.Today.AddDays(-7);
                var messages = await db.Messages.Where(m => m.UserId == userId && m.Date.Date >= startDate.Date).ToListAsync();
                currentUser.Messages = messages;
                return Ok(JsonConvert.SerializeObject(currentUser));
            
            }
            catch(Exception e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
           
        }

        [HttpGet("create-message")]
        public async Task<IActionResult> PostMessage()
        {
            
            Message message = new Message
            {
                Role = "assistent",
                Content = "proba",
            };
            

            db.Messages.Add(message);
            await db.SaveChangesAsync();

            return Ok(JsonConvert.SerializeObject(message));
           
        }

        [HttpPost("process-messages")]
        public async Task<ActionResult<List<Message>>> ProcessMessagesToChatGPT([FromBody] List<Message> messages)
        {
            try
            {
                db.Messages.Add(messages.Last());

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Message message = await
                    openAIHelper.ProcessMessagesToChatGPT(messages, userId);

                db.Messages.Add(message);
                await db.SaveChangesAsync();

                messages.Add(message);
                return Ok(JsonConvert.SerializeObject(messages));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
