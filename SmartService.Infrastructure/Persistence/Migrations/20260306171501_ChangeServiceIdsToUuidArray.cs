using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ChangeServiceIdsToUuidArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceIds",
                table: "AgentCapabilities");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "ServiceIds",
                table: "AgentCapabilities",
                type: "uuid[]",
                nullable: false,
                defaultValue: new List<Guid>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<Guid>>(
                name: "ServiceIds",
                table: "AgentCapabilities",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(List<Guid>),
                oldType: "uuid[]");
        }
    }
}
