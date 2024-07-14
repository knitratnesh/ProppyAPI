using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TheProppyAPI.Models
{
    public class Video
    {
        [Key]
        public Guid VideoId { get; set; }
        public Guid AgentId { get; set; }
        
        [Column(TypeName = "varchar(45)")]
        public string? ApartmentNo { get; set; }
        
        [Column(TypeName = "varchar(6)")]
        public string? DealType { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public int NoOfBedRooms { get; set; }

        [Required]
        public int NoOfBathRooms { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? PropertyLatitude { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? PropertyLongitude { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? LocationLatitude { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? LocationLongitude { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string? Address { get; set; }

        [Column(TypeName = "varchar(45)")]
        public string? Location { get; set; }

        [Column(TypeName = "varchar(45)")]
        public string? City { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? Country { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }

        public bool? HideMap { get; set; }

        public bool? NoFee { get; set; }

        [Column(TypeName = "varchar(15)")]
        public string? VideoName { get; set; }
        [NotMapped]
        public string? VideoSrc { get; set; }
        [NotMapped]
        public IFormFile? VideoFile { get; set; }
    }
}
