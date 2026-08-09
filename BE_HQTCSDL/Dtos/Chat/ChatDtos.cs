using System.ComponentModel.DataAnnotations;

namespace BE_HQTCSDL.Dtos;

public sealed class CreateConversationRequest
{
    [MaxLength(30)]
    public string Type { get; set; } = "GENERAL_SUPPORT";

    public long? OrderId { get; set; }
}

public sealed class SendChatMessageRequest
{
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}

public sealed class AssignConversationRequest
{
    public long? StaffId { get; set; }
}

public sealed class ConversationResponse
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long? AssignedStaffId { get; set; }
    public long? OrderId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int UnreadCount { get; set; }
}

public sealed class ChatMessageResponse
{
    public long Id { get; set; }
    public long ConversationId { get; set; }
    public long SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
