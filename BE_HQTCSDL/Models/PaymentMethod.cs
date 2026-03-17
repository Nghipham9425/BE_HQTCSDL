using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_HQTCSDL.Models
{
    [Table("PAYMENT_METHODS")]
    public class PaymentMethod
    {
        [Key]
        [Column("ID")]
        public long Id { get; set; }

        [Required]
        [Column("METHOD_NAME")]
        [MaxLength(100)]
        public string MethodName { get; set; } = string.Empty;
        // COD | VNPay | MoMo | ZaloPay | Bank Transfer

        [Column("IS_ACTIVE")]
        public int IsActive { get; set; } = 1;

        // Navigation
        public ICollection<Payment> Payments { get; set; } = [];
    }
}
