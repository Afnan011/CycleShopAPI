using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CycleShopAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRazorpayFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:order_status", "pending,processing,completed,cancelled,refunded")
                .Annotation("Npgsql:Enum:payment_status", "requires_payment,requires_confirmation,succeeded,failed")
                .Annotation("Npgsql:Enum:payment_type", "cash,stripe,razorpay")
                .Annotation("Npgsql:Enum:user_role", "admin,employee")
                .OldAnnotation("Npgsql:Enum:order_status", "pending,processing,completed,cancelled,refunded")
                .OldAnnotation("Npgsql:Enum:payment_status", "requires_payment,requires_confirmation,succeeded,failed")
                .OldAnnotation("Npgsql:Enum:payment_type", "cash,stripe")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,employee");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayOrderId",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RazorpayPaymentId",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RazorpaySignature",
                table: "Payments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RazorpayOrderId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RazorpayPaymentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RazorpaySignature",
                table: "Payments");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:order_status", "pending,processing,completed,cancelled,refunded")
                .Annotation("Npgsql:Enum:payment_status", "requires_payment,requires_confirmation,succeeded,failed")
                .Annotation("Npgsql:Enum:payment_type", "cash,stripe")
                .Annotation("Npgsql:Enum:user_role", "admin,employee")
                .OldAnnotation("Npgsql:Enum:order_status", "pending,processing,completed,cancelled,refunded")
                .OldAnnotation("Npgsql:Enum:payment_status", "requires_payment,requires_confirmation,succeeded,failed")
                .OldAnnotation("Npgsql:Enum:payment_type", "cash,stripe,razorpay")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,employee");
        }
    }
}
