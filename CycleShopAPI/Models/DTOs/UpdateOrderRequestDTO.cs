namespace CycleShopAPI.Models.DTOs
{
    public class UpdateOrderRequestDTO
    {
        public OrderStatus Status { get; set; }
        public Guid? ShippingAddressId { get; set; }
        public decimal Discount { get; set; }
        public string Notes { get; set; }
    }
}
