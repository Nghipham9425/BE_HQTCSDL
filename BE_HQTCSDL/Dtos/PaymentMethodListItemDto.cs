namespace BE_HQTCSDL.Dtos
{
    public class PaymentMethodListItemDto
    {
        public long Id { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int PaymentCount { get; set; }
    }
}
