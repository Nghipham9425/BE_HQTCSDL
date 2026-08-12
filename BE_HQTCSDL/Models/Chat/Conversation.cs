using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models;

[Table("CONVERSATIONS")]
public class Conversation
{
    [Key]
    [Column("ID")]
    public long Id { get; set; }

    [Column("CUSTOMER_ID")]
    public long CustomerId { get; set; }

    [Column("ASSIGNED_STAFF_ID")]
    public long? AssignedStaffId { get; set; }

    [Column("ORDER_ID")]
    public long? OrderId { get; set; }

    [Required]
    [Column("TYPE")]
    [MaxLength(30)]
    public string Type { get; set; } = "GENERAL_SUPPORT";

    [Required]
    [Column("STATUS")]
    [MaxLength(20)]
    public string Status { get; set; } = ConversationStatuses.AiActive;

    [Column("CREATED_AT")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("UPDATED_AT")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User Customer { get; set; } = null!;
    public User? AssignedStaff { get; set; }
    public Order? Order { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = [];
}
