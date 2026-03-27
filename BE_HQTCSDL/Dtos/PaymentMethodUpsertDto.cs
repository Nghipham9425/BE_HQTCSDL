namespace BE_HQTCSDL.Dtos
{
    public class PaymentMethodUpsertDto
    {
        public string MethodName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
