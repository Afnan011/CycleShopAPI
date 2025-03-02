using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CycleShopAPI.Models
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        [ForeignKey("Order")]
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
        public PaymentType PaymentType { get; set; }
        public string StripePaymentId { get; set; }
        [Column(TypeName = "numeric(10,2)")]
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public string ReceiptUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
