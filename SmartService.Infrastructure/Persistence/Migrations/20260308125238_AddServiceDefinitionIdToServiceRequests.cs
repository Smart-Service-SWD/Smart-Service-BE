using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceDefinitionIdToServiceRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceDefinitionId",
                table: "ServiceRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ServiceDefinitionId",
                table: "ServiceRequests",
                column: "ServiceDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_ServiceDefinitions_ServiceDefinitionId",
                table: "ServiceRequests",
                column: "ServiceDefinitionId",
                principalTable: "ServiceDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_ServiceDefinitions_ServiceDefinitionId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ServiceDefinitionId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServiceDefinitionId",
                table: "ServiceRequests");
        }
    }
}
