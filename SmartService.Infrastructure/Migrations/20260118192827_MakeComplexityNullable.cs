using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeComplexityNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCapability_ServiceAgents_ServiceAgentId",
                table: "AgentCapability");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AgentCapability",
                table: "AgentCapability");

            migrationBuilder.RenameTable(
                name: "AgentCapability",
                newName: "AgentCapabilities");

            migrationBuilder.RenameIndex(
                name: "IX_AgentCapability_ServiceAgentId",
                table: "AgentCapabilities",
                newName: "IX_AgentCapabilities_ServiceAgentId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ServiceRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<int>(
                name: "ComplexityLevel",
                table: "ServiceRequests",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgentCapabilities",
                table: "AgentCapabilities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCapabilities_ServiceAgents_ServiceAgentId",
                table: "AgentCapabilities",
                column: "ServiceAgentId",
                principalTable: "ServiceAgents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentCapabilities_ServiceAgents_ServiceAgentId",
                table: "AgentCapabilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AgentCapabilities",
                table: "AgentCapabilities");

            migrationBuilder.RenameTable(
                name: "AgentCapabilities",
                newName: "AgentCapability");

            migrationBuilder.RenameIndex(
                name: "IX_AgentCapabilities_ServiceAgentId",
                table: "AgentCapability",
                newName: "IX_AgentCapability_ServiceAgentId");

            migrationBuilder.AlterColumn<int>(
                name: "ComplexityLevel",
                table: "ServiceRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ServiceRequests",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgentCapability",
                table: "AgentCapability",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentCapability_ServiceAgents_ServiceAgentId",
                table: "AgentCapability",
                column: "ServiceAgentId",
                principalTable: "ServiceAgents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
