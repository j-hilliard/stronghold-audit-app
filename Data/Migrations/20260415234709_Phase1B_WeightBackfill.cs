using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase1B_WeightBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill all weight columns that were added with defaultValue: 0m.
            // Neutral weight is 1.0 — this keeps scoring identical to the pre-weight baseline.
            migrationBuilder.Sql("UPDATE audit.AuditQuestion SET Weight = 1.0 WHERE Weight = 0");
            migrationBuilder.Sql("UPDATE audit.AuditSection SET Weight = 1.0 WHERE Weight = 0");
            migrationBuilder.Sql("UPDATE audit.AuditResponse SET QuestionWeightSnapshot = 1.0 WHERE QuestionWeightSnapshot = 0");
            migrationBuilder.Sql("UPDATE audit.AuditResponse SET SectionWeightSnapshot = 1.0 WHERE SectionWeightSnapshot = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
