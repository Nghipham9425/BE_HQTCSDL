using BE_HQTCSDL.Dtos;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using BE_HQTCSDL.Services.Interfaces;
using BE_HQTCSDL.Utils;

namespace BE_HQTCSDL.Services;

public sealed class ChatService(IConversationRepository repository) : IChatService
{
    public async Task<List<ConversationResponse>> GetConversationsAsync(long actorId, string actorRole)
    {
        ValidateActor(actorId);

        var conversations = IsSupportRole(actorRole)
            ? await repository.GetStaffConversationsAsync()
            : await repository.GetCustomerConversationsAsync(actorId);

        return conversations.Select(x => MapConversation(x, actorId)).ToList();
    }

    public async Task<ConversationResponse> CreateConversationAsync(
        long customerId,
        CreateConversationRequest request)
    {
        ValidateActor(customerId);

        var type = request.Type?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!ConversationTypes.IsValid(type))
        {
            throw new ArgumentException("Conversation type must be GENERAL_SUPPORT or ORDER_SUPPORT");
        }

        if (request.OrderId is <= 0)
        {
            throw new ArgumentException("Invalid order id");
        }

        if (request.OrderId.HasValue &&
            !await repository.CustomerOwnsOrderAsync(customerId, request.OrderId.Value))
        {
            throw new KeyNotFoundException("Order not found");
        }

        if (request.OrderId.HasValue)
        {
            type = ConversationTypes.OrderSupport;
        }

        var existing = await repository.GetOpenByCustomerAndOrderAsync(customerId, request.OrderId);
        if (existing is not null)
        {
            return MapConversation(existing, customerId);
        }

        var now = DateTime.UtcNow;
        var conversation = new Conversation
        {
            CustomerId = customerId,
            OrderId = request.OrderId,
            Type = type,
            Status = ConversationStatuses.AiActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        await repository.AddConversationAsync(conversation);
        await repository.SaveChangesAsync();
        return MapConversation(conversation, customerId);
    }

    public async Task<List<ChatMessageResponse>> GetMessagesAsync(
        long conversationId,
        long actorId,
        string actorRole)
    {
        var conversation = await GetAccessibleConversationAsync(conversationId, actorId, actorRole);
        var messages = await repository.GetMessagesAsync(conversation.Id);
        return messages.Select(MapMessage).ToList();
    }

