using System;
using System.ComponentModel.DataAnnotations.Schema;
using CycleShopAPI;

namespace CycleShopAPI.Models
{
    public class InventoryHistory
    {
        public Guid HistoryId { get; set; }
        [ForeignKey("Cycle")]
        public Guid CycleId { get; set; }
        public Cycle Cycle { get; set; }
        public int PreviousQuantity { get; set; }
        public int NewQuantity { get; set; }
        public string ChangeReason { get; set; }
        public Guid? OrderId { get; set; }
        public Order Order { get; set; }
        public Guid? UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
