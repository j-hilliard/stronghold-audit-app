using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditSchemaExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToUserId",
                schema: "audit",
                table: "CorrectiveAction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AuditId",
                schema: "audit",
                table: "CorrectiveAction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AuditResponseId",
                schema: "audit",
                table: "CorrectiveAction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedDate",
                schema: "audit",
                table: "CorrectiveAction",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EvidenceReceivedDate",
                schema: "audit",
                table: "CorrectiveAction",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EvidenceRequired",
                schema: "audit",
                table: "CorrectiveAction",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                schema: "audit",
                table: "AuditVersionQuestion",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                schema: "audit",
                table: "AuditTemplateVersion",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "audit",
                table: "AuditTemplateVersion",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuditType",
                schema: "audit",
                table: "AuditTemplate",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "audit",
                table: "AuditTemplate",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "audit",
                table: "AuditTemplate",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                schema: "audit",
                table: "AuditSection",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReportingCategoryId",
                schema: "audit",
                table: "AuditSection",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionCode",
                schema: "audit",
                table: "AuditSection",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CorrectiveActionDueDate",
                schema: "audit",
                table: "AuditResponse",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CorrectiveActionRequired",
                schema: "audit",
                table: "AuditResponse",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReportingCategorySnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionNameSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrderSnapshot",
                schema: "audit",
                table: "AuditResponse",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HelpText",
                schema: "audit",
                table: "AuditQuestion",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRequired",
                schema: "audit",
                table: "AuditQuestion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsScored",
                schema: "audit",
                table: "AuditQuestion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCommentOnWarning",
                schema: "audit",
                table: "AuditQuestion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RequireCorrectiveAction",
                schema: "audit",
                table: "AuditQuestion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ResponseTypeId",
                schema: "audit",
                table: "AuditQuestion",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortLabel",
                schema: "audit",
                table: "AuditQuestion",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowCorrectedOnSite",
                schema: "audit",
                table: "AuditQuestion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                schema: "audit",
                table: "AuditQuestion",
                type: "decimal(8,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ReportingCategory",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ResponseType",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserDivision",
                schema: "audit",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDivision", x => new { x.UserId, x.DivisionId });
                    table.ForeignKey(
                        name: "FK_UserDivision_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDivision_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResponseOption",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResponseTypeId = table.Column<int>(type: "int", nullable: false),
                    OptionLabel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OptionValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ScoreValue = table.Column<decimal>(type: "decimal(5,4)", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsNegativeFinding = table.Column<bool>(type: "bit", nullable: false),
                    TriggersComment = table.Column<bool>(type: "bit", nullable: false),
                    TriggersCorrectiveAction = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponseOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResponseOption_ResponseType_ResponseTypeId",
                        column: x => x.ResponseTypeId,
                        principalSchema: "audit",
                        principalTable: "ResponseType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditSection_ReportingCategoryId",
                schema: "audit",
                table: "AuditSection",
                column: "ReportingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditQuestion_ResponseTypeId",
                schema: "audit",
                table: "AuditQuestion",
                column: "ResponseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingCategory_Code",
                schema: "audit",
                table: "ReportingCategory",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponseOption_ResponseTypeId_OptionValue",
                schema: "audit",
                table: "ResponseOption",
                columns: new[] { "ResponseTypeId", "OptionValue" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ResponseType_Code",
                schema: "audit",
                table: "ResponseType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDivision_DivisionId",
                schema: "audit",
                table: "UserDivision",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDivision_UserId",
                schema: "audit",
                table: "UserDivision",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditQuestion_ResponseType_ResponseTypeId",
                schema: "audit",
                table: "AuditQuestion",
                column: "ResponseTypeId",
                principalSchema: "audit",
                principalTable: "ResponseType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditSection_ReportingCategory_ReportingCategoryId",
                schema: "audit",
                table: "AuditSection",
                column: "ReportingCategoryId",
                principalSchema: "audit",
                principalTable: "ReportingCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditQuestion_ResponseType_ResponseTypeId",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditSection_ReportingCategory_ReportingCategoryId",
                schema: "audit",
                table: "AuditSection");

            migrationBuilder.DropTable(
                name: "ReportingCategory",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ResponseOption",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "UserDivision",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ResponseType",
                schema: "audit");

            migrationBuilder.DropIndex(
                name: "IX_AuditSection_ReportingCategoryId",
                schema: "audit",
                table: "AuditSection");

            migrationBuilder.DropIndex(
                name: "IX_AuditQuestion_ResponseTypeId",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "AssignedToUserId",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.DropColumn(
                name: "AuditId",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.DropColumn(
                name: "AuditResponseId",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.DropColumn(
                name: "ClosedDate",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.DropColumn(
                name: "EvidenceReceivedDate",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.DropColumn(
                name: "EvidenceRequired",
                schema: "audit",
                table: "CorrectiveAction");

            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "audit",
                table: "AuditVersionQuestion");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                schema: "audit",
                table: "AuditTemplateVersion");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "audit",
                table: "AuditTemplateVersion");

            migrationBuilder.DropColumn(
                name: "AuditType",
                schema: "audit",
                table: "AuditTemplate");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "audit",
                table: "AuditTemplate");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "audit",
                table: "AuditTemplate");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                schema: "audit",
                table: "AuditSection");

            migrationBuilder.DropColumn(
                name: "ReportingCategoryId",
                schema: "audit",
                table: "AuditSection");

            migrationBuilder.DropColumn(
                name: "SectionCode",
                schema: "audit",
                table: "AuditSection");

            migrationBuilder.DropColumn(
                name: "CorrectiveActionDueDate",
                schema: "audit",
                table: "AuditResponse");

            migrationBuilder.DropColumn(
                name: "CorrectiveActionRequired",
                schema: "audit",
                table: "AuditResponse");

            migrationBuilder.DropColumn(
                name: "ReportingCategorySnapshot",
                schema: "audit",
                table: "AuditResponse");

            migrationBuilder.DropColumn(
                name: "SectionNameSnapshot",
                schema: "audit",
                table: "AuditResponse");

            migrationBuilder.DropColumn(
                name: "SortOrderSnapshot",
                schema: "audit",
                table: "AuditResponse");

            migrationBuilder.DropColumn(
                name: "HelpText",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "IsRequired",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "IsScored",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "RequireCommentOnWarning",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "RequireCorrectiveAction",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "ResponseTypeId",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "ShortLabel",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "ShowCorrectedOnSite",
                schema: "audit",
                table: "AuditQuestion");

            migrationBuilder.DropColumn(
                name: "Weight",
                schema: "audit",
                table: "AuditQuestion");
        }
    }
}
