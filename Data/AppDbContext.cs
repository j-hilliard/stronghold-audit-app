using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Data.Models;
using Stronghold.AppDashboard.Data.Models.Audit;

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
    public DbSet<ReportingCategory> ReportingCategories { get; set; } = null!;
    public DbSet<ResponseType> ResponseTypes { get; set; } = null!;
    public DbSet<ResponseOption> ResponseOptions { get; set; } = null!;
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
    public DbSet<UserDivision> UserDivisions { get; set; } = null!;
    public DbSet<ReportDraft> ReportDrafts { get; set; } = null!;
    public DbSet<NewsletterTemplate> NewsletterTemplates { get; set; } = null!;
    public DbSet<ReviewGroupMember> ReviewGroupMembers { get; set; } = null!;
    public DbSet<CaNotificationLog> CaNotificationLogs { get; set; } = null!;
    public DbSet<AuditEnabledSection> AuditEnabledSections { get; set; } = null!;
    public DbSet<AuditSectionNaOverride> AuditSectionNaOverrides { get; set; } = null!;
    public DbSet<ScheduledReport> ScheduledReports { get; set; } = null!;
    public DbSet<QuestionLogicRule> QuestionLogicRules { get; set; } = null!;
    public DbSet<FindingPhoto> FindingPhotos { get; set; } = null!;
    public DbSet<CorrectiveActionPhoto> CorrectiveActionPhotos { get; set; } = null!;
    public DbSet<AuditDistributionRecipient> AuditDistributionRecipients { get; set; } = null!;
    public DbSet<DivisionJobPrefix> DivisionJobPrefixes { get; set; } = null!;
    public DbSet<AuditNumberSequence> AuditNumberSequences { get; set; } = null!;
    public DbSet<AuditActionLog> AuditActionLogs { get; set; } = null!;
    public DbSet<AuditTrailLog> AuditTrailLogs { get; set; } = null!;
    public DbSet<CaPublicToken> CaPublicTokens { get; set; } = null!;

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

        // ── Audit schema — all tables in the "audit" SQL schema ──────────────

        modelBuilder.Entity<Division>(b =>
        {
            b.ToTable("Division", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Code).IsRequired().HasMaxLength(20);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.AuditType).IsRequired().HasMaxLength(20);
            b.Property(e => e.OPUNumber).HasMaxLength(20);
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
            b.HasIndex(e => new { e.TemplateId, e.VersionNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
            // Filtered unique index: only one Active version per template at a time
            b.HasIndex(e => new { e.TemplateId, e.Status }).IsUnique().HasFilter("[IsDeleted] = 0 AND [Status] = 'Active'");
            b.ToTable("AuditTemplateVersion", "audit", t => t.HasCheckConstraint("CK_AuditTemplateVersion_Status", "[Status] IN ('Draft', 'Active', 'Superseded')"));
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditSection>(b =>
        {
            b.ToTable("AuditSection", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(200);
            b.Property(e => e.SectionCode).HasMaxLength(50);
            b.Property(e => e.OptionalGroupKey).HasMaxLength(100);
            b.Property(e => e.Weight).HasColumnType("decimal(8,4)");
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.TemplateVersion).WithMany(v => v.Sections).HasForeignKey(e => e.TemplateVersionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.ReportingCategory).WithMany(rc => rc.Sections).HasForeignKey(e => e.ReportingCategoryId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditQuestion>(b =>
        {
            b.ToTable("AuditQuestion", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.QuestionText).IsRequired().HasMaxLength(1000);
            b.Property(e => e.ShortLabel).HasMaxLength(200);
            b.Property(e => e.HelpText).HasMaxLength(1000);
            b.Property(e => e.Weight).HasColumnType("decimal(8,4)");
            b.Property(e => e.ArchivedBy).HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.ResponseType).WithMany(rt => rt.Questions).HasForeignKey(e => e.ResponseTypeId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
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
            b.Property(e => e.Weight).HasColumnType("decimal(8,4)");
            b.HasOne(e => e.Section).WithMany(s => s.VersionQuestions).HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Question).WithMany(q => q.VersionQuestions).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => e.TemplateVersionId);
            b.HasIndex(e => e.QuestionId);
            // A question can appear only once per template version
            b.HasIndex(e => new { e.TemplateVersionId, e.QuestionId }).IsUnique().HasFilter("[IsDeleted] = 0");
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
            b.HasOne(e => e.JobPrefix).WithMany().HasForeignKey(e => e.JobPrefixId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(e => e.DivisionId);
            b.HasIndex(e => e.Status);
            b.HasIndex(e => e.CreatedBy);
            b.HasIndex(e => e.TrackingNumber).IsUnique().HasFilter("[TrackingNumber] IS NOT NULL");
            b.Property(e => e.TrackingNumber).HasMaxLength(30);
            b.ToTable("Audit", "audit", t =>
            {
                t.HasCheckConstraint("CK_Audit_Status", "[Status] IN ('Draft', 'Submitted', 'Reopened', 'Closed')");
                t.HasCheckConstraint("CK_Audit_AuditType", "[AuditType] IN ('JobSite', 'Facility')");
            });
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
            b.Property(e => e.SiteCode).HasMaxLength(10);
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
            b.Property(e => e.SectionNameSnapshot).HasMaxLength(200);
            b.Property(e => e.ReportingCategorySnapshot).HasMaxLength(100);
            b.Property(e => e.QuestionWeightSnapshot).HasColumnType("decimal(8,4)");
            b.Property(e => e.SectionWeightSnapshot).HasColumnType("decimal(8,4)");
            b.Property(e => e.CorrectiveActionDueDate)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                    v => v.HasValue ? DateOnly.FromDateTime(v.Value) : (DateOnly?)null)
                .HasColumnType("date");
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Audit).WithMany(a => a.Responses).HasForeignKey(e => e.AuditId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Question).WithMany(q => q.Responses).HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => e.AuditId);
            // One response per question per audit
            b.HasIndex(e => new { e.AuditId, e.QuestionId }).IsUnique().HasFilter("[IsDeleted] = 0");
            b.ToTable("AuditResponse", "audit", t => t.HasCheckConstraint("CK_AuditResponse_Status", "[Status] IS NULL OR [Status] IN ('Conforming', 'NonConforming', 'Warning', 'NA')"));
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
            b.Property(e => e.Source).IsRequired().HasMaxLength(20).HasDefaultValue("Manual");
            b.Property(e => e.Priority).IsRequired().HasMaxLength(20).HasDefaultValue("Normal");
            b.Property(e => e.RootCause).HasMaxLength(2000);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Finding).WithMany(f => f.CorrectiveActions).HasForeignKey(e => e.FindingId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Question).WithMany().HasForeignKey(e => e.QuestionId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Audit).WithMany().HasForeignKey(e => e.AuditId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            b.ToTable("CorrectiveAction", "audit", t =>
            {
                t.HasCheckConstraint("CK_CorrectiveAction_Status",   "[Status]   IN ('Open', 'InProgress', 'Closed', 'Voided', 'Overdue')");
                t.HasCheckConstraint("CK_CorrectiveAction_Source",   "[Source]   IN ('Manual', 'AutoGenerated')");
                t.HasCheckConstraint("CK_CorrectiveAction_Priority", "[Priority] IN ('Normal', 'Urgent')");
            });
            b.HasIndex(e => e.Status);
            b.HasIndex(e => e.AssignedToUserId);
            b.HasIndex(e => e.DueDate);
            b.HasIndex(e => e.AuditResponseId);
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
            // No duplicate email per division
            b.HasIndex(e => new { e.DivisionId, e.EmailAddress }).IsUnique().HasFilter("[IsDeleted] = 0");
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

        modelBuilder.Entity<ReportingCategory>(b =>
        {
            b.ToTable("ReportingCategory", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Code).IsRequired().HasMaxLength(50);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ResponseType>(b =>
        {
            b.ToTable("ResponseType", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Code).IsRequired().HasMaxLength(50);
            b.Property(e => e.Name).IsRequired().HasMaxLength(100);
            b.Property(e => e.Description).HasMaxLength(500);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasIndex(e => e.Code).IsUnique();
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ResponseOption>(b =>
        {
            b.ToTable("ResponseOption", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.OptionLabel).IsRequired().HasMaxLength(100);
            b.Property(e => e.OptionValue).IsRequired().HasMaxLength(50);
            b.Property(e => e.ScoreValue).HasColumnType("decimal(5,4)");
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.ResponseType).WithMany(rt => rt.Options).HasForeignKey(e => e.ResponseTypeId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => new { e.ResponseTypeId, e.OptionValue }).IsUnique().HasFilter("[IsDeleted] = 0");
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<UserDivision>(b =>
        {
            b.ToTable("UserDivision", "audit");
            b.HasKey(e => new { e.UserId, e.DivisionId });
            b.Property(e => e.AssignedBy).IsRequired().HasMaxLength(200);
            b.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<ReportDraft>(b =>
        {
            b.ToTable("ReportDraft", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Title).IsRequired().HasMaxLength(200);
            b.Property(e => e.Period).IsRequired().HasMaxLength(50);
            b.Property(e => e.BlocksJson).IsRequired();
            b.Property(e => e.RowVersion).IsRowVersion();
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            // Primary access pattern: list drafts for a division, excluding soft-deleted
            b.HasIndex(e => new { e.DivisionId, e.IsDeleted });
            // Sort/filter by last modified
            b.HasIndex(e => e.UpdatedAt);
            // Range query support for regeneration
            b.HasIndex(e => new { e.DivisionId, e.DateFrom, e.DateTo });
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<NewsletterTemplate>(b =>
        {
            b.ToTable("NewsletterTemplate", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(200);
            b.Property(e => e.PrimaryColor).IsRequired().HasMaxLength(20);
            b.Property(e => e.AccentColor).IsRequired().HasMaxLength(20);
            b.Property(e => e.CoverImageUrl).HasMaxLength(500);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => new { e.DivisionId, e.IsDefault, e.IsDeleted });
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<ReviewGroupMember>(b =>
        {
            b.ToTable("ReviewGroupMember", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Name).IsRequired().HasMaxLength(200);
            b.Property(e => e.Email).IsRequired().HasMaxLength(200);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.UpdatedBy).HasMaxLength(200);
            b.Property(e => e.DeletedBy).HasMaxLength(200);
            b.HasIndex(e => e.Email).IsUnique().HasFilter("[IsDeleted] = 0");
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<CaNotificationLog>(b =>
        {
            b.ToTable("CaNotificationLog", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.NotificationType).IsRequired().HasMaxLength(20);
            b.Property(e => e.Recipient).IsRequired().HasMaxLength(200);
            b.HasOne(e => e.CorrectiveAction).WithMany().HasForeignKey(e => e.CorrectiveActionId).OnDelete(DeleteBehavior.Cascade);
            // Deduplication index: one log entry per CA + type per calendar day
            b.HasIndex(e => new { e.CorrectiveActionId, e.NotificationType, e.SentAt });
        });

        modelBuilder.Entity<AuditEnabledSection>(b =>
        {
            b.ToTable("AuditEnabledSection", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.OptionalGroupKey).IsRequired().HasMaxLength(100);
            b.HasOne(e => e.Audit).WithMany(a => a.EnabledSections).HasForeignKey(e => e.AuditId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(e => e.AuditId);
            // One entry per group key per audit
            b.HasIndex(e => new { e.AuditId, e.OptionalGroupKey }).IsUnique();
        });

        modelBuilder.Entity<AuditSectionNaOverride>(b =>
        {
            b.ToTable("AuditSectionNaOverride", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.HasOne(e => e.Audit).WithMany(a => a.SectionNaOverrides).HasForeignKey(e => e.AuditId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(e => e.Section).WithMany().HasForeignKey(e => e.SectionId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(e => e.AuditId);
            // One N/A override per section per audit
            b.HasIndex(e => new { e.AuditId, e.SectionId }).IsUnique();
        });

        modelBuilder.Entity<ScheduledReport>(b =>
        {
            b.ToTable("ScheduledReport", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.TemplateId).IsRequired().HasMaxLength(50);
            b.Property(e => e.Title).IsRequired().HasMaxLength(200);
            b.Property(e => e.Frequency).IsRequired().HasMaxLength(20);
            b.Property(e => e.TimeUtc).IsRequired().HasMaxLength(10);
            b.Property(e => e.DateRangePreset).HasMaxLength(50);
            b.Property(e => e.PrimaryColor).HasMaxLength(20);
            b.Property(e => e.ScoreThreshold).HasColumnType("decimal(5,2)");
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasIndex(e => new { e.IsActive, e.NextRunAt });
            b.ToTable("ScheduledReport", "audit", t => t.HasCheckConstraint("CK_ScheduledReport_Frequency", "[Frequency] IN ('Daily', 'Weekly', 'Monthly', 'Quarterly')"));
        });

        modelBuilder.Entity<QuestionLogicRule>(b =>
        {
            b.ToTable("QuestionLogicRule", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.TriggerResponse).IsRequired().HasMaxLength(50);
            b.Property(e => e.Action).IsRequired().HasMaxLength(50);
            b.HasOne(e => e.TemplateVersion).WithMany().HasForeignKey(e => e.TemplateVersionId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(e => e.TriggerVersionQuestion).WithMany().HasForeignKey(e => e.TriggerVersionQuestionId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(e => e.TargetSection).WithMany().HasForeignKey(e => e.TargetSectionId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
            b.HasIndex(e => e.TemplateVersionId);
            b.ToTable("QuestionLogicRule", "audit", t => t.HasCheckConstraint("CK_QuestionLogicRule_Action", "[Action] IN ('HideSection', 'ShowSection')"));
        });

        modelBuilder.Entity<FindingPhoto>(b =>
        {
            b.ToTable("FindingPhoto", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            b.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            b.Property(e => e.UploadedBy).IsRequired().HasMaxLength(256);
            b.Property(e => e.Caption).HasMaxLength(500);
            b.HasOne(e => e.Audit).WithMany().HasForeignKey(e => e.AuditId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne(e => e.Question).WithMany().HasForeignKey(e => e.QuestionId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(e => new { e.AuditId, e.QuestionId });
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<CorrectiveActionPhoto>(b =>
        {
            b.ToTable("CorrectiveActionPhoto", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            b.Property(e => e.FilePath).IsRequired().HasMaxLength(1000);
            b.Property(e => e.UploadedBy).IsRequired().HasMaxLength(256);
            b.Property(e => e.Caption).HasMaxLength(500);
            b.HasOne(e => e.CorrectiveAction).WithMany().HasForeignKey(e => e.CorrectiveActionId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(e => e.CorrectiveActionId);
            b.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<AuditDistributionRecipient>(b =>
        {
            b.ToTable("AuditDistributionRecipient", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.EmailAddress).IsRequired().HasMaxLength(200);
            b.Property(e => e.Name).HasMaxLength(200);
            b.Property(e => e.AddedBy).IsRequired().HasMaxLength(200);
            b.HasOne(e => e.Audit).WithMany().HasForeignKey(e => e.AuditId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(e => e.AuditId);
        });

        modelBuilder.Entity<DivisionJobPrefix>(b =>
        {
            b.ToTable("DivisionJobPrefix", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Prefix).IsRequired().HasMaxLength(5);
            b.Property(e => e.Label).IsRequired().HasMaxLength(100);
            b.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(e => new { e.DivisionId, e.SortOrder });
        });

        modelBuilder.Entity<AuditNumberSequence>(b =>
        {
            b.ToTable("AuditNumberSequence", "audit");
            b.HasKey(e => e.Id);
            b.HasOne(e => e.Division).WithMany().HasForeignKey(e => e.DivisionId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(e => new { e.DivisionId, e.Year }).IsUnique();
        });

        modelBuilder.Entity<AuditActionLog>(b =>
        {
            b.ToTable("AuditActionLog", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.PerformedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.Action).IsRequired().HasMaxLength(100);
            b.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            b.Property(e => e.EntityId).HasMaxLength(50);
            b.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            b.Property(e => e.Severity).IsRequired().HasMaxLength(20).HasDefaultValue("Info");
            b.Property(e => e.IpAddress).HasMaxLength(50);
            b.Property(e => e.RequestPath).HasMaxLength(500);
            b.HasIndex(e => e.Timestamp);
            b.HasIndex(e => e.PerformedBy);
            b.HasIndex(e => new { e.EntityType, e.EntityId });
            b.ToTable("AuditActionLog", "audit", t =>
                t.HasCheckConstraint("CK_AuditActionLog_Severity", "[Severity] IN ('Info', 'Warning', 'Error')"));
        });

        modelBuilder.Entity<AuditTrailLog>(b =>
        {
            b.ToTable("AuditTrailLog", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.UserEmail).IsRequired().HasMaxLength(200);
            b.Property(e => e.Action).IsRequired().HasMaxLength(20);
            b.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            b.Property(e => e.EntityId).IsRequired().HasMaxLength(50);
            b.Property(e => e.IpAddress).HasMaxLength(50);
            b.HasIndex(e => e.Timestamp);
            b.HasIndex(e => e.UserEmail);
            b.HasIndex(e => new { e.EntityType, e.EntityId });
            b.ToTable("AuditTrailLog", "audit", t =>
                t.HasCheckConstraint("CK_AuditTrailLog_Action", "[Action] IN ('Insert', 'Update', 'Delete')"));
        });

        modelBuilder.Entity<CaPublicToken>(b =>
        {
            b.ToTable("CaPublicToken", "audit");
            b.HasKey(e => e.Id);
            b.Property(e => e.Token).IsRequired().HasMaxLength(64);
            b.Property(e => e.CreatedBy).IsRequired().HasMaxLength(200);
            b.Property(e => e.SentToName).HasMaxLength(200);
            b.Property(e => e.SentToEmail).HasMaxLength(200);
            b.HasIndex(e => e.Token).IsUnique();
            b.HasIndex(e => e.CorrectiveActionId);
            b.HasOne(e => e.CorrectiveAction)
             .WithMany()
             .HasForeignKey(e => e.CorrectiveActionId)
             .OnDelete(DeleteBehavior.Cascade);
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
