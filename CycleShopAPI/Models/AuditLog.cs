using System;
using CycleShopAPI;

namespace CycleShopAPI.Models
{
    public class AuditLog
    {
        public Guid LogId { get; set; }
        public Guid? UserId { get; set; }
        public User User { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public Guid RecordId { get; set; }
        public string Changes { get; set; } // Stored as JSON string
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
