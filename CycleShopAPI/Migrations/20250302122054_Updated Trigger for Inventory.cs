using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CycleShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTriggerforInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    CREATE OR REPLACE FUNCTION update_inventory()
    RETURNS TRIGGER AS $$
    DECLARE
        current_stock INT;
    BEGIN
        IF TG_OP = 'INSERT' THEN
            -- Subtract the quantity from Inventories for the given CycleId
            UPDATE ""Inventories""
            SET ""StockQuantity"" = ""StockQuantity"" - NEW.""Quantity"",
                ""LastStockUpdate"" = NOW()
            WHERE ""CycleId"" = NEW.""CycleId"";
            
            -- Retrieve the updated stock quantity
            SELECT ""StockQuantity"" INTO current_stock 
            FROM ""Inventories"" 
            WHERE ""CycleId"" = NEW.""CycleId"";
            
            -- Insert inventory history record for a sale
            INSERT INTO ""InventoryHistories"" (
                ""CycleId"", 
                ""PreviousQuantity"", 
                ""NewQuantity"",
                ""ChangeReason"",
                ""OrderId"",
                ""UserId"",
                ""CreatedAt""
            )
            VALUES (
                NEW.""CycleId"",
                current_stock + NEW.""Quantity"",  -- previous quantity before subtraction
                current_stock,                     -- new quantity after subtraction
                'SALE',
                NEW.""OrderId"",
                current_user_id(),
                NOW()
            );
            
        ELSIF TG_OP = 'DELETE' THEN
            -- Add the quantity back to Inventories for the given CycleId
            UPDATE ""Inventories""
            SET ""StockQuantity"" = ""StockQuantity"" + OLD.""Quantity"",
                ""LastStockUpdate"" = NOW()
            WHERE ""CycleId"" = OLD.""CycleId"";
            
            -- Retrieve the updated stock quantity
            SELECT ""StockQuantity"" INTO current_stock 
            FROM ""Inventories"" 
            WHERE ""CycleId"" = OLD.""CycleId"";
            
            -- Insert inventory history record for a return
            INSERT INTO ""InventoryHistories"" (
                ""CycleId"", 
                ""PreviousQuantity"", 
                ""NewQuantity"",
                ""ChangeReason"",
                ""OrderId"",
                ""UserId"",
                ""CreatedAt""
            )
            VALUES (
                OLD.""CycleId"",
                current_stock - OLD.""Quantity"",  -- previous quantity before addition
                current_stock,                     -- new quantity after addition
                'RETURN',
                OLD.""OrderId"",
                current_user_id(),
                NOW()
            );
        END IF;
        
        RETURN NULL;
    EXCEPTION
        WHEN others THEN
            RAISE EXCEPTION 'Inventory update failed: %', SQLERRM;
    END;
    $$ LANGUAGE plpgsql;

    CREATE TRIGGER order_item_inventory_update
    AFTER INSERT OR DELETE ON ""OrderItems""
    FOR EACH ROW EXECUTE FUNCTION update_inventory();
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
