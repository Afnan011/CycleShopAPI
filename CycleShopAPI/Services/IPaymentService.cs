using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleShopAPI.Services
{
    public interface IPaymentService
    {
        Task<Payment> GetPaymentByIdAsync(Guid id);
        Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId);
        Task<Payment> ProcessPaymentAsync(Payment payment);
        Task<bool> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status);
        Task<bool> RefundPaymentAsync(Guid paymentId, string reason);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalPaymentsForPeriodAsync(DateTime startDate, DateTime endDate);
        
        // Razorpay-specific methods
        Task<RazorpayCreateOrderResponse> CreateRazorpayOrderAsync(RazorpayCreateOrderRequest request);
        Task<RazorpayVerifyPaymentResponse> VerifyRazorpayPaymentAsync(RazorpayVerifyPaymentRequest request);
    }
}