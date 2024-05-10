using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace chatbot_backend.Models
{
	public class ApplicationUser: IdentityUser
	{
        [Required]
        public string FirstName { get; set; }
        [Required] 
        public string LastName { get;  set; }

        public ICollection<Message> Messages { get; set; }
    }
}
