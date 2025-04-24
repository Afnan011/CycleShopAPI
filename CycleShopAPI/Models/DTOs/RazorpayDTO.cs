using System;

namespace CycleShopAPI.Models.DTOs
{
    // Request DTOs
    public class RazorpayCreateOrderRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
    }

    public class RazorpayVerifyPaymentRequest
    {
        public Guid OrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpayOrderId { get; set; }
        public string RazorpaySignature { get; set; }
    }

    // Response DTOs
    public class RazorpayCreateOrderResponse
    {
        public string RazorpayOrderId { get; set; }
        public string RazorpayKeyId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }

    public class RazorpayVerifyPaymentResponse
    {
        public bool IsAuthentic { get; set; }
        public Guid PaymentId { get; set; }
        public PaymentStatus Status { get; set; }
        public string Message { get; set; }
    }
}