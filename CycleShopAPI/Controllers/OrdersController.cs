using CycleShopAPI.Models;
using CycleShopAPI.Models.DTOs;
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
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<Order>> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            
            if (order == null)
                return NotFound();
                
            return Ok(order);
        }

        [HttpGet("customer/{customerId}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByCustomer(Guid customerId)
        {
            var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
            return Ok(orders);
        }

        [HttpPost]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<Order>> CreateOrder(CreateOrderRequestDTO request)
        {
            try
            {
                var order = new Order
                {
                    CustomerId = request.CustomerId,
                    EmployeeId = request.EmployeeId,
                    ShippingAddressId = request.ShippingAddressId,
                    Discount = request.Discount,
                    Notes = request.Notes
                };

                var createdOrder = await _orderService.CreateOrderAsync(order, request.Items);
                return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.OrderId }, createdOrder);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin,employee")]
        public async Task<IActionResult> UpdateOrder(Guid id, UpdateOrderRequestDTO request)
        {
            var existingOrder = await _orderService.GetOrderByIdAsync(id);
            if (existingOrder == null)
                return NotFound();

            existingOrder.Status = request.Status;
            existingOrder.ShippingAddressId = request.ShippingAddressId;
            existingOrder.Discount = request.Discount;
            existingOrder.Notes = request.Notes;

            var result = await _orderService.UpdateOrderAsync(existingOrder);
            if (result)
                return NoContent();
            
            return BadRequest("Failed to update order");
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "admin,employee")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, UpdateOrderStatusRequest request)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, request.Status);
            if (result)
                return NoContent();
            
            return BadRequest("Failed to update order status. Check if the order exists or if the status transition is allowed.");
        }

        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "admin,employee")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var result = await _orderService.CancelOrderAsync(id);
            if (result)
                return NoContent();
            
            return BadRequest("Failed to cancel order. Check if the order exists or if it can be cancelled.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var result = await _orderService.DeleteOrderAsync(id);
            if (result)
                return NoContent();
            
            return NotFound();
        }

        [HttpGet("{orderId}/items")]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems(Guid orderId)
        {
            var items = await _orderService.GetOrderItemsByOrderIdAsync(orderId);
            return Ok(items);
        }

        [HttpPost("calculate-total")]
        [Authorize(Roles = "admin,employee")]
        public async Task<ActionResult<decimal>> CalculateOrderTotal(List<OrderItem> items)
        {
            try
            {
                var total = await _orderService.CalculateOrderTotalAsync(items);
                return Ok(total);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}