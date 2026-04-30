using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class add_ca_public_token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaPublicToken",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrectiveActionId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SentToName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SentToEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaPublicToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaPublicToken_CorrectiveAction_CorrectiveActionId",
                        column: x => x.CorrectiveActionId,
                        principalSchema: "audit",
                        principalTable: "CorrectiveAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaPublicToken_CorrectiveActionId",
                schema: "audit",
                table: "CaPublicToken",
                column: "CorrectiveActionId");

            migrationBuilder.CreateIndex(
                name: "IX_CaPublicToken_Token",
                schema: "audit",
                table: "CaPublicToken",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaPublicToken",
                schema: "audit");
        }
    }
}
