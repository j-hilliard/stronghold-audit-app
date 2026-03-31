using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Data.Models;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Data.Models.Safety;

namespace Stronghold.AppDashboard.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<Settings> Settings { get; set; } = null!;
    public DbSet<SystemOwner> SystemOwners { get; set; } = null!;
    public DbSet<TechSupport> TechSupports { get; set; } = null!;

    // ── Audit schema ─────────────────────────────────────────────
    public DbSet<Division> Divisions { get; set; } = null!;
    public DbSet<AuditTemplate> AuditTemplates { get; set; } = null!;
    public DbSet<AuditTemplateVersion> AuditTemplateVersions { get; set; } = null!;
    public DbSet<AuditSection> AuditSections { get; set; } = null!;
    public DbSet<AuditQuestion> AuditQuestions { get; set; } = null!;
    public DbSet<AuditVersionQuestion> AuditVersionQuestions { get; set; } = null!;
    public DbSet<Models.Audit.Audit> Audits { get; set; } = null!;
    public DbSet<AuditHeader> AuditHeaders { get; set; } = null!;
    public DbSet<AuditResponse> AuditResponses { get; set; } = null!;
    public DbSet<AuditFinding> AuditFindings { get; set; } = null!;
    public DbSet<CorrectiveAction> CorrectiveActions { get; set; } = null!;
    public DbSet<AuditAttachment> AuditAttachments { get; set; } = null!;
    public DbSet<EmailRoutingRule> EmailRoutingRules { get; set; } = null!;
    public DbSet<TemplateChangeLog> TemplateChangeLogs { get; set; } = null!;

    // Safety schema
    public DbSet<IncidentReport> IncidentReports { get; set; } = null!;
    public DbSet<IncidentEmployeeInvolved> IncidentEmployeesInvolved { get; set; } = null!;
    public DbSet<IncidentAction> IncidentActions { get; set; } = null!;
    public DbSet<IncidentReportReference> IncidentReportReferences { get; set; } = null!;
    public DbSet<RefCompany> Companies { get; set; } = null!;
    public DbSet<RefRegion> Regions { get; set; } = null!;
    public DbSet<RefSeverity> SeveritiesActual { get; set; } = null!;
    public DbSet<RefSeverity> SeveritiesPotential { get; set; } = null!;
    public DbSet<RefReferenceType> ReferenceTypes { get; set; } = null!;
    public DbSet<RefIncidentReportReference> IncidentReportReferenceOptions { get; set; } = null!;
    public DbSet<RefDocType> DocTypeOptions { get; set; } = null!;
    public DbSet<RefInvestigationReference> InvestigationReferenceOptions { get; set; } = null!;
    public DbSet<RefWorkflowState> WorkflowStates { get; set; } = null!;
    public DbSet<ProcessLog> ProcessLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(typeBuilder =>
        {
            typeBuilder.HasKey(user => user.UserId);
            typeBuilder.HasIndex(user => user.AzureAdObjectId).IsUnique();
            typeBuilder.Property(user => user.AzureAdObjectId).IsRequired();
            typeBuilder
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(user => user.DisabledById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(typeBuilder =>
        {
            typeBuilder.HasKey(role => role.RoleId);
            typeBuilder.Property(role => role.Name).IsRequired();
            typeBuilder.Property(role => role.Description).IsRequired();
        });

        modelBuilder.Entity<UserRole>(typeBuilder =>
        {
            typeBuilder.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            typeBuilder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId }).IsUnique();

            // Relationships are inferred, no need to specify foreign keys
            typeBuilder
                .HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .OnDelete(DeleteBehavior.Restrict);

            typeBuilder
                .HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Settings>(typeBuilder =>
        {
            typeBuilder.HasKey(settings => settings.SettingsId);
            typeBuilder.HasIndex(s => s.ADUsersGroupGUID).IsUnique();
            typeBuilder.HasIndex(s => s.ADAdminsGroupGUID).IsUnique();

            typeBuilder.Property(s => s.ADUsersGroupName).IsRequired();
            typeBuilder.Property(s => s.ADUsersGroupGUID).IsRequired();
            typeBuilder.Property(s => s.ADAdminsGroupName).IsRequired();
            typeBuilder.Property(s => s.ADAdminsGroupGUID).IsRequired();
        });

        modelBuilder.Entity<SystemOwner>(typeBuilder =>
        {
            typeBuilder.HasKey(systemOwner => systemOwner.SystemOwnerId);
            typeBuilder.Property(systemOwner => systemOwner.Name).IsRequired();
        });

        modelBuilder.Entity<TechSupport>(typeBuilder =>
        {
            typeBuilder.HasKey(techSupport => techSupport.TechSupportId);
            typeBuilder.Property(techSupport => techSupport.Name).IsRequired();
        });

        // Safety schema — map to Thad's existing tables (no EF migrations)
        modelBuilder.Entity<IncidentReport>(b =>
        {
            b.ToTable("incident_report", "safety");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.IncidentNumber).HasColumnName("incident_number").IsRequired();
            b.Property(r => r.Status).HasColumnName("status").IsRequired();
            b.Property(r => r.IncidentDate).HasColumnName("incident_date");
            b.Property(r => r.CompanyId).HasColumnName("company_id");
            b.Property(r => r.RegionId).HasColumnName("region_id");
            b.Property(r => r.JobNumber).HasColumnName("job_number");
            b.Property(r => r.ClientCode).HasColumnName("client_code");
            b.Property(r => r.PlantCode).HasColumnName("plant_code");
            b.Property(r => r.WorkDescription).HasColumnName("work_description");
            b.Property(r => r.IncidentSummary).HasColumnName("incident_summary");
            b.Property(r => r.IncidentClass).HasColumnName("incident_class");
            b.Property(r => r.SeverityActualCode).HasColumnName("severity_actual_code");
            b.Property(r => r.SeverityPotentialCode).HasColumnName("severity_potential_code");
            b.Property(r => r.HealthSafetyLeaderId).HasColumnName("health_safety_leader_id");
            b.Property(r => r.SeniorOpsLeaderId).HasColumnName("senior_ops_leader_id");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            b.HasOne(r => r.Company).WithMany().HasForeignKey(r => r.CompanyId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(r => r.Region).WithMany().HasForeignKey(r => r.RegionId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(r => r.EmployeesInvolved).WithOne(e => e.IncidentReport).HasForeignKey(e => e.IncidentReportId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(r => r.Actions).WithOne(a => a.IncidentReport).HasForeignKey(a => a.IncidentReportId).OnDelete(DeleteBehavior.Cascade);
            b.HasMany(r => r.References).WithOne(rf => rf.IncidentReport).HasForeignKey(rf => rf.IncidentReportId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IncidentEmployeeInvolved>(b =>
        {
            b.ToTable("incident_employee_involved", "safety");
            b.HasKey(e => e.Id);
            b.Property(e => e.Id).HasColumnName("id");
            b.Property(e => e.IncidentReportId).HasColumnName("incident_report_id");
            b.Property(e => e.EmployeeIdentifier).HasColumnName("employee_identifier");
            b.Property(e => e.EmployeeName).HasColumnName("employee_name");
            b.Property(e => e.InjuryTypeCode).HasColumnName("injury_type_code");
            b.Property(e => e.Recordable).HasColumnName("recordable");
            b.Property(e => e.HoursWorked).HasColumnName("hours_worked");
            b.Property(e => e.CreatedAt).HasColumnName("created_at");
            b.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<IncidentAction>(b =>
        {
            b.ToTable("incident_action", "safety");
            b.HasKey(a => a.Id);
            b.Property(a => a.Id).HasColumnName("id");
            b.Property(a => a.IncidentReportId).HasColumnName("incident_report_id");
            b.Property(a => a.ActionType).HasColumnName("action_type");
            b.Property(a => a.ActionDescription).HasColumnName("action_description");
            b.Property(a => a.AssignedTo).HasColumnName("assigned_to");
            b.Property(a => a.DueDate).HasColumnName("due_date");
            b.Property(a => a.Status).HasColumnName("status");
            b.Property(a => a.ClosedAt).HasColumnName("closed_at");
            b.Property(a => a.CreatedAt).HasColumnName("created_at");
            b.Property(a => a.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<IncidentReportReference>(b =>
        {
            b.ToTable("incident_report_reference", "safety");
            b.HasKey(r => new { r.IncidentReportId, r.ReferenceId });
            b.Property(r => r.IncidentReportId).HasColumnName("incident_report_id");
            b.Property(r => r.ReferenceId).HasColumnName("reference_id");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.HasOne(r => r.Reference).WithMany().HasForeignKey(r => r.ReferenceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefCompany>(b =>
        {
            b.ToTable("ref_company", "safety");
            b.HasKey(c => c.Id);
            b.Property(c => c.Id).HasColumnName("id");
            b.Property(c => c.Code).HasColumnName("code").IsRequired();
            b.Property(c => c.Name).HasColumnName("name").IsRequired();
            b.Property(c => c.NextIncidentNumber).HasColumnName("next_incident_number");
            b.Property(c => c.IsActive).HasColumnName("is_active");
            b.Property(c => c.CreatedAt).HasColumnName("created_at");
            b.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            b.HasMany(c => c.Regions).WithOne(r => r.Company).HasForeignKey(r => r.CompanyId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefRegion>(b =>
        {
            b.ToTable("ref_region", "safety");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.CompanyId).HasColumnName("company_id");
            b.Property(r => r.Code).HasColumnName("code").IsRequired();
            b.Property(r => r.Name).HasColumnName("name").IsRequired();
            b.Property(r => r.IsActive).HasColumnName("is_active");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.SharedTypeEntity<RefSeverity>("SeveritiesActual", b =>
        {
            b.ToTable("ref_severity_actual", "safety");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id");
            b.Property(s => s.Code).HasColumnName("code").IsRequired();
            b.Property(s => s.Name).HasColumnName("name").IsRequired();
            b.Property(s => s.Rank).HasColumnName("rank");
            b.Property(s => s.IsActive).HasColumnName("is_active");
            b.Property(s => s.CreatedAt).HasColumnName("created_at");
            b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.SharedTypeEntity<RefSeverity>("SeveritiesPotential", b =>
        {
            b.ToTable("ref_severity_potential", "safety");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id");
            b.Property(s => s.Code).HasColumnName("code").IsRequired();
            b.Property(s => s.Name).HasColumnName("name").IsRequired();
            b.Property(s => s.Rank).HasColumnName("rank");
            b.Property(s => s.IsActive).HasColumnName("is_active");
            b.Property(s => s.CreatedAt).HasColumnName("created_at");
            b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<RefReferenceType>(b =>
        {
            b.ToTable("ref_reference_type", "safety");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.Code).HasColumnName("code").IsRequired();
            b.Property(r => r.Name).HasColumnName("name").IsRequired();
            b.Property(r => r.AppliesTo).HasColumnName("applies_to").IsRequired();
            b.Property(r => r.IsActive).HasColumnName("is_active");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
            b.HasMany(r => r.IncidentReportReferences).WithOne(i => i.ReferenceType).HasForeignKey(i => i.ReferenceTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefIncidentReportReference>(b =>
        {
            b.ToTable("ref_incident_report_reference", "safety");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.ReferenceTypeId).HasColumnName("reference_type_id");
            b.Property(r => r.Code).HasColumnName("code").IsRequired();
            b.Property(r => r.Name).HasColumnName("name").IsRequired();
            b.Property(r => r.IsActive).HasColumnName("is_active");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<RefDocType>(b =>
        {
            b.ToTable("ref_doc_type", "safety");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.ReferenceTypeId).HasColumnName("reference_type_id");
            b.Property(r => r.Code).HasColumnName("code").IsRequired();
            b.Property(r => r.Name).HasColumnName("name").IsRequired();
            b.Property(r => r.IsActive).HasColumnName("is_active");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<RefInvestigationReference>(b =>
        {
            b.ToTable("ref_investigation_reference", "safety");
            b.HasKey(r => r.Id);
            b.Property(r => r.Id).HasColumnName("id");
            b.Property(r => r.ReferenceTypeId).HasColumnName("reference_type_id");
            b.Property(r => r.Code).HasColumnName("code").IsRequired();
            b.Property(r => r.Name).HasColumnName("name").IsRequired();
            b.Property(r => r.IsActive).HasColumnName("is_active");
            b.Property(r => r.CreatedAt).HasColumnName("created_at");
            b.Property(r => r.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<RefWorkflowState>(b =>
        {
            b.ToTable("ref_workflow_state", "safety");
            b.HasKey(s => s.Id);
            b.Property(s => s.Id).HasColumnName("id");
            b.Property(s => s.Domain).HasColumnName("domain").IsRequired();
            b.Property(s => s.Code).HasColumnName("code").IsRequired();
            b.Property(s => s.Name).HasColumnName("name").IsRequired();
            b.Property(s => s.IsActive).HasColumnName("is_active");
            b.Property(s => s.CreatedAt).HasColumnName("created_at");
            b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<ProcessLog>(b =>
        {
            b.ToTable("process_log", "safety");
            b.HasKey(p => p.Id);
            b.Property(p => p.Id).HasColumnName("id");
            b.Property(p => p.IncidentReportId).HasColumnName("incident_report_id");
            b.Property(p => p.ProcessName).HasColumnName("process_name").IsRequired();
            b.Property(p => p.ProcessType).HasColumnName("process_type").IsRequired();
            b.Property(p => p.LogType).HasColumnName("log_type").IsRequired();
            b.Property(p => p.Message).HasColumnName("message").IsRequired();
            b.Property(p => p.MessageDetail).HasColumnName("message_detail");
            b.Property(p => p.RelatedObject).HasColumnName("related_object");
            b.Property(p => p.RunId).HasColumnName("run_id").IsRequired();
            b.Property(p => p.LoggedAt).HasColumnName("logged_at");
        });

        // ── Audit schema — all tables in the "audit" SQL schema ──────────────

        modelBuilder.Entity<Division>(b =>
        {
            b.ToTable("Division", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Code).IsRequired().HasMaxLength(20);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.AuditType).IsRequired().HasMaxLength(20);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditTemplate>(b =>
        {
            b.ToTable("AuditTemplate", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Division).WithMany(d => d.Templates).HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditTemplateVersion>(b =>
        {
            b.ToTable("AuditTemplateVersion", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Status).IsRequired().HasMaxLength(20);
            b.Property(e => e.PublishedBy).HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Template).WithMany(t => t.Versions).HasForeignKey(e => e.TemplateId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditSection>(b =>
        {
            b.ToTable("AuditSection", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.TemplateVersion).WithMany(v => v.Sections).HasForeignKey(e => e.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditQuestion>(b =>
        {
            b.ToTable("AuditQuestion", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.QuestionText).IsRequired().HasMaxLength(1000);
            b.Property(e => e.ArchivedBy).HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            // Note: no soft-delete query filter on AuditQuestion — archived questions must be
            // queryable for the admin archive view and for historical audit rendering.
        });

        modelBuilder.Entity<AuditVersionQuestion>(b =>
        {
            b.ToTable("AuditVersionQuestion", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.TemplateVersion).WithMany(v => v.VersionQuestions).HasForeignKey(e => e.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Section).WithMany(s => s.VersionQuestions).HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Question).WithMany(q => q.VersionQuestions).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => e.TemplateVersionId);
            b.HasIndex(e => e.QuestionId);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<Models.Audit.Audit>(b =>
        {
            b.ToTable("Audit", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.AuditType).IsRequired().HasMaxLength(20);
            b.Property(e => e.Status).IsRequired().HasMaxLength(20);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.TemplateVersion).WithMany().HasForeignKey(e => e.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Header).WithOne(h => h.Audit).HasForeignKey<AuditHeader>(h => h.AuditId);
            b.HasIndex(e => e.DivisionId);
            b.HasIndex(e => e.Status);
            b.HasIndex(e => e.CreatedBy);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditHeader>(b =>
        {
            b.ToTable("AuditHeader", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.JobNumber).HasMaxLength(50);
            b.Property(e => e.Client).HasMaxLength(200);
            b.Property(e => e.Location).HasMaxLength(200);
            b.Property(e => e.PM).HasMaxLength(200);
            b.Property(e => e.Auditor).HasMaxLength(200);
            b.Property(e => e.AuditDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null)
                .HasColumnType("date");
            b.Property(e => e.Unit).HasMaxLength(100);
            b.Property(e => e.Time).HasMaxLength(20);
            b.Property(e => e.Shift).HasMaxLength(10);
            b.Property(e => e.WorkDescription).HasMaxLength(1000);
            b.Property(e => e.Company1).HasMaxLength(200);
            b.Property(e => e.Company2).HasMaxLength(200);
            b.Property(e => e.Company3).HasMaxLength(200);
            b.Property(e => e.ResponsibleParty).HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditResponse>(b =>
        {
            b.ToTable("AuditResponse", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.QuestionTextSnapshot).IsRequired().HasMaxLength(1000);
            b.Property(e => e.Status).HasMaxLength(20);
            b.Property(e => e.Comment).HasMaxLength(2000);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Audit).WithMany(a => a.Responses).HasForeignKey(e => e.AuditId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Question).WithMany(q => q.Responses).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => e.AuditId);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditFinding>(b =>
        {
            b.ToTable("AuditFinding", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.QuestionTextSnapshot).IsRequired().HasMaxLength(1000);
            b.Property(e => e.Description).HasMaxLength(2000);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Audit).WithMany(a => a.Findings).HasForeignKey(e => e.AuditId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Question).WithMany().HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<CorrectiveAction>(b =>
        {
            b.ToTable("CorrectiveAction", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            b.Property(e => e.AssignedTo).HasMaxLength(200);
            b.Property(e => e.DueDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null)
                .HasColumnType("date");
            b.Property(e => e.CompletedDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null)
                .HasColumnType("date");
            b.Property(e => e.Status).IsRequired().HasMaxLength(20);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Finding).WithMany(f => f.CorrectiveActions).HasForeignKey(e => e.FindingId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditAttachment>(b =>
        {
            b.ToTable("AuditAttachment", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            b.Property(e => e.BlobPath).HasMaxLength(1000);
            b.Property(e => e.UploadedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Audit).WithMany(a => a.Attachments).HasForeignKey(e => e.AuditId).OnDelete(DeleteBehavior.Restrict);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<EmailRoutingRule>(b =>
        {
            b.ToTable("EmailRoutingRule", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.EmailAddress).IsRequired().HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Division).WithMany(d => d.EmailRoutingRules).HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => e.DivisionId);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<TemplateChangeLog>(b =>
        {
            b.ToTable("TemplateChangeLog", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.ChangedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.ChangeType).IsRequired().HasMaxLength(50);
            b.Property(e => e.ChangeNote).HasMaxLength(1000);
            b.HasOne(e => e.TemplateVersion).WithMany(v => v.ChangeLogs).HasForeignKey(e => e.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
            // No soft delete — change logs are permanent audit trail
        });
    }

    private void ApplyAuditInfo()
    {
        var now = DateTimeOffset.UtcNow;
        var addedModifiedEntries = ChangeTracker
            .Entries()
            .Where(e =>
                (
                    e.Entity is Settings
                    || e.Entity is SystemOwner
                    || e.Entity is TechSupport
                    || e.Entity is User
                ) && (e.State == EntityState.Added || e.State == EntityState.Modified)
            );

        foreach (var entry in addedModifiedEntries)
        {
            if (entry.Entity is SystemOwner systemOwnerEntry)
            {
                systemOwnerEntry.ModifiedOn = now;

                if (entry.State == EntityState.Added)
                    systemOwnerEntry.CreatedOn = now;
            }
            else if (entry.Entity is TechSupport techSupportEntry)
            {
                techSupportEntry.ModifiedOn = now;

                if (entry.State == EntityState.Added)
                    techSupportEntry.CreatedOn = now;
            }
            else if (entry.Entity is User userEntry)
            {
                userEntry.ModifiedOn = now;

                if (entry.State == EntityState.Added)
                    userEntry.CreatedOn = now;
            }
            else if (entry.Entity is Settings settingsEntry)
            {
                settingsEntry.ModifiedOn = now;

                if (entry.State == EntityState.Added)
                    settingsEntry.CreatedOn = now;
            }
        }
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInfo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = new()
    )
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }
}
