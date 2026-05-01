using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_workflow_statuses_and_notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Audit_Status",
                schema: "audit",
                table: "Audit");

            migrationBuilder.CreateTable(
                name: "AppNotification",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntityId = table.Column<int>(type: "int", nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppNotification", x => x.Id);
                    table.CheckConstraint("CK_AppNotification_Type", "[Type] IN ('AuditSubmitted','AuditApproved','AuditDistributed','CaAssigned','CaCompleted')");
                });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Audit_Status",
                schema: "audit",
                table: "Audit",
                sql: "[Status] IN ('Draft', 'Submitted', 'Reopened', 'UnderReview', 'Approved', 'Distributed', 'Closed')");

            migrationBuilder.CreateIndex(
                name: "IX_AppNotification_RecipientEmail",
                schema: "audit",
                table: "AppNotification",
                column: "RecipientEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AppNotification_RecipientEmail_IsRead",
                schema: "audit",
                table: "AppNotification",
                columns: new[] { "RecipientEmail", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppNotification",
                schema: "audit");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Audit_Status",
                schema: "audit",
                table: "Audit");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Audit_Status",
                schema: "audit",
                table: "Audit",
                sql: "[Status] IN ('Draft', 'Submitted', 'Reopened', 'Closed')");
        }
    }
}
