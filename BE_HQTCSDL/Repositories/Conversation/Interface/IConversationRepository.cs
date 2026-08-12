using BE_HQTCSDL.Models;

namespace BE_HQTCSDL.Repositories.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(long conversationId);
    Task<Conversation?> GetOpenByCustomerAndOrderAsync(long customerId, long? orderId);
    Task<List<Conversation>> GetCustomerConversationsAsync(long customerId);
    Task<List<Conversation>> GetStaffConversationsAsync();
    Task<List<ChatMessage>> GetMessagesAsync(long conversationId);
    Task<bool> CustomerOwnsOrderAsync(long customerId, long orderId);
    Task<string?> GetUserRoleAsync(long userId);
    Task AddConversationAsync(Conversation conversation);
    Task AddMessageAsync(ChatMessage message);
    Task MarkMessagesAsReadAsync(long conversationId, long readerId);
    Task SaveChangesAsync();
}
