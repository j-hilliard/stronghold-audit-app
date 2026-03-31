# Database Schema

## Entity Relationship Diagram

```mermaid
erDiagram
    Division {
        int Id PK
        string Code "TKIE, STS, STG, SHI, etc."
        string Name
        string AuditType "JobSite | Facility"
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
    }

    AuditTemplate {
        int Id PK
        string Name
        int DivisionId FK
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
    }

    AuditTemplateVersion {
        int Id PK
        int TemplateId FK
        int VersionNumber
        string Status "Draft | Active | Superseded"
        datetime2 PublishedAt
        string PublishedBy
        int ClonedFromVersionId FK "nullable"
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
    }

    AuditSection {
        int Id PK
        int TemplateVersionId FK
        string Name
        int DisplayOrder
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
    }

    AuditQuestion {
        int Id PK
        string QuestionText
        bool IsArchived
        datetime2 ArchivedAt "nullable"
        string ArchivedBy "nullable"
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
    }

    AuditVersionQuestion {
        int Id PK
        int TemplateVersionId FK
        int SectionId FK
        int QuestionId FK
        int DisplayOrder
        bool AllowNA
        bool RequireCommentOnNC
        bool IsScoreable
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
    }

    Audit {
        int Id PK
        int DivisionId FK
        int TemplateVersionId FK "locked at creation"
        string AuditType "JobSite | Facility"
        string Status "Draft | Submitted | Reopened | Closed"
        datetime2 SubmittedAt "nullable"
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
        datetime2 UpdatedAt
        string UpdatedBy
    }

    AuditHeader {
        int Id PK
        int AuditId FK
        string JobNumber "nullable - JobSite only"
        date Date
        string Client "nullable - JobSite only"
        string Location
        string Unit "nullable"
        string Time "nullable"
        string Shift "DAY | NIGHT - nullable"
        string WorkDesc "nullable"
        string PM "nullable - JobSite only"
        string Auditor
        string Company1 "nullable - Facility only"
        string Company2 "nullable - Facility only"
        string Company3 "nullable - Facility only"
        string ResponsibleParty "nullable - Facility only"
        datetime2 CreatedAt
        string CreatedBy
    }

    AuditResponse {
        int Id PK
        int AuditId FK
        int QuestionId FK
        string QuestionTextSnapshot "exact text at time of audit"
        string Status "Conforming | NonConforming | Warning | NA"
        string Comment "nullable"
        bool CorrectedOnSite
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
        datetime2 UpdatedAt
        string UpdatedBy
    }

    AuditFinding {
        int Id PK
        int AuditId FK
        int QuestionId FK
        string QuestionTextSnapshot
        string Description
        bool CorrectedOnSite
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
    }

    CorrectiveAction {
        int Id PK
        int FindingId FK
        string Description
        date DueDate
        date CompletedDate "nullable"
        string AssignedTo
        string Status "Open | InProgress | Overdue | Closed"
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
        datetime2 UpdatedAt
        string UpdatedBy
    }

    AuditAttachment {
        int Id PK
        int AuditId FK
        string FileName
        string BlobPath
        datetime2 UploadedAt
        string UploadedBy
        bool IsDeleted
    }

    EmailRoutingRule {
        int Id PK
        int DivisionId FK
        string EmailAddress
        bool IsActive
        bool IsDeleted
        datetime2 CreatedAt
        string CreatedBy
    }

    TemplateChangeLog {
        int Id PK
        int TemplateVersionId FK
        string ChangedBy
        datetime2 ChangedAt
        string ChangeType "AddQuestion | RemoveQuestion | Reorder | EditText"
        string ChangeNote
    }

    Division ||--o{ AuditTemplate : "has"
    AuditTemplate ||--o{ AuditTemplateVersion : "has versions"
    AuditTemplateVersion ||--o{ AuditSection : "has sections"
    AuditTemplateVersion ||--o{ AuditVersionQuestion : "has questions"
    AuditSection ||--o{ AuditVersionQuestion : "groups questions"
    AuditQuestion ||--o{ AuditVersionQuestion : "appears in"
    Division ||--o{ Audit : "scopes"
    AuditTemplateVersion ||--o{ Audit : "locked to"
    Audit ||--|| AuditHeader : "has"
    Audit ||--o{ AuditResponse : "has"
    Audit ||--o{ AuditFinding : "generates"
    Audit ||--o{ AuditAttachment : "has"
    AuditQuestion ||--o{ AuditResponse : "answered by"
    AuditQuestion ||--o{ AuditFinding : "referenced by"
    AuditFinding ||--o{ CorrectiveAction : "has"
    Division ||--o{ EmailRoutingRule : "has"
    AuditTemplateVersion ||--o{ TemplateChangeLog : "logged to"
```

---

## Standard Columns on Every Table

Every table in the Audit schema includes these columns — no exceptions:

| Column | Type | Purpose |
|---|---|---|
| `Id` | `int IDENTITY` | Primary key |
| `CreatedAt` | `datetime2 NOT NULL DEFAULT GETUTCDATE()` | When the record was created |
| `CreatedBy` | `nvarchar NOT NULL` | Who created it (Azure AD UPN or dev bypass user) |
| `UpdatedAt` | `datetime2 NULL` | When last modified (null if never updated) |
| `UpdatedBy` | `nvarchar NULL` | Who last modified it |
| `IsDeleted` | `bit NOT NULL DEFAULT 0` | Soft delete flag |
| `DeletedAt` | `datetime2 NULL` | When soft-deleted |
| `DeletedBy` | `nvarchar NULL` | Who soft-deleted it |

`UpdatedAt/By` and `DeletedAt/By` are omitted from lookup/log tables where they are not applicable.

---

## Indexes

| Table | Index | Columns | Reason |
|---|---|---|---|
| `Audit` | `IX_Audit_DivisionId` | DivisionId | Every list query filters by division |
| `Audit` | `IX_Audit_Status` | Status | Constant filter (Draft vs Submitted) |
| `Audit` | `IX_Audit_CreatedBy` | CreatedBy | Auditor-scoped views |
| `AuditResponse` | `IX_AuditResponse_AuditId` | AuditId | Joined on every form load |
| `AuditVersionQuestion` | `IX_AVQ_TemplateVersionId` | TemplateVersionId | Joined on every template load |
| `AuditVersionQuestion` | `IX_AVQ_QuestionId` | QuestionId | Archive lookups |
| `EmailRoutingRule` | `IX_EmailRouting_DivisionId` | DivisionId | Looked up on every submit |
