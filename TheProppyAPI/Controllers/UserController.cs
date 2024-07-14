using Azure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using TheProppyAPI.Configuration;
using TheProppyAPI.Entities;
using TheProppyAPI.Helpers;
using TheProppyAPI.Models;

namespace TheProppyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RepositoryContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUriService uriService;
        private readonly Pagination _pagination;
        AWs3Services _aWs3Services;
        public UserController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager, RepositoryContext context, IConfiguration configuration, IUriService uriService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _aWs3Services = new AWs3Services();
            _pagination = new Pagination();
            this.uriService = uriService;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] PaginationFilter filter)
        {
            try
            {
                var data = await (from a in _context.Users
                                  join ur in _context.UserRoles on a.Id equals ur.UserId
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  select new
                                  {
                                      a.Name,
                                      a.Email,
                                      a.PhoneNumber,
                                      a.Location,
                                      a.City,
                                      a.Country,
                                      a.Address,
                                      a.Id,
                                      a.IsActive,
                                      ur.RoleId,
                                      RoleName = r.Name
                                  }).Where(r => r.RoleName == UserRoles.Customer.ToString()).ToListAsync();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpGet]
        [Route("GetById")]
        public async Task<IActionResult> GetUserById(string UserId)
        {
            try
            {
                var data = await (from a in _context.Users
                                  join ur in _context.UserRoles on a.Id equals ur.UserId
                                  join r in _context.Roles on ur.RoleId equals r.Id
                                  select new
                                  {
                                      a.Name,
                                      a.Email,
                                      a.PhoneNumber,
                                      a.Location,
                                      a.City,
                                      a.Country,
                                      a.Address,
                                      a.Id,
                                      a.IsActive,
                                      ur.RoleId,
                                      RoleName = r.Name
                                  }).Where(r => r.Id == UserId).FirstOrDefaultAsync();
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }

        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromForm] UserRegister model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Email is mandatory" });
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Password is mandatory" });
                }

                #region user creation
                var userExists = await _userManager.FindByNameAsync(model.Email);
                if (userExists != null)
                {
                    //model.Id = userExists.Id;
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Email already exists" });
                }
                else
                {
                    User user = new()
                    {
                        Email = model.Email,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        UserName = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        Name = model.Name,
                        IsActive=true,
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (!result.Succeeded)
                    {
                        return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User creation failed! Please check user details and try again." });
                    }
                    if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                    }
                    await _userManager.AddToRoleAsync(user, UserRoles.Admin);
                }
                #endregion
                //var bemail = Task.Factory.StartNew(() => _emailSender.SendBusinessRegisterEmail(model.Email, model.BusinessName));
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Account Created Successfully! Your account is under review" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpPost]
        [Route("Profile")]
        public async Task<IActionResult> Update([FromForm] Profile profile)
        {
            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
            if (user != null)
            {
                user.Name = profile.Name;
                user.Email = profile.Email;
                user.PhoneNumber = profile.PhoneNumber;
                user.Country = profile.Country;
                user.City = profile.City;
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Record updated successfully!" });
                }
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Failed to update user" });
            }
            else
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User not exists!" });
            }
        }
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            var user = await _userManager.FindByIdAsync(changePassword.UserId);
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, changePassword.OldPassword, changePassword.NewPassword);
                if (result.Succeeded)
                {
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Password changed successfully!" });
                }
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Failed to change password!" });
            }
            else
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User not exists!" });
            }
        }
        [HttpPost]
        [Route("Status")]
        public async Task<IActionResult> UserStatus(string id, bool Status)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user != null)
                {
                    user.IsActive = Status;
                    _context.Users.Update(user);
                    _context.SaveChanges();
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = "User status changed successfully!" });

                }
                else
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User not exists!" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Failed to change user status!" });
            }
        }
        [HttpDelete]
        public async Task<IActionResult> Delete(string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            if (user != null)
            {
                var resetPassResult = await _userManager.DeleteAsync(user);
                if (resetPassResult.Succeeded)
                {
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Account Deleted Successfully!" });
                }
                else
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = DisplayError(resetPassResult) });
                }
            }
            else
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User not exists!" });
            }
        }
        private string DisplayError(IdentityResult result)
        {
            List<IdentityError> errorList = result.Errors.ToList();
            var errors = string.Join(", ", errorList.Select(e => e.Description));
            return errors;
        }        
    }
}
