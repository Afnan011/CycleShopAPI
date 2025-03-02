using System;

namespace CycleShopAPI.Models
{
    public class CycleType
    {
        public Guid TypeId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
