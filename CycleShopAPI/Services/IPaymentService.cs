using CycleShopAPI.Models;

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
    }
}