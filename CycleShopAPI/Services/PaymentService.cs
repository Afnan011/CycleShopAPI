using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CycleShopAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly CycleShopContext _context;
        private readonly IOrderService _orderService;

        public PaymentService(CycleShopContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        public async Task<Payment> GetPaymentByIdAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<Payment> ProcessPaymentAsync(Payment payment)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders.FindAsync(payment.OrderId);
                    if (order == null)
                    {
                        throw new InvalidOperationException($"Order with ID {payment.OrderId} does not exist");
                    }

                    payment.Amount = order.TotalAmount;
                    payment.CreatedAt = DateTime.UtcNow;
                    payment.UpdatedAt = DateTime.UtcNow;
                    payment.Status = payment.PaymentType == PaymentType.cash 
                        ? PaymentStatus.succeeded 
                        : PaymentStatus.requires_confirmation;

                    if (payment.PaymentType != PaymentType.cash && string.IsNullOrEmpty(payment.StripePaymentId))
                    {
                        payment.StripePaymentId = GenerateTransactionReference();
                    }

                    await _context.Payments.AddAsync(payment);
                    await _context.SaveChangesAsync();

                    if (payment.Status == PaymentStatus.succeeded)
                    {
                        await _orderService.UpdateOrderStatusAsync(order.OrderId, OrderStatus.processing);
                    }

                    await transaction.CommitAsync();
                    return payment;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> UpdatePaymentStatusAsync(Guid paymentId, PaymentStatus status)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var payment = await _context.Payments
                        .Include(p => p.Order)
                        .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                    if (payment == null)
                    {
                        return false;
                    }

                    // Update payment status
                    payment.Status = status;
                    payment.UpdatedAt = DateTime.UtcNow;
                    
                    _context.Payments.Update(payment);
                    await _context.SaveChangesAsync();

                    // Update order status if needed
                    var order = payment.Order;
                    if (status == PaymentStatus.succeeded && order.Status == OrderStatus.pending)
                    {
                        await _orderService.UpdateOrderStatusAsync(order.OrderId, OrderStatus.processing);
                    }
                    else if (status == PaymentStatus.failed && order.Status == OrderStatus.pending)
                    {
                        // Mark the order as cancelled if payment fails
                        await _orderService.UpdateOrderStatusAsync(order.OrderId, OrderStatus.cancelled);
                    }

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task<bool> RefundPaymentAsync(Guid paymentId, string reason)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var payment = await _context.Payments
                        .Include(p => p.Order)
                        .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                    if (payment == null || payment.Status != PaymentStatus.succeeded)
                    {
                        return false;
                    }

                    // Create a refund record in the payments table
                    var refundPayment = new Payment
                    {
                        OrderId = payment.OrderId,
                        Amount = -payment.Amount, // Negative amount for refund
                        PaymentType = payment.PaymentType,
                        Status = PaymentStatus.succeeded,
                        StripePaymentId = $"REFUND-{payment.StripePaymentId}",
                        ReceiptUrl = payment.ReceiptUrl,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _context.Payments.AddAsync(refundPayment);

                    // Update order status to refunded
                    await _orderService.UpdateOrderStatusAsync(payment.OrderId, OrderStatus.refunded);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.CreatedAt >= startDate && p.CreatedAt <= endDate && p.Status == PaymentStatus.succeeded)
                .SumAsync(p => p.Amount);
        }

        // Helper methods
        private string GenerateTransactionReference()
        {
            return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
        }
    }
}