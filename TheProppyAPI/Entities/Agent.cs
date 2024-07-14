using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheProppyAPI.Models
{
    public class Agent
    {
        [Key]
        public Guid AgentId { get; set; }
        public Guid UserId { get; set; }        

        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? LicenseName { get; set; }
        [NotMapped]
        public string? LicenseSrc { get; set; }
        [NotMapped]
        public IFormFile? LicenseFile { get; set; }
    }
    public class AgentDTO
    {
        [Key]
        public Guid AgentId { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Location { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; }
        public string? LicenseName { get; set; }
        public string? LicenseSrc { get; set; }
        public IFormFile? LicenseFile { get; set; }
    }
}
