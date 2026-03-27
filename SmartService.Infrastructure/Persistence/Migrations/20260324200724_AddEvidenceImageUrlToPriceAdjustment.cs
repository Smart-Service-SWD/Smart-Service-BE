using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceImageUrlToPriceAdjustment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvidenceImageUrl",
                table: "PriceAdjustmentRequests",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenceImageUrl",
                table: "PriceAdjustmentRequests");
        }
    }
}
