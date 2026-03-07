using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAnalysisFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstimatedDuration",
                table: "ServiceRequests",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedPrice",
                table: "ServiceRequests",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OcrExtractedText",
                table: "ServiceRequests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasAnalyzedByAI",
                table: "ServiceRequests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int[]>(
                name: "ComplexityRange",
                table: "ServiceDefinitions",
                type: "integer[]",
                nullable: false,
                defaultValueSql: "ARRAY[1,3]");

            migrationBuilder.AddColumn<bool>(
                name: "IsDangerous",
                table: "ServiceDefinitions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RiskExplanation",
                table: "ServiceAnalyses",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "EstimatedPrice",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "OcrExtractedText",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "WasAnalyzedByAI",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ComplexityRange",
                table: "ServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "IsDangerous",
                table: "ServiceDefinitions");

            migrationBuilder.DropColumn(
                name: "RiskExplanation",
                table: "ServiceAnalyses");
        }
    }
}
