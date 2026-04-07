using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppDirectory");

            migrationBuilder.DropTable(
                name: "AppDirectoryOrders");

            migrationBuilder.DropTable(
                name: "IntegratedAppOrders");

            migrationBuilder.DropTable(
                name: "IntegratedApps");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "safety");

            migrationBuilder.CreateTable(
                name: "AuditQuestion",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_AuditQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Division",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AuditType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_Division", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "process_log",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    incident_report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    process_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    process_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    log_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message_detail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    related_object = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    run_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    logged_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_process_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ref_company",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    next_incident_number = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_company", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ref_reference_type",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    applies_to = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_reference_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ref_severity_actual",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rank = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_severity_actual", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ref_severity_potential",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rank = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_severity_potential", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ref_workflow_state",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_workflow_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AuditTemplate",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AuditTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditTemplate_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmailRoutingRule",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_EmailRoutingRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailRoutingRule_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ref_region",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_region", x => x.id);
                    table.ForeignKey(
                        name: "FK_ref_region_ref_company_company_id",
                        column: x => x.company_id,
                        principalSchema: "safety",
                        principalTable: "ref_company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ref_doc_type",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reference_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_doc_type", x => x.id);
                    table.ForeignKey(
                        name: "FK_ref_doc_type_ref_reference_type_reference_type_id",
                        column: x => x.reference_type_id,
                        principalSchema: "safety",
                        principalTable: "ref_reference_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ref_incident_report_reference",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reference_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_incident_report_reference", x => x.id);
                    table.ForeignKey(
                        name: "FK_ref_incident_report_reference_ref_reference_type_reference_type_id",
                        column: x => x.reference_type_id,
                        principalSchema: "safety",
                        principalTable: "ref_reference_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ref_investigation_reference",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reference_type_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ref_investigation_reference", x => x.id);
                    table.ForeignKey(
                        name: "FK_ref_investigation_reference_ref_reference_type_reference_type_id",
                        column: x => x.reference_type_id,
                        principalSchema: "safety",
                        principalTable: "ref_reference_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditTemplateVersion",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ClonedFromVersionId = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_AuditTemplateVersion", x => x.Id);
                    table.CheckConstraint("CK_AuditTemplateVersion_Status", "[Status] IN ('Draft', 'Active', 'Superseded')");
                    table.ForeignKey(
                        name: "FK_AuditTemplateVersion_AuditTemplate_TemplateId",
                        column: x => x.TemplateId,
                        principalSchema: "audit",
                        principalTable: "AuditTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incident_report",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    incident_number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    incident_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    company_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    region_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    job_number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    plant_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    work_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    incident_summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    incident_class = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    severity_actual_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    severity_potential_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    health_safety_leader_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    senior_ops_leader_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BodyPartsInjured = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NatureOfInjury = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeOfEquipment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnitNumbers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Visibility = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvestigationDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormalInvestigationRequired = table.Column<bool>(type: "bit", nullable: false),
                    FullCauseMapRequired = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_report", x => x.id);
                    table.ForeignKey(
                        name: "FK_incident_report_ref_company_company_id",
                        column: x => x.company_id,
                        principalSchema: "safety",
                        principalTable: "ref_company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_incident_report_ref_region_region_id",
                        column: x => x.region_id,
                        principalSchema: "safety",
                        principalTable: "ref_region",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Audit",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    TemplateVersionId = table.Column<int>(type: "int", nullable: false),
                    AuditType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_Audit", x => x.Id);
                    table.CheckConstraint("CK_Audit_AuditType", "[AuditType] IN ('JobSite', 'Facility')");
                    table.CheckConstraint("CK_Audit_Status", "[Status] IN ('Draft', 'Submitted', 'Reopened', 'Closed')");
                    table.ForeignKey(
                        name: "FK_Audit_AuditTemplateVersion_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalSchema: "audit",
                        principalTable: "AuditTemplateVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Audit_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditSection",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateVersionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_AuditSection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditSection_AuditTemplateVersion_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalSchema: "audit",
                        principalTable: "AuditTemplateVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemplateChangeLog",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateVersionId = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangeNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateChangeLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateChangeLog_AuditTemplateVersion_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalSchema: "audit",
                        principalTable: "AuditTemplateVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "incident_action",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    incident_report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    action_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    action_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assigned_to = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    due_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    closed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_action", x => x.id);
                    table.ForeignKey(
                        name: "FK_incident_action_incident_report_incident_report_id",
                        column: x => x.incident_report_id,
                        principalSchema: "safety",
                        principalTable: "incident_report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "incident_employee_involved",
                schema: "safety",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    incident_report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    employee_identifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    employee_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    injury_type_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recordable = table.Column<bool>(type: "bit", nullable: true),
                    hours_worked = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_employee_involved", x => x.id);
                    table.ForeignKey(
                        name: "FK_incident_employee_involved_incident_report_incident_report_id",
                        column: x => x.incident_report_id,
                        principalSchema: "safety",
                        principalTable: "incident_report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "incident_report_reference",
                schema: "safety",
                columns: table => new
                {
                    incident_report_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reference_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incident_report_reference", x => new { x.incident_report_id, x.reference_id });
                    table.ForeignKey(
                        name: "FK_incident_report_reference_incident_report_incident_report_id",
                        column: x => x.incident_report_id,
                        principalSchema: "safety",
                        principalTable: "incident_report",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_incident_report_reference_ref_incident_report_reference_reference_id",
                        column: x => x.reference_id,
                        principalSchema: "safety",
                        principalTable: "ref_incident_report_reference",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditAttachment",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_AuditAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditAttachment_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditFinding",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    QuestionTextSnapshot = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectedOnSite = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AuditFinding", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditFinding_AuditQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "audit",
                        principalTable: "AuditQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditFinding_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditHeader",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    JobNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Client = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PM = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Time = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Shift = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WorkDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Company1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Company2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Company3 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ResponsibleParty = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AuditDate = table.Column<DateTime>(type: "date", nullable: true),
                    Auditor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_AuditHeader", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditHeader_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditResponse",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    QuestionTextSnapshot = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectedOnSite = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AuditResponse", x => x.Id);
                    table.CheckConstraint("CK_AuditResponse_Status", "[Status] IS NULL OR [Status] IN ('Conforming', 'NonConforming', 'Warning', 'NA')");
                    table.ForeignKey(
                        name: "FK_AuditResponse_AuditQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "audit",
                        principalTable: "AuditQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditResponse_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditVersionQuestion",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateVersionId = table.Column<int>(type: "int", nullable: false),
                    SectionId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    AllowNA = table.Column<bool>(type: "bit", nullable: false),
                    RequireCommentOnNC = table.Column<bool>(type: "bit", nullable: false),
                    IsScoreable = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_AuditVersionQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditVersionQuestion_AuditQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "audit",
                        principalTable: "AuditQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditVersionQuestion_AuditSection_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "audit",
                        principalTable: "AuditSection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditVersionQuestion_AuditTemplateVersion_TemplateVersionId",
                        column: x => x.TemplateVersionId,
                        principalSchema: "audit",
                        principalTable: "AuditTemplateVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CorrectiveAction",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FindingId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "date", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
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
                    table.PrimaryKey("PK_CorrectiveAction", x => x.Id);
                    table.CheckConstraint("CK_CorrectiveAction_Status", "[Status] IN ('Open', 'InProgress', 'Closed')");
                    table.ForeignKey(
                        name: "FK_CorrectiveAction_AuditFinding_FindingId",
                        column: x => x.FindingId,
                        principalSchema: "audit",
                        principalTable: "AuditFinding",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Audit_CreatedBy",
                schema: "audit",
                table: "Audit",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_DivisionId",
                schema: "audit",
                table: "Audit",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_Status",
                schema: "audit",
                table: "Audit",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Audit_TemplateVersionId",
                schema: "audit",
                table: "Audit",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachment_AuditId",
                schema: "audit",
                table: "AuditAttachment",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFinding_AuditId",
                schema: "audit",
                table: "AuditFinding",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditFinding_QuestionId",
                schema: "audit",
                table: "AuditFinding",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditHeader_AuditId",
                schema: "audit",
                table: "AuditHeader",
                column: "AuditId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponse_AuditId",
                schema: "audit",
                table: "AuditResponse",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponse_AuditId_QuestionId",
                schema: "audit",
                table: "AuditResponse",
                columns: new[] { "AuditId", "QuestionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AuditResponse_QuestionId",
                schema: "audit",
                table: "AuditResponse",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditSection_TemplateVersionId",
                schema: "audit",
                table: "AuditSection",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplate_DivisionId",
                schema: "audit",
                table: "AuditTemplate",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplateVersion_TemplateId_Status",
                schema: "audit",
                table: "AuditTemplateVersion",
                columns: new[] { "TemplateId", "Status" },
                unique: true,
                filter: "[IsDeleted] = 0 AND [Status] = 'Active'");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTemplateVersion_TemplateId_VersionNumber",
                schema: "audit",
                table: "AuditTemplateVersion",
                columns: new[] { "TemplateId", "VersionNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AuditVersionQuestion_QuestionId",
                schema: "audit",
                table: "AuditVersionQuestion",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditVersionQuestion_SectionId",
                schema: "audit",
                table: "AuditVersionQuestion",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditVersionQuestion_TemplateVersionId",
                schema: "audit",
                table: "AuditVersionQuestion",
                column: "TemplateVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditVersionQuestion_TemplateVersionId_QuestionId",
                schema: "audit",
                table: "AuditVersionQuestion",
                columns: new[] { "TemplateVersionId", "QuestionId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveAction_FindingId",
                schema: "audit",
                table: "CorrectiveAction",
                column: "FindingId");

            migrationBuilder.CreateIndex(
                name: "IX_Division_Code",
                schema: "audit",
                table: "Division",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailRoutingRule_DivisionId",
                schema: "audit",
                table: "EmailRoutingRule",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailRoutingRule_DivisionId_EmailAddress",
                schema: "audit",
                table: "EmailRoutingRule",
                columns: new[] { "DivisionId", "EmailAddress" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_incident_action_incident_report_id",
                schema: "safety",
                table: "incident_action",
                column: "incident_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_employee_involved_incident_report_id",
                schema: "safety",
                table: "incident_employee_involved",
                column: "incident_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_report_company_id",
                schema: "safety",
                table: "incident_report",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_report_region_id",
                schema: "safety",
                table: "incident_report",
                column: "region_id");

            migrationBuilder.CreateIndex(
                name: "IX_incident_report_reference_reference_id",
                schema: "safety",
                table: "incident_report_reference",
                column: "reference_id");

            migrationBuilder.CreateIndex(
                name: "IX_ref_doc_type_reference_type_id",
                schema: "safety",
                table: "ref_doc_type",
                column: "reference_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_ref_incident_report_reference_reference_type_id",
                schema: "safety",
                table: "ref_incident_report_reference",
                column: "reference_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_ref_investigation_reference_reference_type_id",
                schema: "safety",
                table: "ref_investigation_reference",
                column: "reference_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_ref_region_company_id",
                schema: "safety",
                table: "ref_region",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateChangeLog_TemplateVersionId",
                schema: "audit",
                table: "TemplateChangeLog",
                column: "TemplateVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditAttachment",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditHeader",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditResponse",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditVersionQuestion",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "CorrectiveAction",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "EmailRoutingRule",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "incident_action",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "incident_employee_involved",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "incident_report_reference",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "process_log",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "ref_doc_type",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "ref_investigation_reference",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "ref_severity_actual",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "ref_severity_potential",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "ref_workflow_state",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "TemplateChangeLog",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditSection",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditFinding",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "incident_report",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "ref_incident_report_reference",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "AuditQuestion",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "Audit",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ref_region",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "ref_reference_type",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "AuditTemplateVersion",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ref_company",
                schema: "safety");

            migrationBuilder.DropTable(
                name: "AuditTemplate",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "Division",
                schema: "audit");

            migrationBuilder.CreateTable(
                name: "AppDirectory",
                columns: table => new
                {
                    AppDirectoryEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisabledById = table.Column<int>(type: "int", nullable: true),
                    DisabledOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DisabledReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemOwnerId = table.Column<int>(type: "int", nullable: true),
                    TechSupportId = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDirectory", x => x.AppDirectoryEntryId);
                    table.ForeignKey(
                        name: "FK_AppDirectory_SystemOwners_SystemOwnerId",
                        column: x => x.SystemOwnerId,
                        principalTable: "SystemOwners",
                        principalColumn: "SystemOwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppDirectory_TechSupports_TechSupportId",
                        column: x => x.TechSupportId,
                        principalTable: "TechSupports",
                        principalColumn: "TechSupportId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntegratedApps",
                columns: table => new
                {
                    IntegratedAppEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisabledById = table.Column<int>(type: "int", nullable: true),
                    DisabledOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DisabledReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Icon = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Image = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemOwnerId = table.Column<int>(type: "int", nullable: true),
                    TechSupportId = table.Column<int>(type: "int", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAuthorizationApiUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegratedApps", x => x.IntegratedAppEntryId);
                    table.ForeignKey(
                        name: "FK_IntegratedApps_SystemOwners_SystemOwnerId",
                        column: x => x.SystemOwnerId,
                        principalTable: "SystemOwners",
                        principalColumn: "SystemOwnerId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntegratedApps_TechSupports_TechSupportId",
                        column: x => x.TechSupportId,
                        principalTable: "TechSupports",
                        principalColumn: "TechSupportId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppDirectoryOrders",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    AppDirectoryEntryId = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppDirectoryOrders", x => new { x.UserId, x.IsAdmin, x.AppDirectoryEntryId });
                    table.ForeignKey(
                        name: "FK_AppDirectoryOrders_IntegratedApps_AppDirectoryEntryId",
                        column: x => x.AppDirectoryEntryId,
                        principalTable: "IntegratedApps",
                        principalColumn: "IntegratedAppEntryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppDirectoryOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IntegratedAppOrders",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IntegratedAppEntryId = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegratedAppOrders", x => new { x.UserId, x.IsAdmin, x.IntegratedAppEntryId });
                    table.ForeignKey(
                        name: "FK_IntegratedAppOrders_IntegratedApps_IntegratedAppEntryId",
                        column: x => x.IntegratedAppEntryId,
                        principalTable: "IntegratedApps",
                        principalColumn: "IntegratedAppEntryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntegratedAppOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppDirectory_SystemOwnerId",
                table: "AppDirectory",
                column: "SystemOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDirectory_TechSupportId",
                table: "AppDirectory",
                column: "TechSupportId");

            migrationBuilder.CreateIndex(
                name: "IX_AppDirectoryOrders_AppDirectoryEntryId",
                table: "AppDirectoryOrders",
                column: "AppDirectoryEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegratedAppOrders_IntegratedAppEntryId",
                table: "IntegratedAppOrders",
                column: "IntegratedAppEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegratedApps_SystemOwnerId",
                table: "IntegratedApps",
                column: "SystemOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegratedApps_TechSupportId",
                table: "IntegratedApps",
                column: "TechSupportId");
        }
    }
}
