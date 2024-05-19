using chatbot_backend.DTOs;
using chatbot_backend.Models;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Claims;
using System.Text;


namespace chatbot_backend.Controllers
{
    public class OpenAIHelper
    {
        private readonly string apiKey;

        public OpenAIHelper()
        {
            
        }
        public async Task<Message> ProcessMessagesToChatGPT(List<Message> messages, int? threadId)
        {
            try
            {
                var apiMessages = messages.Select(message =>
                {
                    return new
                    {
                        role = message.Role == "user" ? "user" : "assistant",
                        content = message.Content
                    };
                }).ToList();

                var systemMessage = new
                {
                    role = "system",
                    content = "Speak like a cool 20 year old student"
                };

                var apiRequestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[] { systemMessage }.Concat(apiMessages)
                };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                    var jsonRequestBody = JsonConvert.SerializeObject(apiRequestBody);
                    var stringContent = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", stringContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Failed to get response from OpenAI API. Status code: {response.StatusCode}");
                    }

                    var responseData = await response.Content.ReadAsStringAsync();
                    dynamic data = JsonConvert.DeserializeObject(responseData);

                    Message message = new Message();
                    if(threadId != null)
                    {
                        message = new Message
                        {
                            Role = "assistent",
                            Content = data.choices[0].message.content,
                            ThreadId = threadId
                        };
                    }
                    else
                    {
                        message = new Message
                        {
                            Role = "assistent",
                            Content = data.choices[0].message.content,
                        };
                    }

                    
                    return message;
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                throw;
            }
        }
    }
}
