using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheProppyAPI.Entities
{
    public class User : IdentityUser
    {
        [Column(TypeName = "varchar(45)")]
        public string? Name { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string? Address { get; set; }

        [Column(TypeName = "varchar(45)")]
        public string? Location { get; set; }

        [Column(TypeName = "varchar(45)")]
        public string? City { get; set; }

        [Column(TypeName = "varchar(25)")]
        public string? Country { get; set; }

        public bool IsActive { get; set; }
    }
    public class Login
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string UserType { get; set; }
    }
    public class GoogleResponse
    {
        public string? aud { get; set; }
        public string? azp { get; set; }
        public string? email { get; set; }
        public bool? email_verified { get; set; }
        public string? exp { get; set; }
        public string? given_name { get; set; }
        public string? iat { get; set; }
        public string? iss { get; set; }
        public string? jti { get; set; }
        public string? locale { get; set; }
        public string? name { get; set; }
        public string? nbf { get; set; }
        public string? picture { get; set; }
        public string? sub { get; set; }
        public string? usertype { get; set; }
    }
    public class LocationData
    {
        public Guid LocationId { get; set; }
        public string? PropertyLatitude { get; set; }
        public string? PropertyLongitude { get; set; }
        public string? LocationLatitude { get; set; }
        public string? LocationLongitude { get; set; }
        public string? Address { get; set; }
        public string? Location { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
        public string? UserId { get; set; }
        public Guid? AgentId { get; set; }
        public string? Role { get; set; }
        public string? Name { get; set; }
        public bool Status { get; set; }
        public string? Message { get; set; }
        public int MessageCount { get; set; }
    }
    public class ChangePassword
    {
        public string OldPassword { get; set; }
        public string UserId { get; set; }
        public string NewPassword { get; set; }
    }
    public class UserRegister
    {
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class AgentRegister
    {
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? LicenseName { get; set; }
        public string? LicenseSrc { get; set; }
        public IFormFile? LicenseFile { get; set; }
    }
    public class Profile
    {
        public Guid UserId { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string? Name { get; set; }
        public string? Email { get; set; }        
        public string? PhoneNumber { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
