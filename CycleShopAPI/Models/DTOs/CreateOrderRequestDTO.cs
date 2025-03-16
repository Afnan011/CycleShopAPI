namespace CycleShopAPI.Models.DTOs
{
    public class CreateOrderRequestDTO
    {
        public Guid CustomerId { get; set; }
        public Guid EmployeeId { get; set; }
        public Guid? ShippingAddressId { get; set; }
        public decimal Discount { get; set; } = 0;
        public string Notes { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
