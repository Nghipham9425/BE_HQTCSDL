using System.Collections.Generic;

namespace BE_HQTCSDL.Dtos
{
    public class OrderPlaceRequestDto
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? OrderEmail { get; set; }
        public string? Note { get; set; }
        public string? VoucherCode { get; set; }
        public long? PaymentMethodId { get; set; }
        public List<OrderPlaceItemDto> Items { get; set; } = new();
    }
}
