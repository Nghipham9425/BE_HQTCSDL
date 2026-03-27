using System;

namespace BE_HQTCSDL.Dtos
{
    public class OrderListItemDto
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public long Amount { get; set; }
        public long DiscountAmount { get; set; }
        public long FinalAmount { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PaymentMethodName { get; set; }
        public int ItemCount { get; set; }
    }
}
