using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models;

[Table("CHAT_MESSAGES")]
public class ChatMessage
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("CONVERSATION_ID")]
    public long ConversationId { get; set; }

    [Column("SENDER_ID")]
    public long? SenderId { get; set; }

    [Required]
    [Column("SENDER_TYPE")]
    [MaxLength(20)]
    public string SenderType { get; set; } = ChatSenderTypes.Customer;

    [Required]
    [Column("CONTENT")]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    [Column("IS_READ")]
    public bool IsRead { get; set; }

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Conversation Conversation { get; set; } = null!;
    public User? Sender { get; set; }
}
