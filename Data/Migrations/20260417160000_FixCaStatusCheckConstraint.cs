using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixCaStatusCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The original constraint only included Open/InProgress/Closed.
            // Voided and Overdue statuses were added to the model later but the
            // constraint was never updated — any write with Status='Voided' or
            // Status='Overdue' would fail at the DB layer.
            migrationBuilder.DropCheckConstraint(
                name: "CK_CorrectiveAction_Status",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CorrectiveAction_Status",
                schema: "audit",
                table: "CorrectiveAction",
                sql: "[Status] IN ('Open', 'InProgress', 'Closed', 'Voided', 'Overdue')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CorrectiveAction_Status",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CorrectiveAction_Status",
                schema: "audit",
                table: "CorrectiveAction",
                sql: "[Status] IN ('Open', 'InProgress', 'Closed')");
        }
    }
}
