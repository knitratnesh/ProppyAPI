using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheProppyAPI.Entities
{
    public class Chat
    {
        [Key]
        public Guid ChatId { get; set; }
        public Guid VideoId { get; set; }
        public Guid AgentId { get; set; }// to restrict the agent act as user
        public Guid UserId { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set;}
        public bool? IsActive { get; set; }
        public bool? ReadStatus { get; set; }
    }
    public class ChatMessage
    {
        [Key]
        public Guid ChatMessageId { get; set; }
        public Guid ChatId { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string Sender { get; set; }
        public Guid UserId { get; set; }
        public Guid AgentId { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class ChatMessageDTO
    {
        [Key]
        public Guid ChatMessageId { get; set; }
        public Guid ChatId { get; set; }
        public Guid VideoId { get; set; }
        public string Sender { get; set; }
        public Guid UserId { get; set; }
        public Guid AgentId { get; set; }
        [Column(TypeName = "varchar(200)")]
        public string? Message { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
