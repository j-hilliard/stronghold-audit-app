using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase2B_CaClosurePhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Division: RequireClosurePhoto setting ─────────────────────────────
            migrationBuilder.AddColumn<bool>(
                name: "RequireClosurePhoto",
                schema: "audit",
                table: "Division",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // ── CorrectiveActionPhoto table ───────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CorrectiveActionPhoto",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrectiveActionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    // AuditableEntity columns
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectiveActionPhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectiveActionPhoto_CorrectiveAction_CorrectiveActionId",
                        column: x => x.CorrectiveActionId,
                        principalSchema: "audit",
                        principalTable: "CorrectiveAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActionPhoto_CorrectiveActionId",
                schema: "audit",
                table: "CorrectiveActionPhoto",
                column: "CorrectiveActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrectiveActionPhoto",
                schema: "audit");

            migrationBuilder.DropColumn(
                name: "RequireClosurePhoto",
                schema: "audit",
                table: "Division");
        }
    }
}
