using System.Security.Claims;
using chatbot_backend.Data;
using chatbot_backend.DTOs;
using chatbot_backend.Mappers;
using chatbot_backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Thread = chatbot_backend.Models.Thread;

namespace chatbot_backend.Controllers
{
    [Route("api/message/")]
    [ApiController]
    [EnableCors("AllowAllOrigins")]
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
                var user = currentUser.ToUserResponse();
                user.Threads = new List<ThreadResponse>();
                var threads = db.Threads.Where(t => t.UserId == userId).Select(t => t.ToThreadResponse()).ToList();
                if(threads.Count > 0)
                {
                    user.Threads = threads;
                    foreach (var thread in user.Threads)
                    {
                        var messages = db.Messages.Where(m => m.ThreadId == thread.Id).Select(m => m.ToMessageResponse()).ToList();
                        thread.Messages = messages;
                    }
                }

                
                return Ok(user);
            
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

        [HttpPost("create-thread")]
        public async Task<IActionResult> CreateThread()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userId == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "User not signed in");
            }

            Thread newThread = new Thread
            {
                Name = "New thread",
                UserId = userId,
                
            };

            db.Threads.Add(newThread);
            await db.SaveChangesAsync();
            var threadDTO = newThread.ToThreadResponse();
            threadDTO.Messages = new List<MessageResponse>();
            return Ok(threadDTO);
        }

        [HttpPost("process-messages/{threadId?}")]
        public async Task<ActionResult<List<Message>>> ProcessMessagesToChatGPT([FromBody] List<Message> messages, [FromRoute] int? threadId)
        {
            try
            {
                if (!signInManager.IsSignedIn(User))
                {
                    db.Messages.Add(messages.Last());
                    await db.SaveChangesAsync();
                    if(threadId < 0)
                    {
                        threadId = null;
                    }

                    Message message = await
                        openAIHelper.ProcessMessagesToChatGPT(messages, threadId);

                    db.Messages.Add(message);
                    await db.SaveChangesAsync();

                    messages.Add(message);

                    var messagesDTO = messages.Select(s => s.ToMessageResponse()).ToList();
                    return Ok(messagesDTO);
                }
                else
                {
                    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    var thread = db.Threads.Find(threadId);

                    if (thread == null)
                    {
                        return NotFound();
                    }

                    if (!(userId == thread.UserId))
                    {
                        return BadRequest();
                    }

                    var messagesToProcess = db.Messages.Where(m => m.ThreadId == thread.Id).ToList();

                    messagesToProcess.Add(messages.Last());
                    db.Messages.Add(messages.Last());
                    await db.SaveChangesAsync();

                    Message message = await
                       openAIHelper.ProcessMessagesToChatGPT(messagesToProcess, threadId);
                    db.Messages.Add(message);

                    await db.SaveChangesAsync();
                    messages.Add(message);

                    var messagesDTO = messages.Select(s => s.ToMessageResponse()).ToList();
                    return Ok(messagesDTO);
                }

               
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
