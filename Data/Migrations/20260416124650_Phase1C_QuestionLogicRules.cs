using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase1C_QuestionLogicRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionLogicRule",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateVersionId = table.Column<int>(type: "int", nullable: false),
                    TriggerVersionQuestionId = table.Column<int>(type: "int", nullable: false),
                    TriggerResponse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetSectionId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionLogicRule", x => x.Id);
                    table.CheckConstraint("CK_QuestionLogicRule_Action", "[Action] IN ('HideSection', 'ShowSection')");
                    table.ForeignKey(
                        name: "FK_QuestionLogicRule_AuditSection_TargetSectionId",
                        column: x => x.TargetSectionId,
                        principalSchema: "audit",
                        principalTable: "AuditSection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionLogicRule_AuditTemplateVersion_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalSchema: "audit",
                        principalTable: "AuditTemplateVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionLogicRule_AuditVersionQuestion_TriggerVersionQuestionId",
                        column: x => x.TriggerVersionQuestionId,
                        principalSchema: "audit",
                        principalTable: "AuditVersionQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionLogicRule_TargetSectionId",
                schema: "audit",
                table: "QuestionLogicRule",
                column: "TargetSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionLogicRule_TemplateVersionId",
                schema: "audit",
                table: "QuestionLogicRule",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionLogicRule_TriggerVersionQuestionId",
                schema: "audit",
                table: "QuestionLogicRule",
                column: "TriggerVersionQuestionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionLogicRule",
                schema: "audit");
        }
    }
}
