using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using TheProppyAPI.Configuration;
using TheProppyAPI.Entities;
using TheProppyAPI.Helpers;
using TheProppyAPI.Models;

namespace TheProppyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly RepositoryContext _context;
        private readonly IUriService uriService;
        private readonly Pagination _pagination;
        private readonly UserManager<User> _userManager;
        AWs3Services aWs3Services = new AWs3Services();
        string S3URL = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AWSFolders")["Url"];
        string BucketName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AWS")["BucketName"];
        public AgentController(RepositoryContext context, IUriService uriService, UserManager<User> userManager)
        {
            _context = context;
            _pagination = new Pagination();
            this.uriService = uriService;
            _userManager = userManager;
        }
        //[Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationFilter filter)
        {
            try
            {
                var data = await (from a in _context.Agents
                                  join u in _context.Users on a.UserId.ToString() equals u.Id
                                  select new AgentDTO
                                  {
                                      AgentId = a.AgentId,
                                      Name = u.Name,
                                      UserId = a.UserId,
                                      CreatedDate = a.CreatedDate,
                                      LicenseSrc = "https://" + BucketName + S3URL + "/" + a.LicenseName,
                                      Email = u.Email,
                                      PhoneNumber = u.PhoneNumber,
                                      IsActive = u.IsActive
                                  }).ToListAsync();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        //[Authorize(Roles = UserRoles.Agent)]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var data = await (from a in _context.Agents
                                  join u in _context.Users on a.UserId.ToString() equals u.Id
                                  select new AgentDTO
                                  {
                                      AgentId = a.AgentId,
                                      Name = u.Name,
                                      UserId = a.UserId,
                                      CreatedDate = a.CreatedDate,
                                      LicenseSrc = "https://" + BucketName + S3URL + "/" + a.LicenseName,
                                      Email = u.Email,
                                      PhoneNumber = u.PhoneNumber,
                                      IsActive = u.IsActive,
                                      Country = u.Country,
                                      City = u.City,
                                      Location = u.Location,
                                      Address = u.Address,
                                  }).Where(v => v.AgentId == id).FirstOrDefaultAsync();
                if (data is null)
                {
                    return Ok(new { StatusCode = HttpStatusCode.NotFound, Data = "No Records" });
                }
                else
                {
                    return Ok(new { StatusCode = HttpStatusCode.OK, Data = data });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        //[Authorize(Roles = UserRoles.Agent)]
        [HttpPut]
        public async Task<IActionResult> UpdateAgentProfile([FromForm] AgentDTO agentDTO)
        {
            try
            {
                if (agentDTO is null)
                {
                    return BadRequest("Owner object is null");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid model object");
                }
                //Agent
                var agent = await _context.Agents.Where(a => a.AgentId == agentDTO.AgentId).AsNoTracking().FirstOrDefaultAsync();                
                if (agent != null)
                {
                    agent.UpdatedDate = DateTime.UtcNow;
                    if (agentDTO.LicenseFile != null)
                    {
                        if (string.IsNullOrEmpty(agent.LicenseName))
                        {
                            string ImageExtension = Path.GetExtension(agentDTO.LicenseFile.FileName).ToLower();
                            string ImageName = DateTime.Now.ToString("yymmssfff") + ImageExtension;
                            agent.LicenseName = ImageName;
                        }
                        await aWs3Services.UploadFileInS3(agentDTO.LicenseFile, agent.LicenseName);
                    }
                    _context.Agents.Update(agent);
                    _context.SaveChanges();
                }
                else
                {
                    return Ok(new { StatusCode = HttpStatusCode.NotFound, Data = "Agent profile not found" });
                }

                var user = await _context.Users.Where(v => v.Id == agentDTO.UserId.ToString()).AsNoTracking().FirstOrDefaultAsync();
                if (user != null)
                {
                    user.Email = agentDTO.Email;
                    user.PhoneNumber = agentDTO.PhoneNumber;
                    user.Location = agentDTO.Location;
                    user.City = agentDTO.City;
                    user.Country = agentDTO.Country;
                    user.Name = agentDTO.Name;
                    user.IsActive = agentDTO.IsActive;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                }
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Agent profile updated successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        //[Authorize(Roles = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgent(Guid id)
        {
            try
            {
                var Entity = await _context.Agents.Where(v => v.AgentId == id).AsNoTracking().FirstOrDefaultAsync();
                if (Entity is null)
                {
                    return NotFound();
                }
                _context.Agents.Remove(Entity);
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(Entity.UserId.ToString());
                if (user != null)
                {
                    var resetPassResult = await _userManager.DeleteAsync(user);
                }
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Record Deleted!" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
    }
}