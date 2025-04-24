namespace CycleShopAPI.Models
{
    public enum UserRole { admin, employee }
    public enum OrderStatus { pending, processing, completed, cancelled, refunded }
    public enum PaymentStatus { requires_payment, requires_confirmation, succeeded, failed }
    public enum PaymentType { cash, stripe, razorpay }
}
