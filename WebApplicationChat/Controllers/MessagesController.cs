using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;
using WebApplicationChat;
using WebApplicationChat.Services;
using static WebApplicationChat.Controllers.ContactsController;
using Microsoft.OpenApi.Validations;
using Microsoft.AspNetCore.SignalR;
using WebApplicationChat.Hubs;

namespace WebApplicationChat.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;
        private readonly IHubContext<WebApplicationHub> _hubContext;
        private readonly HubService _hubService;

        public MessagesController(MessageService messageService, IHubContext<WebApplicationHub> hubContext, HubService hubService)
        {
            _messageService = messageService;
            _hubContext = hubContext;
            _hubService = hubService;
        }

        public class MessageObj
        {
            public string? username { get; set; }
            public string? content { get; set; }
        }


        [HttpGet("{id}/messages")]
        public async Task<ActionResult<IEnumerable<Message>>> getMessages(string username, string id)
        {
            try
            {
                var result = await _messageService.GetMessages(username, id);
                return Ok(result);
            }
            catch
            {
                return NotFound();
            }
        }

        // Get a specific message details (id is the contact Id and messageId is message Id)
        [HttpGet("{id}/messages/{messageId}")]
        public async Task<ActionResult<Message>> GetMessage(string username, string id, int messageId)
        {
            var message = await _messageService.GetMessage(id, username, messageId);
            if (message != null)
            {
                return Ok(message);
            }
            return NotFound();
        }

        // Post a new message between contact and current user (id is the contact)
        [HttpPost("{id}/messages")]
        public async Task<ActionResult<Message>> PostMessage(string id, [FromBody] MessageObj newmsgobj)
        {
            var message = await _messageService.AddMessage(id, newmsgobj.username, newmsgobj.content, true);
            if (message != null)
            {
                string? connectionID = _hubService.GetConnectionId(id);

                if (connectionID != null)
                {
                    await _hubContext.Clients.Client(connectionID).SendAsync("refresh");
                }
                return StatusCode(201);
            }
            return BadRequest();
        }

        // Update a message (id is contact Id and idMessage is message Id)
        [HttpPut("{id}/Messages/{messageId}")]
        public async Task<IActionResult> PutMessage(string id, int messageId, [FromBody] MessageObj newmsgobj)
        {
            var result = await _messageService.SetMessage(id, messageId, newmsgobj.username, newmsgobj.content);
            if (!result)
            {
               return NotFound();
            }
            return StatusCode(204);
        }

        // Delete a message (id is contact Id and idMessage is message Id)
        [HttpDelete("{id}/Messages/{messageId}")]
        public async Task<ActionResult> DeleteMessage([FromBody] string username, string id, int messageId)
        {
            var result = await _messageService.DeleteMessage(id, username, messageId);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
