using BE_HQTCSDL.Database;
using BE_HQTCSDL.Models;
using BE_HQTCSDL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BE_HQTCSDL.Repositories;

public sealed class ConversationRepository(ApplicationDbContext db) : IConversationRepository
{
    public Task<Conversation?> GetByIdAsync(long conversationId) =>
        db.Conversations
            .Include(x => x.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(x => x.Id == conversationId);

    public Task<Conversation?> GetOpenByCustomerAndOrderAsync(long customerId, long? orderId) =>
        db.Conversations
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.OrderId == orderId && x.Status == "OPEN");

    public Task<List<Conversation>> GetCustomerConversationsAsync(long customerId) =>
        db.Conversations
            .AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

    public Task<List<Conversation>> GetStaffConversationsAsync() =>
        db.Conversations
            .AsNoTracking()
            .Where(x => x.Status == "OPEN")
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();

    public Task<List<ChatMessage>> GetMessagesAsync(long conversationId) =>
        db.ChatMessages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    public Task<bool> CustomerOwnsOrderAsync(long customerId, long orderId) =>
        db.Orders.AnyAsync(x => x.Id == orderId && x.CustomerId == customerId);

    public Task<bool> UserExistsAsync(long userId) =>
        db.Users.AnyAsync(x => x.Id == userId);

    public Task AddConversationAsync(Conversation conversation) =>
        db.Conversations.AddAsync(conversation).AsTask();

    public Task AddMessageAsync(ChatMessage message) =>
        db.ChatMessages.AddAsync(message).AsTask();

    public Task MarkMessagesAsReadAsync(long conversationId, long readerId) =>
        db.ChatMessages
            .Where(x => x.ConversationId == conversationId && x.SenderId != readerId && !x.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsRead, true));

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
