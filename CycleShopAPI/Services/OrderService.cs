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

        public async Task<Order> CreateOrderAsync(Order order, List<OrderItemDTO> itemDtos)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    order.OrderNumber = GenerateOrderNumber();
                    order.OrderDate = DateTime.UtcNow;
                    order.Status = OrderStatus.pending;
                    order.CreatedAt = DateTime.UtcNow;
                    order.UpdatedAt = DateTime.UtcNow;

                    var customer = await _context.Customers.FindAsync(order.CustomerId);
                    if (customer == null)
                    {
                        throw new InvalidOperationException($"Customer with ID {order.CustomerId} does not exist");
                    }

                    var employee = await _context.Users.FindAsync(order.EmployeeId);
                    if (employee == null)
                    {
                        throw new InvalidOperationException($"Employee with ID {order.EmployeeId} does not exist");
                    }

                    await _context.Orders.AddAsync(order);
                    await _context.SaveChangesAsync();

                    decimal subtotal = 0;
                    foreach (var dto in itemDtos)
                    {
                        var cycle = await _context.Cycles.FindAsync(dto.CycleId);
                        if (cycle == null)
                        {
                            throw new InvalidOperationException($"Cycle with ID {dto.CycleId} does not exist");
                        }

                        var inventory = await _inventoryService.GetInventoryByCycleIdAsync(dto.CycleId);
                        if (inventory == null || inventory.StockQuantity < dto.Quantity)
                        {
                            throw new InvalidOperationException($"Not enough stock for cycle {cycle.ModelName}. Available: {inventory?.StockQuantity ?? 0}");
                        }

                        var orderItem = new OrderItem
                        {
                            OrderId = order.OrderId,
                            CycleId = dto.CycleId,
                            Quantity = dto.Quantity < 1 ? throw new InvalidOperationException("Quantity must be at least 1") : dto.Quantity,
                            TaxRate = dto.TaxRate,
                            PriceSnapshot = cycle.Price
                        };
                        orderItem.TotalPrice = orderItem.PriceSnapshot * orderItem.Quantity;
                        
                        await _context.OrderItems.AddAsync(orderItem);
                        
                        subtotal += orderItem.TotalPrice;
                    }

                    order.Subtotal = subtotal;
                    order.Tax = Math.Round(subtotal * 0.1m, 2);         // 10% tax rate
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
                    return false;
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

                    if (order == null || 
                        (order.Status != OrderStatus.pending && order.Status != OrderStatus.processing))
                    {
                        return false;
                    }

                    foreach (var item in order.OrderItems)
                    {
                        await _inventoryService.UpdateStockQuantityAsync(item.CycleId, item.Quantity);
                    }

                    order.Status = OrderStatus.cancelled;
                    order.UpdatedAt = DateTime.UtcNow;

                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
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

            if (order.Status != OrderStatus.cancelled)
            {
                bool cancelled = await CancelOrderAsync(orderId);
                if (!cancelled)
                {
                    return false;
                }
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
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

        private string GenerateOrderNumber()
        {
            var now = DateTime.UtcNow;
            string yearMonth = now.ToString("yyyyMM");
            
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