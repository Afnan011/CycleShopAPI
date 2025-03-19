using CycleShopAPI.Models;

namespace CycleShopAPI.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(Guid id);
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(Guid customerId);
        Task<Order> CreateOrderAsync(Order order, List<OrderItemDTO> items);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
        Task<bool> CancelOrderAsync(Guid orderId);
        Task<bool> DeleteOrderAsync(Guid orderId);
        Task<IEnumerable<OrderItem>> GetOrderItemsByOrderIdAsync(Guid orderId);
        Task<decimal> CalculateOrderTotalAsync(List<OrderItem> items);
    }
}