using System;
using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
    public class OrderDetailDto
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
        public string? OrderEmail { get; set; }
        public string? Note { get; set; }
        public long? PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? VoucherCode { get; set; }
        public List<OrderLineItemDto> Items { get; set; } = new();
    }
}