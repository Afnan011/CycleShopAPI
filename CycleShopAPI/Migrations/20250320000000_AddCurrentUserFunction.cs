using Microsoft.EntityFrameworkCore.Migrations;

namespace CycleShopAPI.Migrations
{
    public partial class AddCurrentUserFunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First drop the existing trigger
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS order_item_inventory_update ON \"OrderItems\";");
            
            // Then drop the existing function
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_inventory();");
            
            // Drop the current_user_id function if it exists
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS current_user_id();");

            // Create the current_user_id function first
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION current_user_id()
                RETURNS uuid AS $$
                BEGIN
                    RETURN '00000000-0000-0000-0000-000000000000'::uuid;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Then create the inventory update function
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
                        WHERE ""CycleId"" = NEW.""CycleId""
                        RETURNING ""StockQuantity"" INTO current_stock;

                        IF current_stock < 0 THEN
                            RAISE EXCEPTION 'Insufficient stock available';
                        END IF;
                        
                        -- Insert inventory history record for a sale
                        INSERT INTO ""InventoryHistories"" (
                            ""HistoryId"",
                            ""CycleId"", 
                            ""PreviousQuantity"", 
                            ""NewQuantity"",
                            ""ChangeReason"",
                            ""OrderId"",
                            ""UserId"",
                            ""CreatedAt""
                        )
                        VALUES (
                            gen_random_uuid(),
                            NEW.""CycleId"",
                            current_stock + NEW.""Quantity"",
                            current_stock,
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
                        WHERE ""CycleId"" = OLD.""CycleId""
                        RETURNING ""StockQuantity"" INTO current_stock;
                        
                        -- Insert inventory history record for a return
                        INSERT INTO ""InventoryHistories"" (
                            ""HistoryId"",
                            ""CycleId"", 
                            ""PreviousQuantity"", 
                            ""NewQuantity"",
                            ""ChangeReason"",
                            ""OrderId"",
                            ""UserId"",
                            ""CreatedAt""
                        )
                        VALUES (
                            gen_random_uuid(),
                            OLD.""CycleId"",
                            current_stock - OLD.""Quantity"",
                            current_stock,
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
            ");

            // Finally, create the trigger
            migrationBuilder.Sql(@"
                CREATE TRIGGER order_item_inventory_update
                AFTER INSERT OR DELETE ON ""OrderItems""
                FOR EACH ROW EXECUTE FUNCTION update_inventory();
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS order_item_inventory_update ON \"OrderItems\";");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS update_inventory();");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS current_user_id();");
        }
    }
}