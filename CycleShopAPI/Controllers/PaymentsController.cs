using CycleShopAPI.Models;
using CycleShopAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CycleShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all endpoints
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<Payment>> GetPayment(Guid id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            
            if (payment == null)
                return NotFound();
                
            return Ok(payment);
        }

        [HttpGet("order/{orderId}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByOrder(Guid orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
            return Ok(payments);
        }

        [HttpPost]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<Payment>> ProcessPayment(ProcessPaymentRequest request)
        {
            try
            {
                var payment = new Payment
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    PaymentType = request.PaymentType,
                    StripePaymentId = request.StripePaymentId,
                    ReceiptUrl = request.ReceiptUrl
                };
                
                var processedPayment = await _paymentService.ProcessPaymentAsync(payment);
                return CreatedAtAction(nameof(GetPayment), new { id = processedPayment.PaymentId }, processedPayment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin,employee")]
        public async Task<IActionResult> UpdatePaymentStatus(Guid id, UpdatePaymentStatusRequest request)
        {
            var result = await _paymentService.UpdatePaymentStatusAsync(id, request.Status);
            if (result)
                return NoContent();
            
            return BadRequest("Failed to update payment status");
        }

        [HttpPost("{id}/refund")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RefundPayment(Guid id, RefundRequest request)
        {
            var result = await _paymentService.RefundPaymentAsync(id, request.Reason);
            if (result)
                return NoContent();
            
            return BadRequest("Failed to process refund");
        }

        [HttpGet("reports/by-date-range")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Ensure endDate is inclusive for the entire day
            endDate = endDate.Date.AddDays(1).AddTicks(-1);
            
            var payments = await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate);
            return Ok(payments);
        }

        [HttpGet("reports/total-for-period")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<decimal>> GetTotalPaymentsForPeriod([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            // Ensure endDate is inclusive for the entire day
            endDate = endDate.Date.AddDays(1).AddTicks(-1);
            
            var total = await _paymentService.GetTotalPaymentsForPeriodAsync(startDate, endDate);
            return Ok(total);
        }
    }

    public class ProcessPaymentRequest
    {
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public PaymentType PaymentType { get; set; }
        public string StripePaymentId { get; set; }
        public string ReceiptUrl { get; set; }
    }

    public class UpdatePaymentStatusRequest
    {
        public PaymentStatus Status { get; set; }
    }

    public class RefundRequest
    {
        public string Reason { get; set; }
    }
}