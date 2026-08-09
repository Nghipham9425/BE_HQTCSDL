using System;

namespace BE_HQTCSDL.Dtos
{
    public class UserAddressDto
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? AddressName { get; set; }
        public string? RecipientName { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = "Vietnam";
        public string? Phone { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UserAddressUpsertDto
    {
        public string? AddressName { get; set; }
        public string? RecipientName { get; set; }
        public string FullAddress { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Ward { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = "Vietnam";
        public string? Phone { get; set; }
        public bool IsDefault { get; set; }
    }

    public class UserAddressListResponseDto
    {
        public List<UserAddressDto> Items { get; set; } = new();
    }
}
