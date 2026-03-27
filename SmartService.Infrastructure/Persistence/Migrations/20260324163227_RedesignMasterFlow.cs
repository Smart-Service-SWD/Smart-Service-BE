using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RedesignMasterFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionAmount_Amount",
                table: "ServiceRequests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommissionAmount_Currency",
                table: "ServiceRequests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionRate",
                table: "ServiceRequests",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DepositAmount_Amount",
                table: "ServiceRequests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepositAmount_Currency",
                table: "ServiceRequests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalAmountPaid_Amount",
                table: "ServiceRequests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalAmountPaid_Currency",
                table: "ServiceRequests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice_Amount",
                table: "ServiceRequests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalPrice_Currency",
                table: "ServiceRequests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDepositPaid",
                table: "ServiceRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkerAmount_Amount",
                table: "ServiceRequests",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkerAmount_Currency",
                table: "ServiceRequests",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommissionSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    DepositPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissionSettings_ServiceDefinitions_ServiceDefinitionId",
                        column: x => x.ServiceDefinitionId,
                        principalTable: "ServiceDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletionEvidences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletionEvidences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletionEvidences_ServiceRequests_ServiceRequestId",
                        column: x => x.ServiceRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAmount_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount_Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CommissionAmount_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionAmount_Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    WorkerAmount_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    WorkerAmount_Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentReference = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceAdjustmentRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldPrice_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OldPrice_Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    NewPrice_Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NewPrice_Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceAdjustmentRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionSettings_ServiceDefinitionId",
                table: "CommissionSettings",
                column: "ServiceDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletionEvidences_ServiceRequestId",
                table: "CompletionEvidences",
                column: "ServiceRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionSettings");

            migrationBuilder.DropTable(
                name: "CompletionEvidences");

            migrationBuilder.DropTable(
                name: "Payouts");

            migrationBuilder.DropTable(
                name: "PriceAdjustmentRequests");

            migrationBuilder.DropColumn(
                name: "CommissionAmount_Amount",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "CommissionAmount_Currency",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "CommissionRate",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "DepositAmount_Amount",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "DepositAmount_Currency",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "FinalAmountPaid_Amount",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "FinalAmountPaid_Currency",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "FinalPrice_Amount",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "FinalPrice_Currency",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "IsDepositPaid",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "WorkerAmount_Amount",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "WorkerAmount_Currency",
                table: "ServiceRequests");
        }
    }
}
