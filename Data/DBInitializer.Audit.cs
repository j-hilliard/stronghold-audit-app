using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Data;

/// <summary>
/// Seeds all audit schema data: 9 divisions, initial template versions, all questions,
/// and email routing rules migrated from the SHC_Compliance_Audit_Tool.html prototype.
///
/// Idempotent: if Divisions table already has rows, this method returns immediately.
/// Schema governance: this seed runs once on first startup. Changes to questions
/// after go-live must be made through the Template Manager admin UI.
/// </summary>
public static class AuditDbInitializer
{
    private const string System = "system";

    public static void SeedAuditData(AppDbContext dbContext)
    {
        SeedReportingCategories(dbContext);
        SeedResponseTypes(dbContext);

        // IgnoreQueryFilters so soft-deleted rows still count — prevents duplicate seeding
        if (dbContext.Divisions.IgnoreQueryFilters().Any())
            return;

        var now = DateTime.UtcNow;

        // Use execution strategy to support SqlServerRetryingExecutionStrategy with transactions.
        var strategy = dbContext.Database.CreateExecutionStrategy();
        strategy.Execute(() =>
        {
            using var tx = dbContext.Database.BeginTransaction();
            try
            {
                foreach (var divSeed in GetDivisionSeeds())
                    SeedDivision(dbContext, divSeed, now);

                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        });
    }

    private static void SeedReportingCategories(AppDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var categories = new[]
        {
            ("PERMITTING",   "Permitting",                  1),
            ("PPE",          "PPE",                         2),
            ("EQUIPMENT",    "Equipment",                   3),
            ("JOBSITE_CSE",  "Job Site / CSE",              4),
            ("SCAFFOLDS",    "Scaffolds",                   5),
            ("LOTO",         "LOTO",                        6),
            ("JHA",          "JHA / JSA",                   7),
            ("DAILY_LOGS",   "Daily Logs",                  8),
            ("CULTURE",      "Culture / Attitudes",         9),
            ("ENVIRONMENTAL","Environmental",               10),
            ("HAZ_MATERIAL", "Hazardous Material Storage",  11),
            ("CO_SPECIFIC",  "Company Specific",            12),
            ("GENERAL",      "General",                     13),
        };

        foreach (var (code, name, sortOrder) in categories)
        {
            if (!dbContext.ReportingCategories.IgnoreQueryFilters().Any(rc => rc.Code == code))
            {
                dbContext.ReportingCategories.Add(new ReportingCategory
                {
                    Code = code,
                    Name = name,
                    SortOrder = sortOrder,
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = System,
                });
            }
        }
        dbContext.SaveChanges();
    }

    private static void SeedResponseTypes(AppDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var types = new[]
        {
            ("STATUS_CHOICE", "Status Choice", "Conforming / NonConforming / Warning / NA — primary audit response type"),
            ("YES_NO",        "Yes / No",       "Binary yes/no response"),
            ("YES_NO_NA",     "Yes / No / NA",  "Yes, No, or Not Applicable"),
            ("TEXT",          "Text",           "Free-text comment only"),
            ("NUMBER",        "Number",         "Numeric entry"),
            ("DATE",          "Date",           "Date picker"),
        };

        foreach (var (code, name, description) in types)
        {
            if (!dbContext.ResponseTypes.IgnoreQueryFilters().Any(rt => rt.Code == code))
            {
                dbContext.ResponseTypes.Add(new ResponseType
                {
                    Code = code,
                    Name = name,
                    Description = description,
                    CreatedAt = now,
                    CreatedBy = System,
                });
            }
        }
        dbContext.SaveChanges();

        // Seed options for STATUS_CHOICE
        var statusChoice = dbContext.ResponseTypes.FirstOrDefault(rt => rt.Code == "STATUS_CHOICE");
        if (statusChoice == null) return;

        if (!dbContext.ResponseOptions.IgnoreQueryFilters().Any(ro => ro.ResponseTypeId == statusChoice.Id))
        {
            var options = new[]
            {
                new ResponseOption { ResponseTypeId = statusChoice.Id, OptionLabel = "Conforming",    OptionValue = "Conforming",    ScoreValue = 1,    DisplayOrder = 1, IsNegativeFinding = false, TriggersComment = false, TriggersCorrectiveAction = false, CreatedAt = now, CreatedBy = System },
                new ResponseOption { ResponseTypeId = statusChoice.Id, OptionLabel = "Non-Conforming",OptionValue = "NonConforming", ScoreValue = 0,    DisplayOrder = 2, IsNegativeFinding = true,  TriggersComment = true,  TriggersCorrectiveAction = true,  CreatedAt = now, CreatedBy = System },
                new ResponseOption { ResponseTypeId = statusChoice.Id, OptionLabel = "Warning",       OptionValue = "Warning",       ScoreValue = 0,    DisplayOrder = 3, IsNegativeFinding = true,  TriggersComment = false, TriggersCorrectiveAction = false, CreatedAt = now, CreatedBy = System },
                new ResponseOption { ResponseTypeId = statusChoice.Id, OptionLabel = "N/A",           OptionValue = "NA",            ScoreValue = null, DisplayOrder = 4, IsNegativeFinding = false, TriggersComment = false, TriggersCorrectiveAction = false, CreatedAt = now, CreatedBy = System },
            };
            dbContext.ResponseOptions.AddRange(options);
            dbContext.SaveChanges();
        }

        // Seed options for YES_NO
        var yesNo = dbContext.ResponseTypes.FirstOrDefault(rt => rt.Code == "YES_NO");
        if (yesNo != null && !dbContext.ResponseOptions.IgnoreQueryFilters().Any(ro => ro.ResponseTypeId == yesNo.Id))
        {
            dbContext.ResponseOptions.AddRange(
                new ResponseOption { ResponseTypeId = yesNo.Id, OptionLabel = "Yes", OptionValue = "Yes", ScoreValue = 1,    DisplayOrder = 1, IsNegativeFinding = false, TriggersComment = false, TriggersCorrectiveAction = false, CreatedAt = now, CreatedBy = System },
                new ResponseOption { ResponseTypeId = yesNo.Id, OptionLabel = "No",  OptionValue = "No",  ScoreValue = 0,    DisplayOrder = 2, IsNegativeFinding = true,  TriggersComment = true,  TriggersCorrectiveAction = false, CreatedAt = now, CreatedBy = System }
            );
            dbContext.SaveChanges();
        }

        // Seed options for YES_NO_NA
        var yesNoNa = dbContext.ResponseTypes.FirstOrDefault(rt => rt.Code == "YES_NO_NA");
        if (yesNoNa != null && !dbContext.ResponseOptions.IgnoreQueryFilters().Any(ro => ro.ResponseTypeId == yesNoNa.Id))
        {
            dbContext.ResponseOptions.AddRange(
                new ResponseOption { ResponseTypeId = yesNoNa.Id, OptionLabel = "Yes", OptionValue = "Yes", ScoreValue = 1,    DisplayOrder = 1, IsNegativeFinding = false, TriggersComment = false, TriggersCorrectiveAction = false, CreatedAt = now, CreatedBy = System },
                new ResponseOption { ResponseTypeId = yesNoNa.Id, OptionLabel = "No",  OptionValue = "No",  ScoreValue = 0,    DisplayOrder = 2, IsNegativeFinding = true,  TriggersComment = true,  TriggersCorrectiveAction = false, CreatedAt = now, CreatedBy = System },
                new ResponseOption { ResponseTypeId = yesNoNa.Id, OptionLabel = "N/A", OptionValue = "NA",  ScoreValue = null, DisplayOrder = 3, IsNegativeFinding = false, TriggersComment = false, TriggersCorrectiveAction = false, CreatedAt = now, CreatedBy = System }
            );
            dbContext.SaveChanges();
        }
    }

    // ── Core seeding logic ────────────────────────────────────────────────────

    private static void SeedDivision(AppDbContext dbContext, DivisionSeed divSeed, DateTime now)
    {
        // ── Division ──────────────────────────────────────────────────────────
        var division = new Division
        {
            Code = divSeed.Code,
            Name = divSeed.Name,
            AuditType = divSeed.AuditType,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = System
        };
        dbContext.Divisions.Add(division);
        dbContext.SaveChanges(); // need division.Id

        // ── Template ──────────────────────────────────────────────────────────
        var template = new AuditTemplate
        {
            Name = $"{divSeed.Name} Compliance Audit",
            DivisionId = division.Id,
            CreatedAt = now,
            CreatedBy = System
        };
        dbContext.AuditTemplates.Add(template);
        dbContext.SaveChanges(); // need template.Id

        // ── Version ───────────────────────────────────────────────────────────
        var version = new AuditTemplateVersion
        {
            TemplateId = template.Id,
            VersionNumber = 1,
            Status = "Active",
            PublishedAt = now,
            PublishedBy = System,
            CreatedAt = now,
            CreatedBy = System
        };
        dbContext.AuditTemplateVersions.Add(version);
        dbContext.SaveChanges(); // need version.Id

        // ── Sections (batch) ──────────────────────────────────────────────────
        var activeSections = divSeed.Sections
            .Where(s => s.Questions.Length > 0)
            .Select((s, i) => new AuditSection
            {
                TemplateVersionId = version.Id,
                Name = s.Name,
                DisplayOrder = i + 1,
                CreatedAt = now,
                CreatedBy = System
            }).ToList();

        dbContext.AuditSections.AddRange(activeSections);
        dbContext.SaveChanges(); // need section IDs

        // ── Questions + VersionQuestions (batch per section) ──────────────────
        var sectionLookup = activeSections
            .Zip(divSeed.Sections.Where(s => s.Questions.Length > 0), (entity, seed) => (entity, seed))
            .ToList();

        var allVersionQuestions = new List<AuditVersionQuestion>();

        foreach (var (sectionEntity, sectionSeed) in sectionLookup)
        {
            // Batch all question inserts for this section
            var questions = sectionSeed.Questions
                .Select(qText => new AuditQuestion
                {
                    QuestionText = qText,
                    IsArchived = false,
                    CreatedAt = now,
                    CreatedBy = System
                }).ToList();

            dbContext.AuditQuestions.AddRange(questions);
            dbContext.SaveChanges(); // need question IDs

            // Build version-question links
            for (int i = 0; i < questions.Count; i++)
            {
                allVersionQuestions.Add(new AuditVersionQuestion
                {
                    TemplateVersionId = version.Id,
                    SectionId = sectionEntity.Id,
                    QuestionId = questions[i].Id,
                    DisplayOrder = i + 1,
                    AllowNA = true,
                    RequireCommentOnNC = true,
                    IsScoreable = true,
                    CreatedAt = now,
                    CreatedBy = System
                });
            }
        }

        // ── Email routing + VersionQuestions (single batch SaveChanges) ────────
        dbContext.AuditVersionQuestions.AddRange(allVersionQuestions);

        dbContext.EmailRoutingRules.AddRange(divSeed.Emails.Select(email => new EmailRoutingRule
        {
            DivisionId = division.Id,
            EmailAddress = email,
            IsActive = true,
            CreatedAt = now,
            CreatedBy = System
        }));

        dbContext.SaveChanges(); // version questions + email rules together
    }

    // ── Shared review recipients (added to every division) ────────────────────

    private static readonly string[] GlobalReviewEmails = new[]
    {
        "shelby.armstrong@strongholdlimited.com",
        "shausberger@thestrongholdcompanies.com",
        "cwyatt@thestrongholdcompanies.com",
    };

    private static string[] DivisionEmails(params string[] divisionSpecific) =>
        divisionSpecific.Concat(GlobalReviewEmails).ToArray();

    // ── Division seed definitions ─────────────────────────────────────────────

    private static DivisionSeed[] GetDivisionSeeds() => new[]
    {
        new DivisionSeed("TKIE", "TKIE", "JobSite",
            StandardJobSiteSections(
                includeServiceReceipts: true,
                includeCultureAttitudes: true,
                otherQuestions: StandardOtherQuestions()),
            DivisionEmails(
                "shc-tkiecomplianceaudits@quantaservices.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),

        new DivisionSeed("STS", "STS", "JobSite",
            StandardJobSiteSections(
                includeServiceReceipts: true,
                includeCultureAttitudes: true,
                otherQuestions: StandardOtherQuestions()),
            DivisionEmails(
                "shc-stscomplianceaudits@quantaservices.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),

        new DivisionSeed("SHI", "SHI", "JobSite",
            StandardJobSiteSections(
                includeServiceReceipts: true,
                includeCultureAttitudes: true,
                otherQuestions: StandardOtherQuestions()),
            DivisionEmails(
                "shc-shicomplianceaudits@quantaservices.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),

        new DivisionSeed("SHI_RT", "SHI (RT)", "JobSite",
            ShiRtSections(),
            DivisionEmails(
                "shc-shicomplianceaudits@quantaservices.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),

        new DivisionSeed("SHI_RA", "SHI (RA)", "JobSite",
            ShiRaSections(),
            DivisionEmails(
                "shc-shicomplianceaudits@quantaservices.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),

        new DivisionSeed("STG", "STG", "JobSite",
            StgEtsSections(includeExtraOtherQuestions: true),
            DivisionEmails(
                "shc-stgcomplianceaudits@quantaservices.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),

        new DivisionSeed("ETS", "ETS", "JobSite",
            StgEtsSections(includeExtraOtherQuestions: false),
            DivisionEmails(
                "shc-etscomplianceaudits@quantaservices.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),

        new DivisionSeed("CSL", "CSL", "JobSite",
            CslSections(),
            DivisionEmails(
                "cslcomplianceaudits@catspec.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),

        new DivisionSeed("FACILITY", "FACILITY", "Facility",
            FacilitySections(),
            DivisionEmails(
                "cslcomplianceaudits@catspec.com",
                "shc-etscomplianceaudits@quantaservices.com",
                "SU_ComplianceAudits@thestrongholdcompanies.com")),
    };

    // ── Standard job-site section builder ─────────────────────────────────────

    /// <summary>
    /// Builds the standard job-site section list used by TKIE, STS, and SHI.
    /// Caller can append extra sections (e.g. SHI-RA-specific) before "Other".
    /// </summary>
    private static (string Name, string[] Questions)[] StandardJobSiteSections(
        bool includeServiceReceipts,
        bool includeCultureAttitudes,
        string[] otherQuestions,
        (string Name, string[] Questions)[]? extraSections = null)
    {
        var sections = new List<(string Name, string[] Questions)>
        {
            ("Permitting", StandardPermittingQuestions()),
            ("Personal Protective Equipment", StandardPpeQuestions()),
            ("Equipment & Equipment Inspection", StandardEquipmentQuestions()),
            ("Job Site & Confined Space Condition", StandardJobSiteConditionQuestions()),
            ("Scaffolds", StandardScaffoldQuestions()),
            ("Lock-Out / Tag-Out", StandardLotoQuestions()),
            ("Sign-In / Sign-Out Rosters - Toolbox Safety", StandardSignInQuestions()),
            ("Daily Job Logs", new[] {
                "Are daily logs being completed (written or electronically) & detailed?"
            }),
            ("QA / QC Documentation", StandardQaQcQuestions()),
            ("JHA / JSA", StandardJhaJsaQuestions()),
        };

        if (includeServiceReceipts)
            sections.Add(("Service Receipts", new[] {
                "Are service receipts completed & readily available for review?"
            }));

        if (includeCultureAttitudes)
            sections.Add(("Culture / Attitudes", StandardCultureQuestions()));

        if (extraSections != null)
            sections.AddRange(extraSections);

        sections.Add(("Other", otherQuestions));

        return sections.ToArray();
    }

    // ── Standard shared question sets ─────────────────────────────────────────

    private static string[] StandardPermittingQuestions() => new[]
    {
        "Is the Permit Correct & being followed?",
        "Are Emergency Procedures known & followed?",
        "Has a confined space permit been issued?",
        "Is ventilation in place? Confined Space log complete with entrants sign in & out times? Are readings & temperatures documented?",
    };

    private static string[] StandardPpeQuestions() => new[]
    {
        "Are employees complying with all PPE as listed on the permit & as required by Stronghold?",
        "Have employees been trained in the proper use, care, storage and maintenance of the respirators?",
        "Has fall protection been inspected, documented, & color coded for quarter?",
    };

    private static string[] StandardEquipmentQuestions() => new[]
    {
        "Is Heavy Equipment on-site, inspection form completed, & used properly?",
        "Are operators trained, have training cards on person, & has the equipment section on the STRONG card been completed?",
        "Is an Air Monitor on-site with calibration paperwork?",
        "Verify equipment & tools are in good condition for each shift & a documented inspection has been completed. Is proper tool being used for job task? (e.g. proper ladder used for task, does it have safety feet, etc.)",
        "Are tools being returned to tool trailers, not left out in the unit?",
        "Verify that any equipment found defective has been tagged out of service and removed from service.",
        "Are GFCIs being used?",
        "All cords inspected and properly color coded?",
        "Is all Stronghold owned Equipment in good condition?",
        "Is AED on site and monthly inspection completed?",
    };

    private static string[] StandardJobSiteConditionQuestions() => new[]
    {
        "Are all outlets/inlets blinded/isolated?",
        "Is retrieval device, certifications, & rescue plan in place?",
        "All readings are within Stronghold / Client GOPs.",
        "Are communications established and functioning correctly?",
        "Is breathing air on-site with paperwork, in good condition, tested, & documented?",
        "Is the work area properly barricaded with correct signs?",
        "Is Housekeeping being done?",
        "Is it safe for employees?",
    };

    private static string[] StandardScaffoldQuestions() => new[]
    {
        "Have Scaffolds been inspected per shift?",
        "Are Scaffolds properly constructed for load?",
        "Are Scaffolds clean & organized?",
    };

    private static string[] StandardLotoQuestions() => new[]
    {
        "Are all employees locked and/or tagged out?",
        "Are all employees using uniform locks?",
    };

    private static string[] StandardSignInQuestions() => new[]
    {
        "All employees signed in & out with time written.",
        "All other information / Tailgate Safety completed.",
    };

    private static string[] StandardQaQcQuestions() => new[]
    {
        "Are continuity logs & weld maps up to date?",
        "Are all open filler metal containers disposed of properly due to sharp edges & welding rods in rod ovens? Are rod ovens on (if applicable)?",
        "Verify all QA/QC information completed as needed & customer signatures have been obtained (if applicable).",
    };

    private static string[] StandardJhaJsaQuestions() => new[]
    {
        "Is the Master JHA signed by all employees?",
        "Daily JHA/JSA completed and signed as required?",
        "Are SDS Sheets on-site for all Hazardous Materials?",
        "Are STRONG Cards completed, signed, & collected each shift?",
        "Do all employees have current core/company training?",
        "Does E-Chart show the current core/company training?",
        "Are training records on-site?",
        "Are all employees operating within their training level?",
        "Does E-chart correctly reflect man power present at the job site?",
    };

    private static string[] StandardCultureQuestions() => new[]
    {
        "Attitude & Understanding of PM / Superintendent.",
        "Attitude & Understanding of Supervisors.",
        "Attitude & Understanding of Crew.",
        "Are Company owned trucks clean & organized?",
        "Are Trailers & Offices clean & organized?",
        "Is a Safety Conscience Culture coming from the Crew?",
    };

    private static string[] StandardOtherQuestions() => new[]
    {
        "Is cold water & electrolytes readily available for employees?",
        "Are employees taking breaks as needed?",
        "Are there shaded areas or ac areas available where employees can cool down etc. in order to prevent heat stress?",
    };

    // ── STG / ETS (extended permitting, different LOTO, no QA/QC or Service Receipts) ───

    private static (string Name, string[] Questions)[] StgEtsSections(bool includeExtraOtherQuestions)
    {
        var otherQuestions = includeExtraOtherQuestions
            ? StandardOtherQuestions()
            : new[] { "Is cold water & electrolytes readily available for employees?" };

        return new (string, string[])[]
        {
            ("Permitting", new[]
            {
                "Is there a valid work permit for the job description?",
                "Does the permit have all the correct information? (Example: Date, Vessel, Proper PPE required for the job task)",
                "Are all employees signed on the permit that are performing work?",
                "Are Emergency Procedures known & followed?",
                "Has a confined space permit been issued?",
                "Is ventilation in place? Confined Space log complete with entrants sign in & out times? Are readings & temperatures documented?",
            }),
            ("Personal Protective Equipment", new[]
            {
                "Are employees complying with all PPE as listed on the permit & as required by Stronghold?",
                "Have employees been trained in the proper use, care, storage and maintenance of the respirators?",
                "Has fall protection been inspected, documented, & color coded for quarter? Is fall protection donned properly (i.e. leg straps secured) Is employee tied off as required?",
            }),
            ("Equipment & Equipment Inspection", new[]
            {
                "Are tools and equipment used properly with all safe guards in place (i.e. handles on grinders, guards on tools and equipment)? Authorization must be obtained before bypassing a safety system.",
                "Is Heavy Equipment on-site, inspection form completed, & used properly?",
                "Are operators trained, have training cards on person, & has the equipment section on the STRONG card been completed?",
                "Is an Air Monitor on-site with calibration paperwork?",
                "Are tools being returned to tool trailers, not left out in the unit?",
                "Verify that any equipment found defective has been tagged out of service and removed from service.",
                "Are GFCIs being used?",
                "All cords inspected and properly color coded?",
                "Is all Stronghold owned Equipment in good condition?",
                "Is AED on site and monthly inspection completed?",
            }),
            ("Job Site & Confined Space Condition", StandardJobSiteConditionQuestions()),
            ("Scaffolds", StandardScaffoldQuestions()),
            ("Lock-Out / Tag-Out", new[]
            {
                "Is LOTO required per permit?",
                "Are all employees, working on permit, locked and/or tagged out?",
                "Are all employees using uniform locks with tags to identify employee & company? If employees are locking out on satellite box, is there a supervisor/foreman's key inside the box?",
            }),
            ("Sign-In / Sign-Out Rosters - Toolbox Safety", StandardSignInQuestions()),
            ("JHA / JSA", new[]
            {
                "Is the Master JHA signed by all employees?",
                "Daily JHA/JSA completed and signed as required?",
                "Are SDS Sheets on-site for all Hazardous Materials?",
                "Are STRONG Cards completed, signed, & collected each shift?",
            }),
            ("Training / Dispatch", new[]
            {
                "Do all employees have current core/company training?",
                "Are training records on-site?",
                "Are all employees operating within their training level?",
            }),
            ("Other", otherQuestions),
        };
    }

    // ── SHI (RT) — Radiographic Testing ──────────────────────────────────────

    private static (string Name, string[] Questions)[] ShiRtSections() => new (string, string[])[]
    {
        ("Permitting", StandardPermittingQuestions()),
        ("Personal Protective Equipment", StandardPpeQuestions()),
        ("Equipment & Equipment Inspection", StandardEquipmentQuestions()),
        ("Job Site & Confined Space Condition", StandardJobSiteConditionQuestions()),
        ("Scaffolds", StandardScaffoldQuestions()),
        ("Lock-Out / Tag-Out", StandardLotoQuestions()),
        ("Sign-In / Sign-Out Rosters - Toolbox Safety", StandardSignInQuestions()),
        ("JHA / JSA", StandardJhaJsaQuestions()),
        ("Culture / Attitudes", StandardCultureQuestions()),
        ("SHI (RT) Specific", new[]
        {
            "Does the Radiographer/Radiographer Trainee have their required card on their person/truck?",
            "Does the Radiographer/Radiographer Trainee have proper radiation safety PPE on their person? Operable, Calibrated, Alarming Rate Meter, Personal Dosimeter, Mirion Dosimeter (Film Badge)?",
            "Is survey report & SIP (Safety Info Pkg) current & in truck within arms reach? Included in book: Is a copy of the appropriate license, Operating, Safety, and emergency procedures, daily pocket dosimeter records, and certificate of maintenance for Exposure Device - dated within the last six months?",
            "Appropriate signs/barricades per state? High radiation sign around/at the source? Caution signs as the main barricade?",
            "Is a collimator being utilized?",
            "Are 2 survey meters present & calibrated within last 6 months as required? Has battery been inspected? Is there an extra battery for survey meter?",
            "Is the lock engaged on the exposure device?",
            "Is the source locked/physically secured to prevent unauthorized removal, tampering, etc.? (in the truck/not in use)",
            "If the source is being transported (i.e., the RT truck leaves the site), does the exposure device and transport container have proper labeling - Reportable Quantity (RQ) and Radioactive II labels on both sides? Are the labels correctly filled out for the date, contents, and activity in Tera-Becquerels from the source certificate? Are the labels legible? Correct placement on exposure device so as to not cover source information? SHI label legible and in place?",
            "Does the RT truck have the licensee's (SHI) name, city, or town on both sides?",
            "Is vehicle alarm functional and all required locks present?",
            "High radiation signs or cones (4) in truck?",
            "First aid kit & fire extinguisher inspected and in truck?",
        }),
        ("Other", StandardOtherQuestions()),
    };

    // ── SHI (RA) — Rope Access ────────────────────────────────────────────────

    private static (string Name, string[] Questions)[] ShiRaSections()
    {
        var extra = new (string Name, string[] Questions)[]
        {
            ("SHI (RA) Specific", new[]
            {
                "Does Rope Access Technician have log book and it is current?",
                "Has rescue plan been completed?",
                "Has access zone been established?",
                "Has proper anchorage been established and set up correctly?",
                "Has all equipment and PPE been inspected and documented? (ex. technician's harness, ascenders, carabiner, ropes, etc.)",
            }),
        };

        return StandardJobSiteSections(
            includeServiceReceipts: true,
            includeCultureAttitudes: true,
            otherQuestions: StandardOtherQuestions(),
            extraSections: extra);
    }

    // ── CSL — Confined Space with division-specific questions ─────────────────

    private static (string Name, string[] Questions)[] CslSections()
    {
        var extra = new (string Name, string[] Questions)[]
        {
            ("CSL Specific", new[]
            {
                "Are cell phones being carried in the unit?",
                "Are employees wearing proper gloves per task as required?",
                "Are videos labeled correctly? (e.g. Date, Time, Shift, Job#, Equipment, Phase)",
                "Are job logs detailed and complete? (e.g. Timelines, Progress of Work, Delays, etc.)",
                "Module trailers kept clean and orderly?",
                "Do CMOs have air supply in life support module identified for IVTs?",
                "Are company vans clean, organized, & free of damage?",
                "Is the BSI / Blackbox system set-up properly & inspected?",
                "Is a rescue kit on-site, inventoried, & set-up?",
                "Is loading hopper checklist completed?",
                "Is the life support packing list on-site? (If not, list serial numbers of the trailer/cube, helmets, interconnects, umbilical rack & camera Head/Reel)",
                "Do the serial numbers of the helmets, interconnects, & umbilical rack match the packing list?",
            }),
        };

        return StandardJobSiteSections(
            includeServiceReceipts: true,
            includeCultureAttitudes: true,
            otherQuestions: new[] { "Are mitigations in place to prevent heat stress?" },
            extraSections: extra);
    }

    // ── FACILITY — Office/Facility audit (completely different from job-site) ─

    private static (string Name, string[] Questions)[] FacilitySections() => new (string, string[])[]
    {
        ("Required Building Signage", new[]
        {
            "Is the OSHA poster properly displayed?",
            "Is the EEOC Americans with Disabilities Act poster displayed?",
            "Is form 300, Injury and Illness Reporting Form, posted during the month of February?",
            "Are Safety Hazard Notices displayed near where a violation occurred?",
            "Are emergency phone numbers posted?",
            "Are exits properly marked & clear of obstruction?",
            "Do stairs have standard railings and uniform tread height/width?",
            "Is floor loading exceeded?",
            "Are all work areas properly illuminated?",
            "Is there safe clearance for equipment through aisles and doorways?",
            "Are all fire extinguishers accessible and their locations clearly designated? Are all fire extinguishers inspected yearly (by third party company) & monthly (by Stronghold employee) and documented on the inspection tag?",
            "Are First Aid & Eye Wash stations available and periodically inspected and replenished as needed?",
        }),
        ("Housekeeping", new[]
        {
            "Are work areas (including shop, lunch/break rooms, restrooms, and service rooms) kept clean from trash and debris & housekeeping maintained?",
            "Are containers stored, stacked, blocked, and limited in height so they are stable?",
        }),
        ("Environmental", new[]
        {
            "Are there any apparent signs of physical contamination (dead vegetation, noticeable stains on the grounds, oil sheen)?",
            "Are noise levels within acceptable limits?",
            "Is any product damage or deterioration noted?",
            "Are secondary containments used to prevent spills? Portable storage tanks in good condition and no leaks or spills observed? Labels legible? Plugs in place in the event of rains or spills?",
            "Spill kits staged in the event of a spill? Are employees trained on how to use spill kit?",
            "Do municipal waste receptacles have lids and are closed when not in use?",
            "Debris or contaminates noted in the storm drainage system?",
            "Is all elements of the SWPPP (Storm Water Pollution Prevention Plan) being completed as required? (if applicable)",
        }),
        ("Equipment", new[]
        {
            "Are ladders inspected & used properly?",
            "Are motorized vehicles and mechanized equipment inspected daily?",
            "Are trucks and trailers secured from movement during loading and unloading?",
            "Are industrial trucks equipped with flashing lights, horn, overhead guard, capacity sign and fire extinguisher?",
            "Are industrial trucks not in safe operating condition removed from service?",
            "Are only trained personnel allowed to operate industrial trucks?",
            "Are industrial truck operators record of training kept on file?",
            "Is AED inspected monthly?",
            "Is sufficient clearance provided around and between machines to allow for safe operation, set up, servicing, material handling and waste removal?",
            "Are powered tools used with the correct guard, shield or attachment recommended by the manufacturer?",
            "Are all cord connected, electrically operated tools in good condition & inspected?",
            "Are tools, equipment, & rigging inspected, documented & have proper color code for quarter?",
            "Are rotating or moving parts of equipment guarded to prevent physical contact?",
        }),
        ("Hazardous Material Storage", new[]
        {
            "Gas Cylinder: Stored Properly, Prevent Tipping, No Smoke Signs posted, Fuel Gas segregated from other gases, Cylinders periodically checked for corrosion, distortion & cracks, caps in place when not in use, labelled correctly with contents.",
            "Are fuels kept in safety cans & stored in a \"flammables\" storage cabinet?",
            "Are used oil, oily rags, and oil filters disposed of correctly? Are designated labeled containers identified for each?",
            "Are all chemical containers marked with content name and hazard?",
        }),
        ("Personal Protective Equipment", new[]
        {
            "Is proper PPE being worn for job task?",
            "Are approved safety glasses, face shields, etc., provided and worn when operating equipment which might produced flying material, is subject to breakage, or pose a risk of eye injury?",
        }),
        ("QA / QC", new[]
        {
            "Are MTR's being verified, signed & dated (if applicable)?",
            "Heat numbers being transferred before material is cut (if applicable)?",
        }),
        ("Other", StandardOtherQuestions()),
    };

    // ── Record types ──────────────────────────────────────────────────────────

    private record DivisionSeed(
        string Code,
        string Name,
        string AuditType,
        (string Name, string[] Questions)[] Sections,
        string[] Emails);
}
