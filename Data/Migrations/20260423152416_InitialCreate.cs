using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stronghold.AppDashboard.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "AuditActionLog",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Info"),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditActionLog", x => x.Id);
                    table.CheckConstraint("CK_AuditActionLog_Severity", "[Severity] IN ('Info', 'Warning', 'Error')");
                });

            migrationBuilder.CreateTable(
                name: "AuditTrailLog",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedColumns = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrailLog", x => x.Id);
                    table.CheckConstraint("CK_AuditTrailLog_Action", "[Action] IN ('Insert', 'Update', 'Delete')");
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
                    ScoreTarget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AuditFrequencyDays = table.Column<int>(type: "int", nullable: true),
                    RequireClosurePhoto = table.Column<bool>(type: "bit", nullable: false),
                    SlaNormalDays = table.Column<int>(type: "int", nullable: true),
                    SlaUrgentDays = table.Column<int>(type: "int", nullable: true),
                    SlaEscalateAfterDays = table.Column<int>(type: "int", nullable: true),
                    EscalationEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OPUNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
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
                name: "ReviewGroupMember",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_ReviewGroupMember", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ADUsersGroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ADUsersGroupGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ADAdminsGroupName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ADAdminsGroupGUID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingsId);
                });

            migrationBuilder.CreateTable(
                name: "SystemOwners",
                columns: table => new
                {
                    SystemOwnerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemOwners", x => x.SystemOwnerId);
                });

            migrationBuilder.CreateTable(
                name: "TechSupports",
                columns: table => new
                {
                    TechSupportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechSupports", x => x.TechSupportId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AzureAdObjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    DisabledOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DisabledById = table.Column<int>(type: "int", nullable: true),
                    DisabledReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastLogin = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ModifiedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Users_DisabledById",
                        column: x => x.DisabledById,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditNumberSequence",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    LastSequence = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditNumberSequence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditNumberSequence_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditTemplate",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AuditType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                name: "DivisionJobPrefix",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Prefix = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DivisionJobPrefix", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DivisionJobPrefix_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "NewsletterTemplate",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccentColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CoverImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VisibleSectionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_NewsletterTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsletterTemplate_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportDraft",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Period = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BlocksJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
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
                    table.PrimaryKey("PK_ReportDraft", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportDraft_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledReport",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DivisionId = table.Column<int>(type: "int", nullable: true),
                    TemplateId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    DayOfMonth = table.Column<int>(type: "int", nullable: true),
                    TimeUtc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DateRangePreset = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RecipientsJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryColor = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ScoreThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRunAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledReport", x => x.Id);
                    table.CheckConstraint("CK_ScheduledReport_Frequency", "[Frequency] IN ('Daily', 'Weekly', 'Monthly', 'Quarterly')");
                    table.ForeignKey(
                        name: "FK_ScheduledReport_Division_DivisionId",
                        column: x => x.DivisionId,
                        principalSchema: "audit",
                        principalTable: "Division",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditQuestion",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ShortLabel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HelpText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ResponseTypeId = table.Column<int>(type: "int", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    RequireCommentOnWarning = table.Column<bool>(type: "bit", nullable: false),
                    ShowCorrectedOnSite = table.Column<bool>(type: "bit", nullable: false),
                    RequireCorrectiveAction = table.Column<bool>(type: "bit", nullable: false),
                    IsScored = table.Column<bool>(type: "bit", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    IsLifeCritical = table.Column<bool>(type: "bit", nullable: false),
                    RequirePhotoOnNc = table.Column<bool>(type: "bit", nullable: false),
                    AutoCreateCa = table.Column<bool>(type: "bit", nullable: false),
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
                    table.ForeignKey(
                        name: "FK_AuditQuestion_ResponseType_ResponseTypeId",
                        column: x => x.ResponseTypeId,
                        principalSchema: "audit",
                        principalTable: "ResponseType",
                        principalColumn: "Id",
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
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
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
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    AiSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewSummary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    JobPrefixId = table.Column<int>(type: "int", nullable: true),
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
                        name: "FK_Audit_DivisionJobPrefix_JobPrefixId",
                        column: x => x.JobPrefixId,
                        principalSchema: "audit",
                        principalTable: "DivisionJobPrefix",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    SectionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ReportingCategoryId = table.Column<int>(type: "int", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    OptionalGroupKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
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
                    table.ForeignKey(
                        name: "FK_AuditSection_ReportingCategory_ReportingCategoryId",
                        column: x => x.ReportingCategoryId,
                        principalSchema: "audit",
                        principalTable: "ReportingCategory",
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
                name: "AuditAttachment",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BlobPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
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
                name: "AuditDistributionRecipient",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditDistributionRecipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditDistributionRecipient_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    SiteCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
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
                    SectionNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReportingCategorySnapshot = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrderSnapshot = table.Column<int>(type: "int", nullable: true),
                    QuestionWeightSnapshot = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    SectionWeightSnapshot = table.Column<decimal>(type: "decimal(8,4)", nullable: false),
                    IsLifeCriticalSnapshot = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CorrectedOnSite = table.Column<bool>(type: "bit", nullable: false),
                    CorrectiveActionRequired = table.Column<bool>(type: "bit", nullable: false),
                    CorrectiveActionDueDate = table.Column<DateTime>(type: "date", nullable: true),
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
                name: "FindingPhoto",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FindingPhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FindingPhoto_AuditQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "audit",
                        principalTable: "AuditQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FindingPhoto_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Weight = table.Column<decimal>(type: "decimal(8,4)", nullable: true),
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
                    FindingId = table.Column<int>(type: "int", nullable: true),
                    AuditId = table.Column<int>(type: "int", nullable: true),
                    QuestionId = table.Column<int>(type: "int", nullable: true),
                    AuditResponseId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    RootCause = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "date", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "date", nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EvidenceRequired = table.Column<bool>(type: "bit", nullable: false),
                    EvidenceReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.CheckConstraint("CK_CorrectiveAction_Status", "[Status] IN ('Open', 'InProgress', 'Closed', 'Voided', 'Overdue')");
                    table.ForeignKey(
                        name: "FK_CorrectiveAction_AuditFinding_FindingId",
                        column: x => x.FindingId,
                        principalSchema: "audit",
                        principalTable: "AuditFinding",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrectiveAction_AuditQuestion_QuestionId",
                        column: x => x.QuestionId,
                        principalSchema: "audit",
                        principalTable: "AuditQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CorrectiveAction_Audit_AuditId",
                        column: x => x.AuditId,
                        principalSchema: "audit",
                        principalTable: "Audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "CaNotificationLog",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrectiveActionId = table.Column<int>(type: "int", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Recipient = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaNotificationLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaNotificationLog_CorrectiveAction_CorrectiveActionId",
                        column: x => x.CorrectiveActionId,
                        principalSchema: "audit",
                        principalTable: "CorrectiveAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CorrectiveActionPhoto",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrectiveActionId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Caption = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrectiveActionPhoto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CorrectiveActionPhoto_CorrectiveAction_CorrectiveActionId",
                        column: x => x.CorrectiveActionId,
                        principalSchema: "audit",
                        principalTable: "CorrectiveAction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_Audit_JobPrefixId",
                schema: "audit",
                table: "Audit",
                column: "JobPrefixId");

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
                name: "IX_Audit_TrackingNumber",
                schema: "audit",
                table: "Audit",
                column: "TrackingNumber",
                unique: true,
                filter: "[TrackingNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditActionLog_EntityType_EntityId",
                schema: "audit",
                table: "AuditActionLog",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditActionLog_PerformedBy",
                schema: "audit",
                table: "AuditActionLog",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AuditActionLog_Timestamp",
                schema: "audit",
                table: "AuditActionLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditAttachment_AuditId",
                schema: "audit",
                table: "AuditAttachment",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditDistributionRecipient_AuditId",
                schema: "audit",
                table: "AuditDistributionRecipient",
                column: "AuditId");

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
                name: "IX_AuditNumberSequence_DivisionId_Year",
                schema: "audit",
                table: "AuditNumberSequence",
                columns: new[] { "DivisionId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditQuestion_ResponseTypeId",
                schema: "audit",
                table: "AuditQuestion",
                column: "ResponseTypeId");

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
                name: "IX_AuditSection_ReportingCategoryId",
                schema: "audit",
                table: "AuditSection",
                column: "ReportingCategoryId");

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
                name: "IX_AuditTrailLog_EntityType_EntityId",
                schema: "audit",
                table: "AuditTrailLog",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailLog_Timestamp",
                schema: "audit",
                table: "AuditTrailLog",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailLog_UserEmail",
                schema: "audit",
                table: "AuditTrailLog",
                column: "UserEmail");

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
                name: "IX_CaNotificationLog_CorrectiveActionId_NotificationType_SentAt",
                schema: "audit",
                table: "CaNotificationLog",
                columns: new[] { "CorrectiveActionId", "NotificationType", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveAction_AuditId",
                schema: "audit",
                table: "CorrectiveAction",
                column: "AuditId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveAction_FindingId",
                schema: "audit",
                table: "CorrectiveAction",
                column: "FindingId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveAction_QuestionId",
                schema: "audit",
                table: "CorrectiveAction",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CorrectiveActionPhoto_CorrectiveActionId",
                schema: "audit",
                table: "CorrectiveActionPhoto",
                column: "CorrectiveActionId");

            migrationBuilder.CreateIndex(
                name: "IX_Division_Code",
                schema: "audit",
                table: "Division",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DivisionJobPrefix_DivisionId_SortOrder",
                schema: "audit",
                table: "DivisionJobPrefix",
                columns: new[] { "DivisionId", "SortOrder" });

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
                name: "IX_FindingPhoto_AuditId_QuestionId",
                schema: "audit",
                table: "FindingPhoto",
                columns: new[] { "AuditId", "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_FindingPhoto_QuestionId",
                schema: "audit",
                table: "FindingPhoto",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterTemplate_DivisionId_IsDefault_IsDeleted",
                schema: "audit",
                table: "NewsletterTemplate",
                columns: new[] { "DivisionId", "IsDefault", "IsDeleted" });

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

            migrationBuilder.CreateIndex(
                name: "IX_ReportDraft_DivisionId_DateFrom_DateTo",
                schema: "audit",
                table: "ReportDraft",
                columns: new[] { "DivisionId", "DateFrom", "DateTo" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportDraft_DivisionId_IsDeleted",
                schema: "audit",
                table: "ReportDraft",
                columns: new[] { "DivisionId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportDraft_UpdatedAt",
                schema: "audit",
                table: "ReportDraft",
                column: "UpdatedAt");

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
                name: "IX_ReviewGroupMember_Email",
                schema: "audit",
                table: "ReviewGroupMember",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledReport_DivisionId",
                schema: "audit",
                table: "ScheduledReport",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledReport_IsActive_NextRunAt",
                schema: "audit",
                table: "ScheduledReport",
                columns: new[] { "IsActive", "NextRunAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Settings_ADAdminsGroupGUID",
                table: "Settings",
                column: "ADAdminsGroupGUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_ADUsersGroupGUID",
                table: "Settings",
                column: "ADUsersGroupGUID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateChangeLog_TemplateVersionId",
                schema: "audit",
                table: "TemplateChangeLog",
                column: "TemplateVersionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_AzureAdObjectId",
                table: "Users",
                column: "AzureAdObjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DisabledById",
                table: "Users",
                column: "DisabledById");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditActionLog",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditAttachment",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditDistributionRecipient",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditEnabledSection",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditHeader",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditNumberSequence",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditResponse",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditTrailLog",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "CaNotificationLog",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "CorrectiveActionPhoto",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "EmailRoutingRule",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "FindingPhoto",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "NewsletterTemplate",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "QuestionLogicRule",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ReportDraft",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ResponseOption",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ReviewGroupMember",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ScheduledReport",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "SystemOwners");

            migrationBuilder.DropTable(
                name: "TechSupports");

            migrationBuilder.DropTable(
                name: "TemplateChangeLog",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "UserDivision",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "CorrectiveAction",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditVersionQuestion",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AuditFinding",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditSection",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditQuestion",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "Audit",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ReportingCategory",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "ResponseType",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditTemplateVersion",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "DivisionJobPrefix",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "AuditTemplate",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "Division",
                schema: "audit");
        }
    }
}
