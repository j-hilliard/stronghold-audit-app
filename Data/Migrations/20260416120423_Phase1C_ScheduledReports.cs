using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase1C_ScheduledReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                schema: "audit",
                table: "AuditSection",
                type: "decimal(8,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "OptionalGroupKey",
                schema: "audit",
                table: "AuditSection",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SectionWeightSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "decimal(8,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuestionWeightSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "decimal(8,4)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "ScheduledReport",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    TemplateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    DayOfMonth = table.Column<int>(type: "int", nullable: true),
                    TimeUtc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateRangePreset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ScoreThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledReport", x => x.Id);
                    table.CheckConstraint("CK_ScheduledReport_Frequency", "[Frequency] IN ('Daily', 'Weekly', 'Monthly', 'Quarterly')");
                    table.ForeignKey(
                        name: "FK_ScheduledReport_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledReport_DivisionId",
                schema: "audit",
                table: "ScheduledReport",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledReport_IsActive_NextRunAt",
                schema: "audit",
                table: "ScheduledReport",
                columns: new[] { "IsActive", "NextRunAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledReport",
                schema: "audit");

            migrationBuilder.AlterColumn<decimal>(
                name: "Weight",
                schema: "audit",
                table: "AuditSection",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,4)");

            migrationBuilder.AlterColumn<string>(
                name: "OptionalGroupKey",
                schema: "audit",
                table: "AuditSection",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "SectionWeightSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,4)");

            migrationBuilder.AlterColumn<decimal>(
                name: "QuestionWeightSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,4)");
        }
    }
}
