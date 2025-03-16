using CycleShopAPI.Models;

namespace CycleShopAPI.Data
{
    public static class DatabaseSeeder
    {
        public static void SeedData(CycleShopContext context)
        {
            // Check if data already exists
            if (context.Brands.Any() || context.CycleTypes.Any())
                return;

            // Add Brands
            var brands = new List<Brand>
            {
                new Brand { BrandId = Guid.NewGuid(), Name = "Trek", Description = "American bicycle manufacturer" },
                new Brand { BrandId = Guid.NewGuid(), Name = "Giant", Description = "Global bicycle manufacturer" },
                new Brand { BrandId = Guid.NewGuid(), Name = "Specialized", Description = "Premium bicycle brand" }
            };
            context.Brands.AddRange(brands);

            // Add Cycle Types
            var cycleTypes = new List<CycleType>
            {
                new CycleType { CycleTypeId = Guid.NewGuid(), Name = "Mountain Bike" },
                new CycleType { CycleTypeId = Guid.NewGuid(), Name = "Road Bike" },
                new CycleType { CycleTypeId = Guid.NewGuid(), Name = "Hybrid" }
            };
            context.CycleTypes.AddRange(cycleTypes);

            // Save changes to get IDs
            context.SaveChanges();

            // Add Cycles
            var cycles = new List<Cycle>
            {
                new Cycle {
                    CycleId = Guid.NewGuid(),
                    ModelName = "Trek Mountain Pro",
                    BrandId = brands[0].BrandId,
                    TypeId = cycleTypes[0].CycleTypeId,
                    Description = "Professional mountain bike",
                    Price = 1299.99m,
                    CostPrice = 899.99m,
                    IsActive = true
                },
                new Cycle {
                    CycleId = Guid.NewGuid(),
                    ModelName = "Giant Road Elite",
                    BrandId = brands[1].BrandId,
                    TypeId = cycleTypes[1].CycleTypeId,
                    Description = "Elite road cycling bike",
                    Price = 1599.99m,
                    CostPrice = 1099.99m,
                    IsActive = true
                },
                new Cycle {
                    CycleId = Guid.NewGuid(),
                    ModelName = "Specialized Hybrid Comfort",
                    BrandId = brands[2].BrandId,
                    TypeId = cycleTypes[2].CycleTypeId,
                    Description = "Comfortable hybrid bike",
                    Price = 899.99m,
                    CostPrice = 599.99m,
                    IsActive = true
                }
            };
            context.Cycles.AddRange(cycles);

            // Add Inventory
            var inventory = new List<Inventory>
            {
                new Inventory {
                    InventoryId = Guid.NewGuid(),
                    CycleId = cycles[0].CycleId,
                    StockQuantity = 10,
                    ReorderThreshold = 5,
                    WarehouseLocation = "A1-01",
                    LastStockUpdate = DateTime.UtcNow
                },
                new Inventory {
                    InventoryId = Guid.NewGuid(),
                    CycleId = cycles[1].CycleId,
                    StockQuantity = 8,
                    ReorderThreshold = 4,
                    WarehouseLocation = "A1-02",
                    LastStockUpdate = DateTime.UtcNow
                },
                new Inventory {
                    InventoryId = Guid.NewGuid(),
                    CycleId = cycles[2].CycleId,
                    StockQuantity = 15,
                    ReorderThreshold = 7,
                    WarehouseLocation = "A1-03",
                    LastStockUpdate = DateTime.UtcNow
                }
            };
            context.Inventories.AddRange(inventory);

            context.SaveChanges();
        }
    }
}