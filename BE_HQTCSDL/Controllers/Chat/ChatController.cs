using System.Security.Claims;
using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Services.Interfaces;
using BE_HQTCSDL.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE_HQTCSDL.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/chat/conversations")]
public sealed class ChatController(IChatService chatService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var actor = GetActor();
        if (actor.Id <= 0) return Unauthorized(new { message = "Unauthorized" });

        var conversations = await chatService.GetConversationsAsync(actor.Id, actor.Role);
        return Ok(conversations);
    }

    [Authorize(Roles = AppRoles.User)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConversationRequest request)
    {
        try
        {
            var actor = GetActor();
            if (actor.Id <= 0) return Unauthorized(new { message = "Unauthorized" });

            var conversation = await chatService.CreateConversationAsync(actor.Id, request);
            return Ok(conversation);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id:long}/messages")]
    public async Task<IActionResult> GetMessages(long id) =>
        await ExecuteAsync(async actor =>
            Ok(await chatService.GetMessagesAsync(id, actor.Id, actor.Role)));

    [HttpPost("{id:long}/messages")]
    public async Task<IActionResult> SendMessage(long id, [FromBody] SendChatMessageRequest request) =>
        await ExecuteAsync(async actor =>
            Ok(await chatService.SendMessageAsync(id, actor.Id, actor.Role, request)));

    [HttpPatch("{id:long}/read")]
    public async Task<IActionResult> MarkAsRead(long id) =>
        await ExecuteAsync(async actor =>
        {
            await chatService.MarkAsReadAsync(id, actor.Id, actor.Role);
            return NoContent();
        });

    [Authorize(Roles = AppRoles.User)]
    [HttpPost("{id:long}/handoff")]
    public async Task<IActionResult> RequestHandoff(long id) =>
        await ExecuteAsync(async actor =>
            Ok(await chatService.RequestHandoffAsync(id, actor.Id)));

    [Authorize(Roles = AppRoles.AdminOrOrderManager)]
    [HttpPatch("{id:long}/assign")]
    public async Task<IActionResult> Assign(long id, [FromBody] AssignConversationRequest request) =>
        await ExecuteAsync(async actor =>
            Ok(await chatService.AssignAsync(id, actor.Id, actor.Role, request.StaffId)));

    [HttpPatch("{id:long}/close")]
    public async Task<IActionResult> Close(long id) =>
        await ExecuteAsync(async actor =>
            Ok(await chatService.CloseAsync(id, actor.Id, actor.Role)));

    private async Task<IActionResult> ExecuteAsync(Func<(long Id, string Role), Task<IActionResult>> action)
    {
        var actor = GetActor();
        if (actor.Id <= 0) return Unauthorized(new { message = "Unauthorized" });

        try
        {
            return await action(actor);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private (long Id, string Role) GetActor()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = long.TryParse(idValue, out var userId);
        return (userId, User.FindFirstValue(ClaimTypes.Role) ?? string.Empty);
    }
}
