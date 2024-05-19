using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace chatbot_backend.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Role { get; set; }
        public string Content { get; set; }
        public int? ThreadId {  get; set; }
        [ForeignKey("ThreadId")]

        [ValidateNever]
        public Models.Thread Thread { get; set; }

    }
}

