using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_review_started_and_ca_submitted_statuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CorrectiveAction_Status",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppNotification_Type",
                schema: "audit",
                table: "AppNotification");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CorrectiveAction_Status",
                schema: "audit",
                table: "CorrectiveAction",
                sql: "[Status]   IN ('Open', 'InProgress', 'Submitted', 'Closed', 'Voided', 'Overdue')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppNotification_Type",
                schema: "audit",
                table: "AppNotification",
                sql: "[Type] IN ('AuditSubmitted','ReviewStarted','AuditApproved','AuditDistributed','CaAssigned','CaCompleted')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CorrectiveAction_Status",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AppNotification_Type",
                schema: "audit",
                table: "AppNotification");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CorrectiveAction_Status",
                schema: "audit",
                table: "CorrectiveAction",
                sql: "[Status]   IN ('Open', 'InProgress', 'Closed', 'Voided', 'Overdue')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AppNotification_Type",
                schema: "audit",
                table: "AppNotification",
                sql: "[Type] IN ('AuditSubmitted','AuditApproved','AuditDistributed','CaAssigned','CaCompleted')");
        }
    }
}
