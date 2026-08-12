using BE_HQTCSDL.Dtos;

namespace BE_HQTCSDL.Services.Interfaces;

public interface IChatService
{
    Task<List<ConversationResponse>> GetConversationsAsync(long actorId, string actorRole);
    Task<ConversationResponse> CreateConversationAsync(long customerId, CreateConversationRequest request);
    Task<List<ChatMessageResponse>> GetMessagesAsync(long conversationId, long actorId, string actorRole);
    Task<ChatMessageResponse> SendMessageAsync(
        long conversationId,
        long actorId,
        string actorRole,
        SendChatMessageRequest request);
    Task MarkAsReadAsync(long conversationId, long actorId, string actorRole);
    Task<ConversationResponse> RequestHandoffAsync(long conversationId, long customerId);
    Task<ConversationResponse> AssignAsync(
        long conversationId,
        long actorId,
        string actorRole,
        long? staffId);
    Task<ConversationResponse> CloseAsync(long conversationId, long actorId, string actorRole);
}
