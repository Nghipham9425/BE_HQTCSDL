using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("ORDERS")]
    public class Order
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Column("CUSTOMER_ID")]
        public long CustomerId { get; set; }

        [Column("VOUCHER_ID")]
        public long? VoucherId { get; set; }

        [Column("AMOUNT")]
        public long Amount { get; set; }

        [Column("DISCOUNT_AMOUNT")]
        public long DiscountAmount { get; set; } = 0;

        [Column("SHIPPING_ADDRESS")]
        [MaxLength(300)]
        public string? ShippingAddress { get; set; }

        [Column("ORDER_EMAIL")]
        [MaxLength(200)]
        public string? OrderEmail { get; set; }

        [Column("NOTE")]
        [MaxLength(500)]
        public string? Note { get; set; }

        [Column("PAYMENT_METHOD_ID")]
        public long? PaymentMethodId { get; set; }

        [Column("ORDER_DATE")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column("ORDER_STATUS")]
        [MaxLength(20)]
        public string OrderStatus { get; set; } = "PENDING";
        // PENDING | CONFIRMED | SHIPPED | DONE | CANCELLED

        // Navigation
        [ForeignKey("CustomerId")]
        public User Customer { get; set; } = null!;

        [ForeignKey("VoucherId")]
        public Voucher? Voucher { get; set; }

        [ForeignKey("PaymentMethodId")]
        public PaymentMethod? PaymentMethod { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; } = [];
        public ICollection<Payment> Payments { get; set; } = [];
        public ICollection<Review> Reviews { get; set; } = [];
    }
}
