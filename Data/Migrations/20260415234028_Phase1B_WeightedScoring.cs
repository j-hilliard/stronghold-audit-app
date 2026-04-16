using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase1B_WeightedScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                schema: "audit",
                table: "AuditSection",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuestionWeightSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SectionWeightSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "audit",
                table: "AuditSection");

            migrationBuilder.DropColumn(
                name: "QuestionWeightSnapshot",
                schema: "audit",
                table: "AuditResponse");

            migrationBuilder.DropColumn(
                name: "SectionWeightSnapshot",
                schema: "audit",
                table: "AuditResponse");
        }
    }
}
