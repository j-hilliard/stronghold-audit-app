using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase1B_LifeCritical : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                schema: "audit",
                table: "AuditSection",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OptionalGroupKey",
                schema: "audit",
                table: "AuditSection",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLifeCriticalSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLifeCritical",
                schema: "audit",
                table: "AuditQuestion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AuditEnabledSection",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    OptionalGroupKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEnabledSection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditEnabledSection_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEnabledSection_AuditId",
                schema: "audit",
                table: "AuditEnabledSection",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEnabledSection_AuditId_OptionalGroupKey",
                schema: "audit",
                table: "AuditEnabledSection",
                columns: new[] { "AuditId", "OptionalGroupKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEnabledSection",
                schema: "audit");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                schema: "audit",
                table: "AuditSection");

            migrationBuilder.DropColumn(
                name: "OptionalGroupKey",
                schema: "audit",
                table: "AuditSection");

            migrationBuilder.DropColumn(
                name: "IsLifeCriticalSnapshot",
                schema: "audit",
                table: "AuditResponse");

            migrationBuilder.DropColumn(
                name: "IsLifeCritical",
                schema: "audit",
                table: "AuditQuestion");
        }
    }
}
