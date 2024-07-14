using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using TheProppyAPI.Configuration;
using TheProppyAPI.Helpers;
using TheProppyAPI.Models;

namespace TheProppyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly RepositoryContext _context;
        private readonly IUriService uriService;
        private readonly Pagination _pagination;
        private readonly CommonFilter _commonFilter;
        AWs3Services _aWs3Services;
        string S3URL = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AWSFolders")["Url"];
        string BucketName = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("AWS")["BucketName"];
        public VideoController(RepositoryContext context, IUriService uriService)
        {
            _context = context;
            _pagination = new Pagination();
            _commonFilter= new CommonFilter();
            this.uriService = uriService;
            _aWs3Services = new AWs3Services();
        }
        [HttpGet]
        public IActionResult Get([FromQuery] PaginationFilter filter)
        {
            try
            {
                //var d = _context.Videos.ToList();
                var data = (from v in _context.Videos
                            join a in _context.Agents on v.AgentId equals a.AgentId
                            join u in _context.Users on a.UserId.ToString() equals u.Id
                            select new
                            {
                                a.AgentId,
                                u.Name,
                                a.CreatedDate,
                                v.Address,
                                v.VideoId,
                                v.VideoName,
                                v.NoOfBathRooms,
                                v.NoOfBedRooms,
                                v.Location,
                                v.City,
                                v.Country,
                                v.LocationLatitude,
                                v.LocationLongitude,
                                v.PropertyLatitude,
                                v.PropertyLongitude,
                                v.Price,
                                v.ApartmentNo,
                                v.IsActive,
                                AgentStatus = u.IsActive,
                                v.HideMap,
                                v.DealType,
                                v.StartDate, v.EndDate,v.NoFee,
                                VideoUrl = "https://" + BucketName + S3URL + "/" + v.VideoName,
                            }).ToList();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpPost("GetActive")]
        public IActionResult GetActiveVideos([FromQuery] PaginationFilter filter, CommonFilter? commonFilter)
        {
            try
            {
                var data = (from v in _context.Videos
                            join a in _context.Agents on v.AgentId equals a.AgentId
                            join u in _context.Users on a.UserId.ToString() equals u.Id
                            select new
                            {
                                a.AgentId,
                                u.Name,
                                a.CreatedDate,
                                v.VideoId,
                                v.Address,
                                v.City,
                                v.Country,
                                v.VideoName,
                                v.NoOfBathRooms,
                                v.NoOfBedRooms,
                                v.Location,
                                v.LocationLatitude,
                                v.LocationLongitude,
                                v.PropertyLatitude,
                                v.PropertyLongitude,
                                v.Price,
                                v.ApartmentNo,
                                v.IsActive,
                                AgentStatus = u.IsActive,
                                v.HideMap,
                                v.DealType,
                                v.StartDate,
                                v.EndDate,
                                v.NoFee,
                                VideoUrl = "https://" + BucketName + S3URL + "/" + v.VideoName,
                            }).Where(v => v.IsActive == true).ToList();
                
                data = data.Where(d => d.StartDate?.Date <= DateTime.UtcNow.Date).ToList();
                data = data.Where(d => d.EndDate?.Date >= DateTime.UtcNow.Date).ToList();

                if (commonFilter != null)
                {
                    if (commonFilter.DealType != null)
                    {
                        data = data.Where(d => d.DealType == commonFilter.DealType).ToList();
                    }
                    if (commonFilter.Nofee != null)
                    {
                        data = data.Where(d => d.NoFee == commonFilter.Nofee).ToList();
                    }
                    if (commonFilter.NoOfBeds != null)
                    {
                        if (commonFilter.NoOfBeds.Length > 0)
                        {
                            data = data.Where(d => commonFilter.NoOfBeds.Contains(d.NoOfBedRooms)).ToList();
                        }
                    }
                    if (commonFilter.NoOfBaths != null)
                    {
                        if (commonFilter.NoOfBaths.Length > 0)
                        {
                            data = data.Where(d => commonFilter.NoOfBaths.Contains(d.NoOfBathRooms)).ToList();
                        }
                    }
                    if (commonFilter.Location != null)
                    {
                        if (commonFilter.Location.Length > 0)
                        {
                            data = data.Where(d => commonFilter.Location.Contains(d.Location)).ToList();
                        }
                    }
                    if (commonFilter.City != null)
                    {
                        if (commonFilter.City.Length > 0)
                        {
                            data = data.Where(d => commonFilter.City.Contains(d.City)).ToList();
                        }
                    }
                    if (commonFilter.Price !=null)
                    {
                        if (commonFilter.Price.MinValue != null)
                        {
                            data = data.Where(d => d.Price >= commonFilter.Price.MinValue).ToList();
                        }
                        if (commonFilter.Price.MaxValue != null)
                        {
                            data = data.Where(d => d.Price <= commonFilter.Price.MaxValue).ToList();
                        }
                    }
                }
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpGet("GetVideoById")]
        public IActionResult GetVideoById(Guid VideoId)
        {
            try
            {
                var data = (from v in _context.Videos
                            join a in _context.Agents on v.AgentId equals a.AgentId
                            join u in _context.Users on a.UserId.ToString() equals u.Id
                            select new
                            {
                                a.AgentId,
                                u.Name,
                                a.CreatedDate,
                                v.VideoId,
                                v.Address,
                                v.City,
                                v.Country,
                                v.VideoName,
                                v.NoOfBathRooms,
                                v.NoOfBedRooms,
                                v.Location,
                                v.LocationLatitude,
                                v.LocationLongitude,
                                v.PropertyLatitude,
                                v.PropertyLongitude,
                                v.Price,
                                v.ApartmentNo,
                                v.IsActive,
                                AgentStatus = u.IsActive,
                                v.HideMap,
                                v.DealType,
                                v.StartDate,
                                v.EndDate,
                                v.NoFee,
                                VideoUrl = "https://" + BucketName + S3URL + "/" + v.VideoName,
                            }).Where(v => v.IsActive == true && v.VideoId == VideoId).ToList();
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpGet("GetByAgent")]
        public IActionResult GetByAgent(Guid AgentId, [FromQuery] PaginationFilter filter)
        {
            try
            {
                var data = (from v in _context.Videos
                            join a in _context.Agents on v.AgentId equals a.AgentId
                            join u in _context.Users on a.UserId.ToString() equals u.Id
                            select new
                            {
                                a.AgentId,
                                u.Name,
                                a.CreatedDate,
                                v.VideoId,
                                v.VideoName,
                                v.Address,
                                v.NoOfBathRooms,
                                v.NoOfBedRooms,
                                v.Location,
                                v.City,
                                v.Country,
                                v.LocationLatitude,
                                v.LocationLongitude,
                                v.PropertyLatitude,
                                v.PropertyLongitude,
                                v.Price,
                                v.ApartmentNo,
                                v.IsActive,
                                AgentStatus = u.IsActive,
                                v.HideMap,
                                v.DealType,
                                v.StartDate,
                                v.EndDate,
                                v.NoFee,
                                VideoUrl = "https://" + BucketName + S3URL + "/" + v.VideoName,
                            }).Where(v => v.AgentId == AgentId).ToList();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpGet("GetLocations")]
        public IActionResult GetVideoLocations()
        {
            try
            {
                var data = (from dbo in _context.Videos where dbo.IsActive == true select dbo.Location).Distinct();
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = data });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadVideo([FromForm] Video video)
        {
            try
            {
                if (video is null)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Check the Payload!" });
                }
                if (!ModelState.IsValid)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Validation Error!" });
                }
                video.VideoId = Guid.NewGuid();
                video.CreatedDate = DateTime.UtcNow;
                video.UpdatedDate = DateTime.UtcNow;
                if (video.VideoFile != null)
                {
                    string VideoExtension = Path.GetExtension(video.VideoFile.FileName).ToLower();
                    string VideoName = DateTime.UtcNow.ToString("yymmssfff") + VideoExtension;
                    video.VideoName = VideoName;
                    await _aWs3Services.UploadFileInS3(video.VideoFile, VideoName);
                }
                _context.Videos.Add(video);
                await _context.SaveChangesAsync();
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Video Uploaded!" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpPut]
        public async Task<IActionResult> UpdateVideo([FromForm] Video video)
        {
            try
            {
                if (video is null)
                {
                    return BadRequest("Owner object is null");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid model object");
                }
                var entity = await _context.Videos.Where(v => v.VideoId == video.VideoId).AsNoTracking().FirstOrDefaultAsync();
                if (entity is null)
                {
                    return NotFound();
                }
                entity.ApartmentNo = video.ApartmentNo;
                entity.HideMap = video.HideMap;
                entity.Address = video.Address;
                entity.NoOfBedRooms = video.NoOfBedRooms;
                entity.NoOfBathRooms = video.NoOfBathRooms;
                entity.Price = video.Price;
                entity.AgentId = video.AgentId;
                entity.City = video.City;
                entity.Country = video.Country;
                entity.IsActive = video.IsActive;
                entity.PropertyLatitude = video.PropertyLatitude;
                entity.PropertyLongitude = video.PropertyLongitude;
                entity.Location = video.Location;
                entity.LocationLatitude = video.LocationLatitude;
                entity.LocationLongitude = video.LocationLongitude;
                entity.DealType = video.DealType;
                entity.StartDate = video.StartDate;
                entity.EndDate = video.EndDate;
                entity.NoFee = video.NoFee;
                if (video.VideoFile != null)
                {
                    if (entity.VideoName != video.VideoName)
                    {
                        if (string.IsNullOrEmpty(video.VideoName))
                        {
                            string VideoExtension = Path.GetExtension(video.VideoFile.FileName).ToLower();
                            string VideoName = DateTime.UtcNow.ToString("yymmssfff") + VideoExtension;
                            video.VideoName = VideoName;
                        }
                        await _aWs3Services.UploadFileInS3(video.VideoFile, video.VideoName);
                    }
                }
                _context.Videos.Update(entity);
                _context.SaveChanges();
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Record Updated!" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideo(Guid id)
        {
            try
            {
                var Entity = await _context.Videos.Where(v => v.VideoId == id).AsNoTracking().FirstOrDefaultAsync();
                if (Entity is null)
                {
                    return NotFound();
                }
                _context.Videos.Remove(Entity);
                await _context.SaveChangesAsync();
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Record Deleted!" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
    }
}


