using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net;
using TheProppyAPI.Configuration;
using TheProppyAPI.Entities;
using TheProppyAPI.Helpers;
using TheProppyAPI.Models;
using static Amazon.S3.Util.S3EventNotification;

namespace TheProppyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatMessageController : ControllerBase
    {
        private readonly RepositoryContext _context;
        private readonly IUriService uriService;
        private readonly Pagination _pagination;
        public ChatMessageController(RepositoryContext context, IUriService uriService)
        {
            _context = context;
            _pagination = new Pagination();
            this.uriService = uriService;
        }
        [HttpGet("GetChatData")]
        public IActionResult GetChatData(Guid ChatId, [FromQuery] PaginationFilter filter)
        {
            try
            {
                var data = (from cm in _context.ChatMessages
                            join c in _context.Chats on cm.ChatId equals c.ChatId
                            join u in _context.Users on c.UserId.ToString() equals u.Id
                            select new
                            {
                                cm.ChatMessageId,
                                cm.ChatId,
                                cm.Sender,
                                cm.UserId,
                                cm.Message,
                                cm.CreatedDate,
                                c.VideoId,
                                c.IsActive,
                                u.Name
                            }).Where(c => c.ChatId == ChatId).OrderBy(c => c.CreatedDate).ToList();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpGet("GetChatByUser")]
        public IActionResult GetChatByUser(Guid VideoId, Guid UserId, [FromQuery] PaginationFilter filter)
        {
            try
            {
                var data = (from cm in _context.ChatMessages
                            join c in _context.Chats on cm.ChatId equals c.ChatId
                            join u in _context.Users on c.UserId.ToString() equals u.Id
                            join v in _context.Videos on c.VideoId equals v.VideoId
                            select new
                            {
                                cm.ChatMessageId,
                                cm.ChatId,
                                cm.Sender,
                                cm.UserId,
                                cm.Message,
                                cm.CreatedDate,
                                c.VideoId,
                                c.IsActive,
                                u.Name,
                                ApartmentNo = v.ApartmentNo,
                                Bath = v.NoOfBathRooms,
                                Bed = v.NoOfBedRooms,
                                City = v.City,
                                Address = v.Address
                            }).Where(c => c.VideoId == VideoId && c.UserId == UserId).OrderByDescending(c => c.CreatedDate).ToList();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpGet("GetChatByAgent")]
        public IActionResult GetChatByAgent(Guid VideoId, Guid AgentId, [FromQuery] PaginationFilter filter)
        {
            try
            {
                var data = (from cm in _context.ChatMessages
                            join c in _context.Chats on cm.ChatId equals c.ChatId
                            join a in _context.Agents on c.AgentId equals a.AgentId
                            join u in _context.Users on a.UserId.ToString() equals u.Id
                            select new
                            {
                                cm.ChatMessageId,
                                cm.ChatId,
                                cm.Sender,
                                cm.UserId,
                                cm.Message,
                                cm.CreatedDate,
                                c.VideoId,
                                c.IsActive,
                                c.AgentId,
                                u.Name
                            }).Where(c => c.VideoId == VideoId && c.AgentId == AgentId).OrderByDescending(c => c.CreatedDate).ToList();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpGet("GetByUser")]
        public IActionResult Get(Guid UserId, [FromQuery] PaginationFilter filter)
        {
            try
            {
                var data = (from c in _context.Chats
                            join a in _context.Agents on c.AgentId equals a.AgentId
                            join u in _context.Users on a.UserId.ToString() equals u.Id
                            join v in _context.Videos on c.VideoId equals v.VideoId
                            select new
                            {
                                c.UserId,
                                c.ChatId,
                                c.UpdatedDate,
                                c.Message,
                                c.ReadStatus,
                                CustomerName = u.Name,
                                c.VideoId,
                                c.IsActive,
                                c.AgentId,
                                ApartmentNo = v.ApartmentNo,
                                Bath = v.NoOfBathRooms,
                                Bed = v.NoOfBedRooms,
                                City = v.City,
                                Address = v.Address
                            }).Where(c => c.UserId == UserId).OrderByDescending(c => c.UpdatedDate).ToList();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
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
                var data = (from c in _context.Chats
                            join u in _context.Users on c.UserId.ToString() equals u.Id
                            join v in _context.Videos on c.VideoId equals v.VideoId
                            select new
                            {
                                c.UserId,
                                c.ChatId,
                                c.UpdatedDate,
                                c.Message,
                                c.ReadStatus,
                                CustomerName = u.Name,
                                c.VideoId,
                                c.IsActive,
                                c.AgentId,
                                ApartmentNo = v.ApartmentNo,
                                Bath= v.NoOfBathRooms,
                                Bed= v.NoOfBedRooms,
                                City=v.City,
                                Address=v.Address
                            }).Where(c => c.AgentId == AgentId).OrderByDescending(c => c.UpdatedDate).ToList();
                var pagedResponse = _pagination.GetPagination(data, filter, Request.Path.Value, uriService);
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = pagedResponse });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpPost]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage([FromForm] ChatMessageDTO chatMessageDTO)
        {
            try
            {
                if (chatMessageDTO is null)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Check the Payload" });
                }
                if (!ModelState.IsValid)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Validation Error!" });
                }
                if (chatMessageDTO.UserId == Guid.Empty)
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "User is mandatory" });
                }
                if (string.IsNullOrEmpty(chatMessageDTO.Sender))
                {
                    return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = "Sender is mandatory" });
                }
                if (chatMessageDTO.ChatId == Guid.Empty)
                {
                    Chat chat = new Chat();
                    chat.ChatId = Guid.NewGuid();
                    chat.VideoId = chatMessageDTO.VideoId;
                    chat.CreatedDate = DateTime.UtcNow;
                    chat.UpdatedDate = DateTime.Now;
                    chat.UserId = chatMessageDTO.UserId;
                    chat.AgentId = chatMessageDTO.AgentId;
                    chat.IsActive = true;
                    chat.Message = chatMessageDTO.Message;
                    chat.ReadStatus = false;
                    await _context.Chats.AddAsync(chat);
                    await _context.SaveChangesAsync();

                    ChatMessage chatMessage = new ChatMessage();
                    chatMessage.ChatMessageId = Guid.NewGuid();
                    chatMessage.ChatId = chat.ChatId;
                    chatMessage.Sender = chatMessageDTO.Sender;
                    chatMessage.UserId = chatMessageDTO.UserId;
                    chatMessage.AgentId = chatMessageDTO.AgentId;
                    chatMessage.CreatedDate = DateTime.UtcNow;
                    chatMessage.Message= chatMessageDTO.Message;
                    await _context.ChatMessages.AddAsync(chatMessage);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var entity = await _context.Chats.Where(v => v.ChatId == chatMessageDTO.ChatId).AsNoTracking().FirstOrDefaultAsync();
                    if (entity != null)
                    {
                        entity.Message = chatMessageDTO.Message;
                        entity.UpdatedDate = DateTime.UtcNow;
                        entity.ReadStatus = false;
                        _context.Chats.Update(entity);
                        _context.SaveChanges();
                    }
                    ChatMessage chatMessage = new ChatMessage();
                    chatMessage.ChatMessageId = Guid.NewGuid();
                    chatMessage.ChatId = chatMessageDTO.ChatId;
                    chatMessage.Sender = chatMessageDTO.Sender;
                    chatMessage.UserId = chatMessageDTO.UserId;
                    chatMessage.AgentId = chatMessageDTO.AgentId;
                    chatMessage.CreatedDate = DateTime.UtcNow;
                    chatMessage.Message = chatMessageDTO.Message;
                    await _context.ChatMessages.AddAsync(chatMessage);
                    await _context.SaveChangesAsync();
                }
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Record Created!" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpPost("ReadStatus")]
        public async Task<IActionResult> UpdateReadStatus(Guid UserId)
        {
            try
            {                
                var entity = await _context.Chats.Where(v => v.UserId == UserId).AsNoTracking().FirstOrDefaultAsync();
                if (entity is null)
                {
                    return NotFound();
                }
                entity.ReadStatus = true;
                entity.UpdatedDate = DateTime.UtcNow;
                _context.Chats.Update(entity);
                _context.SaveChanges();
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Record Updated!" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
        [HttpPost("TempChatDelete")]
        public async Task<IActionResult> TempDelete()
        {
            try
            {
                var entity = await _context.Chats.ToListAsync();
                for (int i = 0; i < entity.Count; i++)
                {
                    _context.Chats.Remove(entity[i]);
                    await _context.SaveChangesAsync();
                }
                var centity = await _context.ChatMessages.ToListAsync();
                for (int i = 0; i < centity.Count; i++)
                {
                    _context.ChatMessages.Remove(centity[i]);
                    await _context.SaveChangesAsync();
                }
                return Ok(new { StatusCode = HttpStatusCode.OK, Data = "Record Updated!" });
            }
            catch (Exception ex)
            {
                return Ok(new { StatusCode = HttpStatusCode.InternalServerError, Data = ex.Message });
            }
        }
    }
}
