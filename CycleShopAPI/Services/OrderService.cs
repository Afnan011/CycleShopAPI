using CycleShopAPI.Data;
using CycleShopAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CycleShopAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly CycleShopContext _context;
        private readonly IInventoryService _inventoryService;

        public OrderService(CycleShopContext context, IInventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Cycle)
                .ToListAsync();
        }

        public async Task<Order> GetOrderByIdAsync(Guid id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Cycle)
                .FirstOrDefaultAsync(o => o.OrderId == id);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.ShippingAddress)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Cycle)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderAsync(Order order, List<OrderItem> items)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Generate a unique order number
                    string orderNumber = GenerateOrderNumber();
                    order.OrderNumber = orderNumber;
                    order.OrderDate = DateTime.UtcNow;
                    order.Status = OrderStatus.pending;
                    order.CreatedAt = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;

                    // Check if customer exists
                    var customer = await _context.Customers.FindAsync(order.CustomerId);
                    if (customer == null)
                    {
                        throw new InvalidOperationException($"Customer with ID {order.CustomerId} does not exist");
                    }

                    // Check if employee exists
                    var employee = await _context.Users.FindAsync(order.EmployeeId);
                    if (employee == null)
                    {
                        throw new InvalidOperationException($"Employee with ID {order.EmployeeId} does not exist");
                    }

                    // Add the order
                    await _context.Orders.AddAsync(order);
                    await _context.SaveChangesAsync();

                    // Calculate order totals
                    decimal subtotal = 0;
                    foreach (var item in items)
                    {
                        // Check if cycle exists and get its price
                        var cycle = await _context.Cycles.FindAsync(item.CycleId);
                        if (cycle == null)
                        {
                            throw new InvalidOperationException($"Cycle with ID {item.CycleId} does not exist");
                        }

                        // Check inventory availability
                        var inventory = await _inventoryService.GetInventoryByCycleIdAsync(item.CycleId);
                        if (inventory == null || inventory.StockQuantity < item.Quantity)
                        {
                            throw new InvalidOperationException($"Not enough stock for cycle {cycle.ModelName}. Available: {inventory?.StockQuantity ?? 0}");
                        }

                        // Set order item properties
                        item.OrderId = order.OrderId;
                        item.UnitPrice = cycle.Price;
                        item.TotalPrice = item.UnitPrice * item.Quantity;
                        
                        // Add the order item
                        await _context.OrderItems.AddAsync(item);
                        
                        // Update inventory by reducing stock
                        await _inventoryService.UpdateStockQuantityAsync(item.CycleId, -item.Quantity);
                        
                        subtotal += item.TotalPrice;
                    }

                    // Update order with calculated totals
                    order.Subtotal = subtotal;
                    // Assuming tax is calculated as a percentage of the subtotal
                    order.Tax = Math.Round(subtotal * 0.1m, 2); // 10% tax rate example
                    order.TotalAmount = order.Subtotal + order.Tax - order.Discount;
                    
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return order;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            var existingOrder = await _context.Orders.FindAsync(order.OrderId);
            if (existingOrder == null)
            {
                return false;
            }

            // Update order properties
            existingOrder.Status = order.Status;
            existingOrder.ShippingAddressId = order.ShippingAddressId;
            existingOrder.Discount = order.Discount;
            existingOrder.Notes = order.Notes;
            existingOrder.UpdatedAt = DateTime.UtcNow;
            existingOrder.TotalAmount = existingOrder.Subtotal + existingOrder.Tax - order.Discount;

            _context.Orders.Update(existingOrder);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return false;
            }

            // Only allow certain status transitions
            switch (order.Status)
            {
                case OrderStatus.pending when status == OrderStatus.processing:
                case OrderStatus.processing when status == OrderStatus.completed:
                case OrderStatus.pending when status == OrderStatus.cancelled:
                case OrderStatus.processing when status == OrderStatus.cancelled:
                    order.Status = status;
                    order.UpdatedAt = DateTime.UtcNow;
                    _context.Orders.Update(order);
                    return await _context.SaveChangesAsync() > 0;
                default:
                    return false; // Invalid status transition
            }
        }

        public async Task<bool> CancelOrderAsync(Guid orderId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var order = await _context.Orders
                        .Include(o => o.OrderItems)
                        .FirstOrDefaultAsync(o => o.OrderId == orderId);

                    if (order == null)
                    {
                        return false;
                    }

                    // Only allow cancellation if the order is pending or processing
                    if (order.Status != OrderStatus.pending && order.Status != OrderStatus.processing)
                    {
                        return false;
                    }

                    // Restore inventory for each order item
                    foreach (var item in order.OrderItems)
                    {
                        // Return items to inventory
                        await _inventoryService.UpdateStockQuantityAsync(item.CycleId, item.Quantity);
                    }

                    // Update order status to cancelled
                    order.Status = OrderStatus.cancelled;
                    order.UpdatedAt = DateTime.UtcNow;

                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<bool> DeleteOrderAsync(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return false;
            }

            // Only allow deletion if the order is cancelled
            if (order.Status != OrderStatus.cancelled)
            {
                // Call cancel order first if it is not already cancelled
                bool cancelled = await CancelOrderAsync(orderId);
                if (!cancelled)
                {
                    return false;
                }
            }

            // Remove order items first
            _context.OrderItems.RemoveRange(order.OrderItems);
            
            // Then remove the order
            _context.Orders.Remove(order);
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Cycle)
                .ThenInclude(c => c.Brand)
                .Include(oi => oi.Cycle)
                .ThenInclude(c => c.CycleType)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<decimal> CalculateOrderTotalAsync(List<OrderItem> items)
        {
            decimal total = 0;
            foreach (var item in items)
            {
                var cycle = await _context.Cycles.FindAsync(item.CycleId);
                if (cycle == null)
                {
                    throw new InvalidOperationException($"Cycle with ID {item.CycleId} does not exist");
                }
                total += cycle.Price * item.Quantity;
            }
            return total;
        }

        // Helper methods
        private string GenerateOrderNumber()
        {
            // Generate a unique order number with year-month-sequential format
            // e.g., 2024070001
            var now = DateTime.UtcNow;
            string yearMonth = now.ToString("yyyyMM");
            
            // Get the last order number with this prefix
            var lastOrder = _context.Orders
                .Where(o => o.OrderNumber.StartsWith(yearMonth))
                .OrderByDescending(o => o.OrderNumber)
                .FirstOrDefault();
                
            int sequenceNumber = 1;
            if (lastOrder != null && int.TryParse(lastOrder.OrderNumber.Substring(6), out int lastSequence))
            {
                sequenceNumber = lastSequence + 1;
            }
            
            return $"{yearMonth}{sequenceNumber:D4}";
        }
    }
}