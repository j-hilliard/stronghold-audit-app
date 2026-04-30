using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSectionNaOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditSectionNaOverride",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditSectionNaOverride", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditSectionNaOverride_AuditSection_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "audit",
                        principalTable: "AuditSection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditSectionNaOverride_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditSectionNaOverride_AuditId",
                schema: "audit",
                table: "AuditSectionNaOverride",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditSectionNaOverride_AuditId_SectionId",
                schema: "audit",
                table: "AuditSectionNaOverride",
                columns: new[] { "AuditId", "SectionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditSectionNaOverride_SectionId",
                schema: "audit",
                table: "AuditSectionNaOverride",
                column: "SectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditSectionNaOverride",
                schema: "audit");
        }
    }
}