    public async Task<ChatMessageResponse> SendMessageAsync(
        long conversationId,
        long actorId,
        string actorRole,
        SendChatMessageRequest request)
    {
        var conversation = await GetAccessibleConversationAsync(conversationId, actorId, actorRole);
        if (conversation.Status == ConversationStatuses.Closed)
        {
            throw new InvalidOperationException("Conversation is closed");
        }

        var content = request.Content?.Trim() ?? string.Empty;
        if (content.Length == 0)
        {
            throw new ArgumentException("Message content is required");
        }

        var isSupport = IsSupportRole(actorRole);
        if (isSupport)
        {
            if (actorRole != AppRoles.Admin && conversation.AssignedStaffId != actorId)
            {
                throw new UnauthorizedAccessException("Assign this conversation before replying");
            }

            conversation.AssignedStaffId ??= actorId;
            conversation.Status = ConversationStatuses.StaffActive;
        }

        var message = new ChatMessage
        {
            ConversationId = conversationId,
            SenderId = actorId,
            SenderType = isSupport ? ChatSenderTypes.Staff : ChatSenderTypes.Customer,
            Content = content,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        conversation.UpdatedAt = message.CreatedAt;
        await repository.AddMessageAsync(message);
        await repository.SaveChangesAsync();
        return MapMessage(message);
    }

    public async Task MarkAsReadAsync(long conversationId, long actorId, string actorRole)
    {
        await GetAccessibleConversationAsync(conversationId, actorId, actorRole);
        await repository.MarkMessagesAsReadAsync(conversationId, actorId);
    }

    public async Task<ConversationResponse> RequestHandoffAsync(long conversationId, long customerId)
    {
        var conversation = await GetAccessibleConversationAsync(conversationId, customerId, AppRoles.User);
        if (conversation.Status == ConversationStatuses.Closed)
        {
            throw new InvalidOperationException("Conversation is closed");
        }

        if (conversation.Status == ConversationStatuses.AiActive)
        {
            var now = DateTime.UtcNow;
            conversation.Status = ConversationStatuses.WaitingStaff;
            conversation.UpdatedAt = now;
            await repository.AddMessageAsync(new ChatMessage
            {
                ConversationId = conversationId,
                SenderType = ChatSenderTypes.System,
                Content = "Cuộc trò chuyện đã được chuyển đến nhân viên hỗ trợ.",
                CreatedAt = now
            });
            await repository.SaveChangesAsync();
        }

        return MapConversation(conversation, customerId);
    }

    public async Task<ConversationResponse> AssignAsync(
        long conversationId,
        long actorId,
        string actorRole,
        long? staffId)
    {
        EnsureSupportRole(actorRole);
        var conversation = await GetRequiredConversationAsync(conversationId);
        if (conversation.Status == ConversationStatuses.Closed)
        {
            throw new InvalidOperationException("Conversation is closed");
        }

        if (actorRole != AppRoles.Admin && staffId != actorId)
        {
            throw new UnauthorizedAccessException("Order managers can only assign conversations to themselves");
        }

        if (staffId.HasValue)
        {
            var staffRole = await repository.GetUserRoleAsync(staffId.Value);
            if (!IsSupportRole(staffRole))
            {
                throw new ArgumentException("Assigned user is not support staff");
            }
        }

        conversation.AssignedStaffId = staffId;
        conversation.Status = staffId.HasValue
            ? ConversationStatuses.StaffActive
            : ConversationStatuses.WaitingStaff;
        conversation.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync();
        return MapConversation(conversation, actorId);
    }

    public async Task<ConversationResponse> CloseAsync(
        long conversationId,
        long actorId,
        string actorRole)
    {
        var conversation = await GetAccessibleConversationAsync(conversationId, actorId, actorRole);
        if (IsSupportRole(actorRole) &&
            actorRole != AppRoles.Admin &&
            conversation.AssignedStaffId != actorId)
        {
            throw new UnauthorizedAccessException("Only assigned staff can close this conversation");
        }

        conversation.Status = ConversationStatuses.Closed;
        conversation.UpdatedAt = DateTime.UtcNow;
        await repository.SaveChangesAsync();
        return MapConversation(conversation, actorId);
    }

    private async Task<Conversation> GetAccessibleConversationAsync(
        long conversationId,
        long actorId,
        string actorRole)
    {
        ValidateActor(actorId);
        var conversation = await GetRequiredConversationAsync(conversationId);
        if (!IsSupportRole(actorRole) && conversation.CustomerId != actorId)
        {
            throw new UnauthorizedAccessException("You cannot access this conversation");
        }

        return conversation;
    }

    private async Task<Conversation> GetRequiredConversationAsync(long conversationId)
    {
        if (conversationId <= 0) throw new ArgumentException("Invalid conversation id");
        return await repository.GetByIdAsync(conversationId)
            ?? throw new KeyNotFoundException("Conversation not found");
    }

    private static ConversationResponse MapConversation(Conversation conversation, long readerId) => new()
    {
        Id = conversation.Id,
        CustomerId = conversation.CustomerId,
        AssignedStaffId = conversation.AssignedStaffId,
        OrderId = conversation.OrderId,
        Type = conversation.Type,
        Status = conversation.Status,
        CreatedAt = conversation.CreatedAt,
        UpdatedAt = conversation.UpdatedAt,
        UnreadCount = conversation.Messages.Count(x =>
            !x.IsRead && (x.SenderId == null || x.SenderId != readerId))
    };

    private static ChatMessageResponse MapMessage(ChatMessage message) => new()
    {
        Id = message.Id,
        ConversationId = message.ConversationId,
        SenderId = message.SenderId,
        SenderType = message.SenderType,
        Content = message.Content,
        IsRead = message.IsRead,
        CreatedAt = message.CreatedAt
    };

    private static bool IsSupportRole(string? role) =>
        role is AppRoles.Admin or AppRoles.OrderManager;

    private static void EnsureSupportRole(string role)
    {
        if (!IsSupportRole(role)) throw new UnauthorizedAccessException("Support role is required");
    }

    private static void ValidateActor(long actorId)
    {
        if (actorId <= 0) throw new ArgumentException("Invalid user id");
    }
}
