using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceAgentBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PayOSOrderCode",
                table: "ServiceRequests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Balance_Amount",
                table: "ServiceAgents",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Balance_Currency",
                table: "ServiceAgents",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayOSOrderCode",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "Balance_Amount",
                table: "ServiceAgents");

            migrationBuilder.DropColumn(
                name: "Balance_Currency",
                table: "ServiceAgents");
        }
    }
}
