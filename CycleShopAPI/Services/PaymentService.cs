using CycleShopAPI.Data;
using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
using CycleShopAPI.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Razorpay.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CycleShopAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly CycleShopContext _context;
        private readonly IOrderService _orderService;
        private readonly RazorpaySettings _razorpaySettings;

        public PaymentService(CycleShopContext context, IOrderService orderService, IOptions<RazorpaySettings> razorpaySettings)
        {
            _context = context;
            _orderService = orderService;
            _razorpaySettings = razorpaySettings.Value;
        }

        public async Task<Models.Payment> GetPaymentByIdAsync(Guid id)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.PaymentId == id);
        }

        public async Task<IEnumerable<Models.Payment>> GetPaymentsByOrderIdAsync(Guid orderId)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .Where(p => p.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<Models.Payment> ProcessPaymentAsync(Models.Payment payment)
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
                    var refundPayment = new Models.Payment
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

        public async Task<IEnumerable<Models.Payment>> GetPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
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

        public async Task<RazorpayCreateOrderResponse> CreateRazorpayOrderAsync(RazorpayCreateOrderRequest request)
        {
            try
            {
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    throw new InvalidOperationException($"Order with ID {request.OrderId} does not exist");
                }

                // Calculate amount in smallest currency unit (paise)
                decimal amountInPaise = request.Amount * 100;

                // Initialize Razorpay client
                var razorpayClient = new RazorpayClient(_razorpaySettings.KeyId, _razorpaySettings.KeySecret);

                // Create order in Razorpay
                Dictionary<string, object> options = new()
                {
                    { "amount", Convert.ToInt32(amountInPaise) },
                    { "currency", request.Currency },
                    { "receipt", $"receipt_{request.OrderId}" },
                    { "payment_capture", 1 } // Auto-capture
                };

                // Make API call to Razorpay
                Razorpay.Api.Order razorpayOrder = razorpayClient.Order.Create(options);
                string razorpayOrderId = razorpayOrder["id"].ToString();

                // Create response
                var response = new RazorpayCreateOrderResponse
                {
                    RazorpayOrderId = razorpayOrderId,
                    RazorpayKeyId = _razorpaySettings.KeyId,
                    Amount = request.Amount,
                    Currency = request.Currency
                };

                return response;
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error creating Razorpay order: {ex.Message}");
                throw;
            }
        }

        public async Task<RazorpayVerifyPaymentResponse> VerifyRazorpayPaymentAsync(RazorpayVerifyPaymentRequest request)
        {
            try
            {
                // Verify signature
                string calculatedSignature = CalculateRazorpaySignature(
                    request.RazorpayOrderId,
                    request.RazorpayPaymentId,
                    _razorpaySettings.KeySecret);

                bool isSignatureValid = calculatedSignature == request.RazorpaySignature;

                if (!isSignatureValid)
                {
                    return new RazorpayVerifyPaymentResponse
                    {
                        IsAuthentic = false,
                        Message = "Payment signature verification failed",
                        Status = PaymentStatus.failed
                    };
                }

                // Find the order
                var order = await _context.Orders.FindAsync(request.OrderId);
                if (order == null)
                {
                    return new RazorpayVerifyPaymentResponse
                    {
                        IsAuthentic = false,
                        Message = $"Order with ID {request.OrderId} not found",
                        Status = PaymentStatus.failed
                    };
                }

                // Create a new payment record with dedicated Razorpay fields
                var payment = new Models.Payment
                {
                    OrderId = request.OrderId,
                    Amount = order.TotalAmount,
                    PaymentType = PaymentType.razorpay,
                    Status = PaymentStatus.succeeded,
                    
                    // Store Razorpay data in dedicated fields
                    RazorpayOrderId = request.RazorpayOrderId,
                    RazorpayPaymentId = request.RazorpayPaymentId,
                    RazorpaySignature = request.RazorpaySignature,
                    
                    // Keep these fields for backward compatibility
                    StripePaymentId = request.RazorpayPaymentId,
                    ReceiptUrl = $"Razorpay Payment",
                    
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add payment to database
                await _context.Payments.AddAsync(payment);
                await _context.SaveChangesAsync();

                // Update order status
                await _orderService.UpdateOrderStatusAsync(order.OrderId, OrderStatus.processing);

                return new RazorpayVerifyPaymentResponse
                {
                    IsAuthentic = true,
                    PaymentId = payment.PaymentId,
                    Status = PaymentStatus.succeeded,
                    Message = "Payment verification successful"
                };
            }
            catch (Exception ex)
            {
                // Log exception
                Console.WriteLine($"Error verifying Razorpay payment: {ex.Message}");
                
                return new RazorpayVerifyPaymentResponse
                {
                    IsAuthentic = false,
                    Message = $"Payment verification error: {ex.Message}",
                    Status = PaymentStatus.failed
                };
            }
        }

        // Private helper method to calculate Razorpay signature
        private string CalculateRazorpaySignature(string orderId, string paymentId, string secret)
        {
            string payload = $"{orderId}|{paymentId}";
            
            using HMACSHA256 hmac = new(Encoding.UTF8.GetBytes(secret));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}