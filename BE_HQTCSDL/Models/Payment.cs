using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("PAYMENTS")]
    public class Payment
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("ORDER_ID")]
        public long OrderId { get; set; }

        [Column("PAYMENT_METHOD_ID")]
        public long PaymentMethodId { get; set; }

        [Column("AMOUNT")]
        public long Amount { get; set; }

        [Column("STATUS")]
        [MaxLength(20)]
        public string Status { get; set; } = "PENDING";
        // PENDING | SUCCESS | FAILED | REFUNDED

        [Column("TRANSACTION_ID")]
        [MaxLength(200)]
        public string? TransactionId { get; set; }

        [Column("PAID_AT")]
        public DateTime? PaidAt { get; set; }

        [Column("CREATED_AT")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("OrderId")]
        public Order Order { get; set; } = null!;

        [ForeignKey("PaymentMethodId")]
        public PaymentMethod PaymentMethod { get; set; } = null!;
    }
}
