using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using TheProppyAPI.Entities;
using TheProppyAPI.Helpers;
using TheProppyAPI.Models;

namespace TheProppyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class homeController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private SignInManager<User> signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RepositoryContext _context;
        private readonly IConfiguration _configuration;
        AWs3Services _aWs3Services;
        public homeController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager, RepositoryContext context, IConfiguration configuration, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _configuration = configuration;
            _aWs3Services = new AWs3Services();
            this.signInManager = signInManager;
        }
        [HttpGet]
        [Route("location")]
        public async Task<IActionResult> SearchLocation(string input)
        {
            List<LocationData> locationList = new List<LocationData>();
            try
            {                
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://maps.googleapis.com/maps/api/place/autocomplete/json?input=" + input + "&country=us&language=EN&types=geocode&language=fr&key=AIzaSyDWVA34PTvtvFaM6D4RT8HeG9Ykq6BZV0M"))
                    {
                        var response = await httpClient.SendAsync(request);
                        var data = response.Content.ReadAsStringAsync().Result;
                        Root root = JsonConvert.DeserializeObject<Root>(data);

                        if (root != null)
                        {
                            if (root.predictions != null)
                            {
                                for (int i = 0; i < root.predictions.Count; i++)
                                {
                                    LocationData locationData = new LocationData();
                                    locationData.LocationId = Guid.NewGuid();
                                    locationData.Address = root.predictions[i].description;
                                    if (root.predictions[i].terms != null)
                                    {
                                        if (root.predictions[i].terms.Count > 0)
                                        {
                                            locationData.Location = root.predictions[i].terms[0].value;
                                        }
                                        if (root.predictions[i].terms.Count > 1)
                                        {
                                            locationData.City = root.predictions[i].terms[1].value;
                                        }
                                        if (root.predictions[i].terms.Count > 2)
                                        {
                                            locationData.Country = root.predictions[i].terms[3].value;
                                        }
                                    }
                                    locationData = await GetLatLonValues(locationData);
                                    locationList.Add(locationData);
                                }
                            }
                        }
                    }
                }
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = locationList });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = locationList });
            }
        }
        private async Task<LocationData> GetLatLonValues(LocationData locationData)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://maps.googleapis.com/maps/api/geocode/json?address="+locationData.Address+"&sensor=false&key=AIzaSyDWVA34PTvtvFaM6D4RT8HeG9Ykq6BZV0M"))
                {
                    var response = await httpClient.SendAsync(request);
                    var data = response.Content.ReadAsStringAsync().Result;
                    LatRoot root = JsonConvert.DeserializeObject<LatRoot>(data);

                    if (root != null)
                    {
                        if (root.results != null)
                        {
                            for (int i = 0; i < root.results.Count; i++)
                            {
                                if (i == 0)
                                {
                                    var address = root.results[i].address_components;
                                    if (address != null)
                                    {
                                        for (int j = address.Count - 1; j >= 0; j--)
                                        {
                                            if (j == address.Count - 1)
                                            {
                                                locationData.Country = address[j].long_name;
                                            }
                                            if (j == address.Count - 2)
                                            {
                                                locationData.State = address[j].long_name;
                                            }
                                            if (j == address.Count - 3)
                                            {
                                                locationData.City = address[j].long_name;
                                            }
                                            //if (address[0].types != null)
                                            //{
                                            //    for (int k = 0; k < address[0].types.Count; k++)
                                            //    {
                                            //        if (address[0].types[k] == "political")
                                            //        {
                                            //            locationData.Location = address[j].long_name;
                                            //        }
                                            //        if (address[0].types[k] == "administrative_area_level_1")
                                            //        {
                                            //            locationData.City = address[j].long_name;
                                            //        }
                                            //        if (address[0].types[k] == "country")
                                            //        {
                                            //            locationData.Country = address[j].long_name;
                                            //        }
                                            //    }
                                            //}
                                        }
                                    }
                                }
                                locationData.LocationLatitude = root.results[0].geometry.location.lat.ToString();
                                locationData.LocationLongitude = root.results[0].geometry.location.lng.ToString();
                                locationData.PropertyLatitude = root.results[0].geometry.location.lat.ToString();
                                locationData.PropertyLongitude = root.results[0].geometry.location.lng.ToString();
                            }
                        }
                    }
                }
            }
            return locationData;
        }
        [HttpPost]
        [Route("signin-google")]
        public async Task<IActionResult> GoogleSignIn([FromForm]GoogleResponse googleResponse)
        {
            LoginResponse loginResponse = new LoginResponse();
            var userExists = await _userManager.FindByNameAsync(googleResponse.email);
            if (userExists != null)
            {
                var userRoles = await _userManager.GetRolesAsync(userExists);
                var authClaims = new List<Claim>
                        {
                            new Claim(googleResponse.given_name, userExists.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var token = GetToken(authClaims);
                var roleName = userRoles.FirstOrDefault();

                if (roleName == UserRoles.Agent)
                {
                    var agent = await _context.Agents.Where(c => c.UserId.ToString() == userExists.Id).FirstOrDefaultAsync();
                    if (agent != null)
                    {
                        loginResponse.AgentId = agent.AgentId;
                        var chat = await _context.Chats.Where(c => c.AgentId == agent.AgentId && c.ReadStatus == false).ToListAsync();
                        if (chat != null)
                        {
                            loginResponse.MessageCount = chat.Count;
                        }
                    }
                    loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
                    loginResponse.Expiration = token.ValidTo;
                    loginResponse.UserId = userExists.Id;
                    loginResponse.Role = roleName;
                    loginResponse.Status = true;
                    loginResponse.Name = userExists.Name;
                    loginResponse.Message = "Success";
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = loginResponse });
                }
                else
                {
                    loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
                    loginResponse.Expiration = token.ValidTo;
                    loginResponse.UserId = userExists.Id;
                    loginResponse.Role = roleName;
                    loginResponse.Status = true;
                    loginResponse.Name = userExists.Name;
                    loginResponse.Message = "Success";
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = loginResponse });
                }
            }
            else
            {
                User user = new()
                {
                    Email = googleResponse.email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = googleResponse.email,
                    Name = googleResponse.given_name,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, "NoPassword");

                if (!result.Succeeded)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User creation failed! Please check user details and try again." });
                }                
                if (googleResponse.usertype == UserRoles.Agent)
                {
                    if (!await _roleManager.RoleExistsAsync(UserRoles.Agent))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Agent));
                    }
                    await _userManager.AddToRoleAsync(user, UserRoles.Agent);
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var authClaims = new List<Claim>
                        {
                            new Claim(user.Name, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };
                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }
                    var token = GetToken(authClaims);
                    var roleName = UserRoles.Agent;
                    #region Create Agent
                    Agent agent = new Agent();
                    agent.AgentId = Guid.NewGuid();
                    agent.UserId = Guid.Parse(user.Id);
                    agent.CreatedDate = DateTime.UtcNow;
                    agent.UpdatedDate = DateTime.UtcNow;
                    await _context.AddAsync(agent);
                    await _context.SaveChangesAsync();
                    #endregion
                    loginResponse.AgentId = agent.AgentId;
                    loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
                    loginResponse.Expiration = token.ValidTo;
                    loginResponse.UserId = user.Id;
                    loginResponse.Role = roleName;
                    loginResponse.Status = true;
                    loginResponse.Name = user.Name;
                    loginResponse.Message = "Success";
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = loginResponse });
                }
                else
                {
                    if (!await _roleManager.RoleExistsAsync(UserRoles.Customer))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Customer));
                    }
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var authClaims = new List<Claim>
                        {
                            new Claim(user.Name, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };
                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }
                    var token = GetToken(authClaims);
                    var roleName = UserRoles.Customer;
                    await _userManager.AddToRoleAsync(user, UserRoles.Customer);
                    loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
                    loginResponse.Expiration = token.ValidTo;
                    loginResponse.UserId = user.Id;
                    loginResponse.Role = roleName;
                    loginResponse.Status = true;
                    loginResponse.Name = user.Name;
                    loginResponse.Message = "Success";
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = loginResponse });
                }
            }
        }
        
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(Login model)
        {
            LoginResponse loginResponse = new LoginResponse();
            loginResponse.MessageCount = 0;
            if (string.IsNullOrEmpty(model.Email))
            {
                loginResponse.Status = false;
                loginResponse.Message = "Email is mandatory!";
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = loginResponse });
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                loginResponse.Status = false;
                loginResponse.Message = "Password is mandatory!";
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = loginResponse });
            }
            
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                if (user.IsActive)
                {
                    if (await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        var userRoles = await _userManager.GetRolesAsync(user);
                        var authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };
                        foreach (var userRole in userRoles)
                        {
                            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                        }
                        var token = GetToken(authClaims);
                        var roleName = userRoles.FirstOrDefault();
                        if (roleName != model.UserType)
                        {
                            loginResponse.Message = "Access Denied!";
                            loginResponse.Status = false;
                            return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = loginResponse });
                        }
                        if (roleName == UserRoles.Agent)
                        {
                            var agent = await _context.Agents.Where(c => c.UserId.ToString() == user.Id).FirstOrDefaultAsync();
                            if (agent != null)
                            {
                                loginResponse.AgentId = agent.AgentId;
                                var chat = await _context.Chats.Where(c => c.AgentId == agent.AgentId && c.ReadStatus == false).ToListAsync();
                                if (chat != null)
                                {
                                    loginResponse.MessageCount = chat.Count;
                                }
                            }
                            loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
                            loginResponse.Expiration = token.ValidTo;
                            loginResponse.UserId = user.Id;
                            loginResponse.Role = roleName;
                            loginResponse.Status = true;
                            loginResponse.Name = user.Name;
                            loginResponse.Message = "Success";

                            return Ok(new { StatusCode = HttpStatusCode.OK, Data = loginResponse });
                        }
                        else
                        {
                            loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
                            loginResponse.Expiration = token.ValidTo;
                            loginResponse.UserId = user.Id;
                            loginResponse.Role = roleName;
                            loginResponse.Status = true;
                            loginResponse.Name = user.Name;
                            loginResponse.Message = "Success";
                            return Ok(new { StatusCode = HttpStatusCode.OK, Data = loginResponse });
                        }
                    }
                    else
                    {
                        loginResponse.Message = "Please check the credentials!";
                        loginResponse.Status = false;
                        return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = loginResponse });
                    }
                }
                else
                {
                    loginResponse.Message = "Account activation pending!";
                    loginResponse.Status = false;
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = loginResponse });
                }
            }
            else if (model.UserType == UserRoles.Agent)
            {
                loginResponse.Message = "No User Exists!";
                loginResponse.Status = false;
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = loginResponse });
            }
            else
            {
                User newuser = new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    IsActive = true
                };
                var result = await _userManager.CreateAsync(newuser, model.Password);

                if (!result.Succeeded)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User creation failed! Please check user details and try again." });
                }
                if (!await _roleManager.RoleExistsAsync(UserRoles.Customer))
                {
                    await _roleManager.CreateAsync(new IdentityRole(UserRoles.Customer));
                }
                await _userManager.AddToRoleAsync(newuser, UserRoles.Customer);
                var userRoles = await _userManager.GetRolesAsync(newuser);
                var authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, newuser.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        };
                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }
                var roleName = userRoles.FirstOrDefault();
                if (roleName != model.UserType)
                {
                    loginResponse.Message = "Access Denied!";
                    loginResponse.Status = false;
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = loginResponse });
                }
                var token = GetToken(authClaims);
                loginResponse.Token = new JwtSecurityTokenHandler().WriteToken(token);
                loginResponse.Expiration = token.ValidTo;
                loginResponse.UserId = newuser.Id;
                loginResponse.Role = UserRoles.Customer;
                loginResponse.Status = true;
                loginResponse.Name = newuser.Name;
                loginResponse.Message = "Success";
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = loginResponse });
            }
        }
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromForm] UserRegister model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Email is mandatory!" });
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Password is mandatory!" });
                }

                #region user creation
                var userExists = await _userManager.FindByNameAsync(model.Email);
                if (userExists != null)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Email already exists!" });
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
                        IsActive = true
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (!result.Succeeded)
                    {
                        return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User creation failed! Please check user details and try again." });
                    }
                    if (!await _roleManager.RoleExistsAsync(UserRoles.Customer))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Customer));
                    }
                    await _userManager.AddToRoleAsync(user, UserRoles.Customer);

                }
                #endregion
                //var bemail = Task.Factory.StartNew(() => _emailSender.SendBusinessRegisterEmail(model.Email, model.BusinessName));
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Account Created Successfully!" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpPost]
        [Route("register-agent")]
        public async Task<IActionResult> RegisterAgent([FromForm] AgentRegister model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Email is mandatory!" });
                }
                if (string.IsNullOrEmpty(model.Password))
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Password is mandatory!" });
                }

                #region user creation
                var userExists = await _userManager.FindByNameAsync(model.Email);
                if (userExists != null)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Email already exists!" });
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
                        IsActive = false
                    };

                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (!result.Succeeded)
                    {
                        return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User creation failed! Please check user details and try again." });
                    }
                    if (!await _roleManager.RoleExistsAsync(UserRoles.Agent))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(UserRoles.Agent));
                    }
                    await _userManager.AddToRoleAsync(user, UserRoles.Agent);

                    #region Create Agent
                    Agent agent = new Agent();
                    agent.AgentId = Guid.NewGuid();
                    agent.UserId = Guid.Parse(user.Id);
                    agent.CreatedDate = DateTime.UtcNow;
                    agent.UpdatedDate = DateTime.UtcNow;
                    if (model.LicenseFile != null)
                    {
                        string ImageExtension = Path.GetExtension(model.LicenseFile.FileName).ToLower();
                        string ImageName = DateTime.Now.ToString("yymmssfff") + ImageExtension;
                        agent.LicenseName = ImageName;
                        await _aWs3Services.UploadFileInS3(model.LicenseFile, ImageName);
                    }
                    await _context.AddAsync(agent);
                    await _context.SaveChangesAsync();
                    #endregion
                }
                #endregion
                //var bemail = Task.Factory.StartNew(() => _emailSender.SendBusinessRegisterEmail(model.Email, model.BusinessName));
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Account Created Successfully! Your account is under review." });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            return token;
        }
    }
}
