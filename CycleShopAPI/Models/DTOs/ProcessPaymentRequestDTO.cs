namespace CycleShopAPI.Models.DTOs
{
    public class ProcessPaymentRequestDTO
    {
        public Guid OrderId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string? StripePaymentId { get; set; }
        public string? ReceiptUrl { get; set; }
    }
}
