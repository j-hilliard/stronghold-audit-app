using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Data;

/// <summary>
/// Seeds realistic demo audits for UI/reporting testing.
/// Idempotent — skips if any Audits rows already exist.
/// </summary>
public static class AuditDemoDataSeeder
{
    private const string System = "system";

    public static void SeedDemoAudits(AppDbContext db)
    {
        if (db.Audits.Any(a => a.Status == "Submitted" || a.Status == "Closed"))
            return;

        // Look up divisions and their active template versions by code
        var divisions = db.Divisions
            .Where(d => !d.IsDeleted)
            .ToDictionary(d => d.Code, d => d);

        var versions = db.AuditTemplateVersions
            .Where(v => v.Status == "Active")
            .Join(db.AuditTemplates, v => v.TemplateId, t => t.Id, (v, t) => new { v, t })
            .Join(db.Divisions, x => x.t.DivisionId, d => d.Id, (x, d) => new { x.v, divCode = d.Code })
            .ToDictionary(x => x.divCode, x => x.v);

        // Pull questions grouped by (divisionCode, sectionName) so we can snapshot them
        var questionsByDivSection = db.AuditVersionQuestions
            .Join(db.AuditTemplateVersions.Where(v => v.Status == "Active"), vq => vq.TemplateVersionId, v => v.Id, (vq, v) => new { vq, v })
            .Join(db.AuditTemplates, x => x.v.TemplateId, t => t.Id, (x, t) => new { x.vq, t })
            .Join(db.Divisions, x => x.t.DivisionId, d => d.Id, (x, d) => new { x.vq, divCode = d.Code })
            .Join(db.AuditSections, x => x.vq.SectionId, s => s.Id, (x, s) => new { x.vq, x.divCode, sectionName = s.Name })
            .Join(db.AuditQuestions, x => x.vq.QuestionId, q => q.Id, (x, q) => new { x.divCode, x.sectionName, x.vq, questionText = q.QuestionText, questionId = q.Id })
            .GroupBy(x => new { x.divCode, x.sectionName })
            .ToDictionary(
                g => (g.Key.divCode, g.Key.sectionName),
                g => g.OrderBy(x => x.vq.DisplayOrder).Select(x => (x.questionId, x.questionText)).ToList());

        if (!divisions.TryGetValue("SHI", out var shiDiv) || !versions.TryGetValue("SHI", out var shiVer)) return;
        if (!divisions.TryGetValue("ETS", out var etsDiv) || !versions.TryGetValue("ETS", out var etsVer)) return;
        if (!divisions.TryGetValue("TKIE", out var tkieDiv) || !versions.TryGetValue("TKIE", out var tkieVer)) return;
        if (!divisions.TryGetValue("STS", out var stsDiv) || !versions.TryGetValue("STS", out var stsVer)) return;
        if (!divisions.TryGetValue("STG", out var stgDiv) || !versions.TryGetValue("STG", out var stgVer)) return;

        var audits = new List<(Audit audit, AuditHeader header, List<ResponseSeed> responses)>
        {
            // ── SHI — Q1 audits (Jan-Mar 2026) ───────────────────────────────

            // #1 — SHI, Jan 14, Gray Oak Pipeline, good score
            BuildAudit(shiDiv, shiVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "SHI-2601-042",
                    Client = "Gray Oak Pipeline",
                    PM = "Marcus Webb",
                    Unit = "Unit 4 / Compressor Station",
                    Location = "Corpus Christi, TX",
                    AuditDate = new DateOnly(2026, 1, 14),
                    Auditor = "R. Thibodaux",
                    Shift = "DAY",
                    WorkDescription = "Pipe insulation and scaffold erection on suction side of K-200 compressor",
                },
                submittedAt: new DateTime(2026, 1, 14, 15, 22, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 1, 14, 8, 5, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    // Permitting — all conforming
                    R("SHI", "Permitting", 0, "Conforming"),
                    R("SHI", "Permitting", 1, "Conforming"),
                    R("SHI", "Permitting", 2, "NA"),
                    R("SHI", "Permitting", 3, "NA"),
                    // PPE — 1 warning (hard hat not labeled)
                    R("SHI", "Personal Protective Equipment", 0, "Conforming"),
                    R("SHI", "Personal Protective Equipment", 1, "Conforming"),
                    R("SHI", "Personal Protective Equipment", 2, "Warning", "Fall protection color code sticker worn off on 2 harnesses — replaced same day"),
                    // Equipment — mostly conforming
                    R("SHI", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 1, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 9, "Conforming"),
                    // Job Site
                    R("SHI", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 1, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 7, "Conforming"),
                    // Scaffolds
                    R("SHI", "Scaffolds", 0, "Conforming"),
                    R("SHI", "Scaffolds", 1, "Conforming"),
                    R("SHI", "Scaffolds", 2, "Conforming"),
                    // LOTO
                    R("SHI", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("SHI", "Lock-Out / Tag-Out", 1, "Conforming"),
                    // Sign-in
                    R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    // JHA
                    R("SHI", "JHA / JSA", 0, "Conforming"),
                    R("SHI", "JHA / JSA", 1, "Conforming"),
                    R("SHI", "JHA / JSA", 2, "Conforming"),
                    R("SHI", "JHA / JSA", 3, "Conforming"),
                    R("SHI", "JHA / JSA", 4, "Conforming"),
                    R("SHI", "JHA / JSA", 5, "Conforming"),
                    R("SHI", "JHA / JSA", 6, "Conforming"),
                    R("SHI", "JHA / JSA", 7, "Conforming"),
                    R("SHI", "JHA / JSA", 8, "Conforming"),
                    // Culture
                    R("SHI", "Culture / Attitudes", 0, "Conforming"),
                    R("SHI", "Culture / Attitudes", 1, "Conforming"),
                    R("SHI", "Culture / Attitudes", 2, "Conforming"),
                    R("SHI", "Culture / Attitudes", 3, "Conforming"),
                    R("SHI", "Culture / Attitudes", 4, "Conforming"),
                    R("SHI", "Culture / Attitudes", 5, "Conforming"),
                },
                questionsByDivSection),

            // #2 — SHI, Feb 6, Valero Port Arthur, moderate NCs
            BuildAudit(shiDiv, shiVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "SHI-2602-017",
                    Client = "Valero Port Arthur",
                    PM = "Marcus Webb",
                    Unit = "Unit 7 / Distillation Tower",
                    Location = "Port Arthur, TX",
                    AuditDate = new DateOnly(2026, 2, 6),
                    Auditor = "R. Thibodaux",
                    Shift = "DAY",
                    WorkDescription = "Scaffold build and blasting inside distillation tower T-702",
                },
                submittedAt: new DateTime(2026, 2, 6, 16, 45, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 2, 6, 9, 0, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("SHI", "Permitting", 0, "Conforming"),
                    R("SHI", "Permitting", 1, "Conforming"),
                    R("SHI", "Permitting", 2, "Conforming"),
                    R("SHI", "Permitting", 3, "Conforming"),
                    R("SHI", "Personal Protective Equipment", 0, "NonConforming", "2 employees not wearing splash goggles as required on permit — verbal correction issued, goggles donned immediately", correctedOnSite: true),
                    R("SHI", "Personal Protective Equipment", 1, "Conforming"),
                    R("SHI", "Personal Protective Equipment", 2, "Warning", "1 harness tag expired — tagged out and replaced"),
                    R("SHI", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 1, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 3, "NonConforming", "Air monitor calibration paperwork not on-site — paperwork retrieved from foreman's truck within 30 min", correctedOnSite: true),
                    R("SHI", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 1, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("SHI", "Scaffolds", 0, "Conforming"),
                    R("SHI", "Scaffolds", 1, "Conforming"),
                    R("SHI", "Scaffolds", 2, "Conforming"),
                    R("SHI", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("SHI", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Warning", "Toolbox safety section partially incomplete — 3 signatures missing"),
                    R("SHI", "JHA / JSA", 0, "Conforming"),
                    R("SHI", "JHA / JSA", 1, "Conforming"),
                    R("SHI", "JHA / JSA", 2, "Conforming"),
                    R("SHI", "JHA / JSA", 3, "Conforming"),
                    R("SHI", "JHA / JSA", 4, "Conforming"),
                    R("SHI", "JHA / JSA", 5, "Conforming"),
                    R("SHI", "JHA / JSA", 6, "Conforming"),
                    R("SHI", "JHA / JSA", 7, "Conforming"),
                    R("SHI", "JHA / JSA", 8, "NonConforming", "E-Chart headcount (14) does not match manpower on site (17) — PM notified to update E-Chart"),
                    R("SHI", "Culture / Attitudes", 0, "Conforming"),
                    R("SHI", "Culture / Attitudes", 1, "Conforming"),
                    R("SHI", "Culture / Attitudes", 2, "Conforming"),
                    R("SHI", "Culture / Attitudes", 3, "Conforming"),
                    R("SHI", "Culture / Attitudes", 4, "Conforming"),
                    R("SHI", "Culture / Attitudes", 5, "Conforming"),
                },
                questionsByDivSection),

            // #3 — SHI, Mar 18, Chevron Phillips, poor score — several open CAs
            BuildAudit(shiDiv, shiVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "SHI-2603-091",
                    Client = "Chevron Phillips Cedar Bayou",
                    PM = "Derrick Fontenot",
                    Unit = "Unit 12 / Ethylene Cracker",
                    Location = "Baytown, TX",
                    AuditDate = new DateOnly(2026, 3, 18),
                    Auditor = "C. Wyatt",
                    Shift = "DAY",
                    WorkDescription = "Heat exchanger bundle pull and cleaning — confined space entry",
                },
                submittedAt: new DateTime(2026, 3, 18, 17, 10, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 3, 18, 7, 30, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("SHI", "Permitting", 0, "Conforming"),
                    R("SHI", "Permitting", 1, "Conforming"),
                    R("SHI", "Permitting", 2, "Conforming"),
                    R("SHI", "Permitting", 3, "NonConforming", "Confined space log not on-site — entry in progress with no atmospheric readings documented", caDesc: "Ensure confined space log is on-site before entry begins; crew foreman to acknowledge in writing", caAssignedTo: "Derrick Fontenot", caDaysOffset: -19),
                    R("SHI", "Personal Protective Equipment", 0, "NonConforming", "Employee observed without required Level C suit during initial purging phase", caDesc: "Conduct focused PPE re-briefing with crew; verify all employees have correct chemical ensemble on future permits", caAssignedTo: "Derrick Fontenot", caDaysOffset: -19),
                    R("SHI", "Personal Protective Equipment", 1, "Conforming"),
                    R("SHI", "Personal Protective Equipment", 2, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 1, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 3, "Warning", "Air monitor battery low — swapped out during audit"),
                    R("SHI", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 1, "NonConforming", "Retrieval device not rigged at confined space entry — work halted until retrieval set up", correctedOnSite: true),
                    R("SHI", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 6, "Warning", "Work area not fully barricaded on east approach — tape added during audit"),
                    R("SHI", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("SHI", "Scaffolds", 0, "Conforming"),
                    R("SHI", "Scaffolds", 1, "Conforming"),
                    R("SHI", "Scaffolds", 2, "Conforming"),
                    R("SHI", "Lock-Out / Tag-Out", 0, "NonConforming", "Two employees found working on equipment that was not locked out — work stopped immediately", caDesc: "Complete LOTO refresher training for full crew at Cedar Bayou site; document completion in E-Chart", caAssignedTo: "Derrick Fontenot", caDaysOffset: -19),
                    R("SHI", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("SHI", "JHA / JSA", 0, "Conforming"),
                    R("SHI", "JHA / JSA", 1, "Conforming"),
                    R("SHI", "JHA / JSA", 2, "Conforming"),
                    R("SHI", "JHA / JSA", 3, "Conforming"),
                    R("SHI", "JHA / JSA", 4, "Conforming"),
                    R("SHI", "JHA / JSA", 5, "Conforming"),
                    R("SHI", "JHA / JSA", 6, "Conforming"),
                    R("SHI", "JHA / JSA", 7, "Conforming"),
                    R("SHI", "JHA / JSA", 8, "Conforming"),
                    R("SHI", "Culture / Attitudes", 0, "Warning", "Crew attitude during initial walkthrough was dismissive; improved after PM arrived"),
                    R("SHI", "Culture / Attitudes", 1, "Conforming"),
                    R("SHI", "Culture / Attitudes", 2, "Conforming"),
                    R("SHI", "Culture / Attitudes", 3, "Conforming"),
                    R("SHI", "Culture / Attitudes", 4, "Conforming"),
                    R("SHI", "Culture / Attitudes", 5, "Conforming"),
                },
                questionsByDivSection),

            // ── ETS — 3 audits ────────────────────────────────────────────────

            // #4 — ETS, Jan 28, Enterprise Products, solid score
            BuildAudit(etsDiv, etsVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "ETS-2601-008",
                    Client = "Enterprise Products Partners",
                    PM = "Angela Broussard",
                    Unit = "Unit 3 / NGL Fractionator",
                    Location = "Mont Belvieu, TX",
                    AuditDate = new DateOnly(2026, 1, 28),
                    Auditor = "S. Armstrong",
                    Shift = "DAY",
                    WorkDescription = "Tracing installation and insulation on fractionation columns",
                },
                submittedAt: new DateTime(2026, 1, 28, 16, 0, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 1, 28, 8, 15, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("ETS", "Permitting", 0, "Conforming"),
                    R("ETS", "Permitting", 1, "Conforming"),
                    R("ETS", "Permitting", 2, "Conforming"),
                    R("ETS", "Permitting", 3, "Conforming"),
                    R("ETS", "Permitting", 4, "NA"),
                    R("ETS", "Permitting", 5, "NA"),
                    R("ETS", "Personal Protective Equipment", 0, "Conforming"),
                    R("ETS", "Personal Protective Equipment", 1, "Conforming"),
                    R("ETS", "Personal Protective Equipment", 2, "Warning", "Two employees' harnesses not color-coded for current quarter — corrected on site"),
                    R("ETS", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 1, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 1, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("ETS", "Scaffolds", 0, "Conforming"),
                    R("ETS", "Scaffolds", 1, "Conforming"),
                    R("ETS", "Scaffolds", 2, "Conforming"),
                    R("ETS", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("ETS", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("ETS", "Lock-Out / Tag-Out", 2, "Conforming"),
                    R("ETS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("ETS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("ETS", "JHA / JSA", 0, "Conforming"),
                    R("ETS", "JHA / JSA", 1, "Conforming"),
                    R("ETS", "JHA / JSA", 2, "Conforming"),
                    R("ETS", "JHA / JSA", 3, "Conforming"),
                    R("ETS", "Training / Dispatch", 0, "Conforming"),
                    R("ETS", "Training / Dispatch", 1, "Conforming"),
                    R("ETS", "Training / Dispatch", 2, "Conforming"),
                },
                questionsByDivSection),

            // #5 — ETS, Feb 25, LyondellBasell, several NCs — open CAs
            BuildAudit(etsDiv, etsVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "ETS-2602-031",
                    Client = "LyondellBasell",
                    PM = "Angela Broussard",
                    Unit = "Unit 6 / Polyethylene",
                    Location = "La Porte, TX",
                    AuditDate = new DateOnly(2026, 2, 25),
                    Auditor = "S. Armstrong",
                    Shift = "DAY",
                    WorkDescription = "Vessel cleaning and blind flanging on PE reactor R-601",
                },
                submittedAt: new DateTime(2026, 2, 25, 15, 50, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 2, 25, 7, 45, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("ETS", "Permitting", 0, "Conforming"),
                    R("ETS", "Permitting", 1, "NonConforming", "Permit did not list correct PPE — chemical splash goggles required but not noted; foreman corrected permit", correctedOnSite: true),
                    R("ETS", "Permitting", 2, "Conforming"),
                    R("ETS", "Permitting", 3, "Conforming"),
                    R("ETS", "Permitting", 4, "Conforming"),
                    R("ETS", "Permitting", 5, "Conforming"),
                    R("ETS", "Personal Protective Equipment", 0, "Conforming"),
                    R("ETS", "Personal Protective Equipment", 1, "NonConforming", "Respirator fit-test records not on-site for 3 of 8 employees doing confined space work", caDesc: "Retrieve and file respirator fit-test records for La Porte crew; confirm on-site availability before next entry", caAssignedTo: "Angela Broussard", caDaysOffset: -10),
                    R("ETS", "Personal Protective Equipment", 2, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 1, "NA"),
                    R("ETS", "Equipment & Equipment Inspection", 2, "NA"),
                    R("ETS", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 5, "NonConforming", "Defective angle grinder not tagged out — found in active tool crib with cracked guard", caDesc: "Remove defective grinder from service; implement daily tool inspection checklist for La Porte crew", caAssignedTo: "Angela Broussard", caDaysOffset: -10),
                    R("ETS", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 1, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("ETS", "Scaffolds", 0, "Warning", "Scaffold planks have debris on walking surface — crew swept during audit"),
                    R("ETS", "Scaffolds", 1, "Conforming"),
                    R("ETS", "Scaffolds", 2, "Conforming"),
                    R("ETS", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("ETS", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("ETS", "Lock-Out / Tag-Out", 2, "Conforming"),
                    R("ETS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("ETS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("ETS", "JHA / JSA", 0, "Conforming"),
                    R("ETS", "JHA / JSA", 1, "Conforming"),
                    R("ETS", "JHA / JSA", 2, "Conforming"),
                    R("ETS", "JHA / JSA", 3, "Conforming"),
                    R("ETS", "Training / Dispatch", 0, "Conforming"),
                    R("ETS", "Training / Dispatch", 1, "Conforming"),
                    R("ETS", "Training / Dispatch", 2, "Conforming"),
                },
                questionsByDivSection),

            // #6 — ETS, Mar 30, Enterprise Products — recent, mostly clean
            BuildAudit(etsDiv, etsVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "ETS-2603-055",
                    Client = "Enterprise Products Partners",
                    PM = "Angela Broussard",
                    Unit = "Unit 8 / Butane Splitter",
                    Location = "Chambers County, TX",
                    AuditDate = new DateOnly(2026, 3, 30),
                    Auditor = "R. Thibodaux",
                    Shift = "DAY",
                    WorkDescription = "Valve maintenance and line blinds on butane splitter overhead system",
                },
                submittedAt: new DateTime(2026, 3, 30, 14, 30, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 3, 30, 8, 0, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("ETS", "Permitting", 0, "Conforming"),
                    R("ETS", "Permitting", 1, "Conforming"),
                    R("ETS", "Permitting", 2, "Conforming"),
                    R("ETS", "Permitting", 3, "Conforming"),
                    R("ETS", "Permitting", 4, "NA"),
                    R("ETS", "Permitting", 5, "NA"),
                    R("ETS", "Personal Protective Equipment", 0, "Conforming"),
                    R("ETS", "Personal Protective Equipment", 1, "Conforming"),
                    R("ETS", "Personal Protective Equipment", 2, "Warning", "One employee harness leg strap not fully secured — corrected immediately"),
                    R("ETS", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 1, "NA"),
                    R("ETS", "Equipment & Equipment Inspection", 2, "NA"),
                    R("ETS", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("ETS", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 1, "NA"),
                    R("ETS", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 4, "NA"),
                    R("ETS", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("ETS", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("ETS", "Scaffolds", 0, "NA"),
                    R("ETS", "Scaffolds", 1, "NA"),
                    R("ETS", "Scaffolds", 2, "NA"),
                    R("ETS", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("ETS", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("ETS", "Lock-Out / Tag-Out", 2, "Conforming"),
                    R("ETS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("ETS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("ETS", "JHA / JSA", 0, "Conforming"),
                    R("ETS", "JHA / JSA", 1, "Conforming"),
                    R("ETS", "JHA / JSA", 2, "Conforming"),
                    R("ETS", "JHA / JSA", 3, "Conforming"),
                    R("ETS", "Training / Dispatch", 0, "Conforming"),
                    R("ETS", "Training / Dispatch", 1, "Conforming"),
                    R("ETS", "Training / Dispatch", 2, "Conforming"),
                },
                questionsByDivSection),

            // ── TKIE — 2 audits ───────────────────────────────────────────────

            // #7 — TKIE, Jan 22, Kinder Morgan
            BuildAudit(tkieDiv, tkieVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "TKIE-2601-019",
                    Client = "Kinder Morgan",
                    PM = "Josh Hebert",
                    Unit = "Pipeline Station 7",
                    Location = "Beaumont, TX",
                    AuditDate = new DateOnly(2026, 1, 22),
                    Auditor = "C. Wyatt",
                    Shift = "DAY",
                    WorkDescription = "Cathodic protection installation and pipeline coating inspection",
                },
                submittedAt: new DateTime(2026, 1, 22, 16, 20, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 1, 22, 8, 0, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("TKIE", "Permitting", 0, "Conforming"),
                    R("TKIE", "Permitting", 1, "Conforming"),
                    R("TKIE", "Permitting", 2, "NA"),
                    R("TKIE", "Permitting", 3, "NA"),
                    R("TKIE", "Personal Protective Equipment", 0, "Conforming"),
                    R("TKIE", "Personal Protective Equipment", 1, "Conforming"),
                    R("TKIE", "Personal Protective Equipment", 2, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 0, "NA"),
                    R("TKIE", "Equipment & Equipment Inspection", 1, "NA"),
                    R("TKIE", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 1, "NA"),
                    R("TKIE", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 4, "NA"),
                    R("TKIE", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("TKIE", "Scaffolds", 0, "NA"),
                    R("TKIE", "Scaffolds", 1, "NA"),
                    R("TKIE", "Scaffolds", 2, "NA"),
                    R("TKIE", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("TKIE", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("TKIE", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("TKIE", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("TKIE", "Daily Job Logs", 0, "Conforming"),
                    R("TKIE", "QA / QC Documentation", 0, "Conforming"),
                    R("TKIE", "QA / QC Documentation", 1, "Conforming"),
                    R("TKIE", "QA / QC Documentation", 2, "Conforming"),
                    R("TKIE", "JHA / JSA", 0, "Conforming"),
                    R("TKIE", "JHA / JSA", 1, "Conforming"),
                    R("TKIE", "JHA / JSA", 2, "Conforming"),
                    R("TKIE", "JHA / JSA", 3, "Conforming"),
                    R("TKIE", "JHA / JSA", 4, "Conforming"),
                    R("TKIE", "JHA / JSA", 5, "Conforming"),
                    R("TKIE", "JHA / JSA", 6, "Conforming"),
                    R("TKIE", "JHA / JSA", 7, "Conforming"),
                    R("TKIE", "JHA / JSA", 8, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 0, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 1, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 2, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 3, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 4, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 5, "Conforming"),
                },
                questionsByDivSection),

            // #8 — TKIE, Mar 5, Plains All American — some NCs
            BuildAudit(tkieDiv, tkieVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "TKIE-2603-044",
                    Client = "Plains All American Pipeline",
                    PM = "Josh Hebert",
                    Unit = "Pump Station 12",
                    Location = "Victoria, TX",
                    AuditDate = new DateOnly(2026, 3, 5),
                    Auditor = "C. Wyatt",
                    Shift = "DAY",
                    WorkDescription = "Pump overhaul and mechanical seal replacement on PS-12-200 series",
                },
                submittedAt: new DateTime(2026, 3, 5, 15, 0, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 3, 5, 7, 30, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("TKIE", "Permitting", 0, "Conforming"),
                    R("TKIE", "Permitting", 1, "Conforming"),
                    R("TKIE", "Permitting", 2, "NA"),
                    R("TKIE", "Permitting", 3, "NA"),
                    R("TKIE", "Personal Protective Equipment", 0, "NonConforming", "3 employees wearing non-rated safety glasses in chemical splash zone — stopped and replaced with goggles", correctedOnSite: true),
                    R("TKIE", "Personal Protective Equipment", 1, "Conforming"),
                    R("TKIE", "Personal Protective Equipment", 2, "Warning", "Color code sticker missing from one harness"),
                    R("TKIE", "Equipment & Equipment Inspection", 0, "NA"),
                    R("TKIE", "Equipment & Equipment Inspection", 1, "NA"),
                    R("TKIE", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("TKIE", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 1, "NA"),
                    R("TKIE", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 4, "NA"),
                    R("TKIE", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("TKIE", "Job Site & Confined Space Condition", 6, "NonConforming", "Barricade tape missing on south side of excavation — installed during audit", correctedOnSite: true),
                    R("TKIE", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("TKIE", "Scaffolds", 0, "NA"),
                    R("TKIE", "Scaffolds", 1, "NA"),
                    R("TKIE", "Scaffolds", 2, "NA"),
                    R("TKIE", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("TKIE", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("TKIE", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("TKIE", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("TKIE", "Daily Job Logs", 0, "Conforming"),
                    R("TKIE", "QA / QC Documentation", 0, "Conforming"),
                    R("TKIE", "QA / QC Documentation", 1, "Conforming"),
                    R("TKIE", "QA / QC Documentation", 2, "Conforming"),
                    R("TKIE", "JHA / JSA", 0, "Conforming"),
                    R("TKIE", "JHA / JSA", 1, "Conforming"),
                    R("TKIE", "JHA / JSA", 2, "Conforming"),
                    R("TKIE", "JHA / JSA", 3, "Conforming"),
                    R("TKIE", "JHA / JSA", 4, "Conforming"),
                    R("TKIE", "JHA / JSA", 5, "Conforming"),
                    R("TKIE", "JHA / JSA", 6, "Conforming"),
                    R("TKIE", "JHA / JSA", 7, "Conforming"),
                    R("TKIE", "JHA / JSA", 8, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 0, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 1, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 2, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 3, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 4, "Conforming"),
                    R("TKIE", "Culture / Attitudes", 5, "Conforming"),
                },
                questionsByDivSection),

            // ── STS — 2 audits ────────────────────────────────────────────────

            // #9 — STS, Feb 12, Motiva Port Arthur
            BuildAudit(stsDiv, stsVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "STS-2602-006",
                    Client = "Motiva Enterprises",
                    PM = "Kevin Melancon",
                    Unit = "Unit 22 / Crude Desalter",
                    Location = "Port Arthur, TX",
                    AuditDate = new DateOnly(2026, 2, 12),
                    Auditor = "S. Guillory",
                    Shift = "DAY",
                    WorkDescription = "Desalter vessel internal cleaning and mixer element replacement",
                },
                submittedAt: new DateTime(2026, 2, 12, 16, 10, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 2, 12, 8, 30, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("STS", "Permitting", 0, "Conforming"),
                    R("STS", "Permitting", 1, "Conforming"),
                    R("STS", "Permitting", 2, "Conforming"),
                    R("STS", "Permitting", 3, "Conforming"),
                    R("STS", "Personal Protective Equipment", 0, "Conforming"),
                    R("STS", "Personal Protective Equipment", 1, "Warning", "Respirator storage in hard hat bag exposed to direct sunlight — relocated to trailer"),
                    R("STS", "Personal Protective Equipment", 2, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 1, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 1, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("STS", "Scaffolds", 0, "Conforming"),
                    R("STS", "Scaffolds", 1, "Conforming"),
                    R("STS", "Scaffolds", 2, "Conforming"),
                    R("STS", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("STS", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("STS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("STS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("STS", "Daily Job Logs", 0, "Conforming"),
                    R("STS", "QA / QC Documentation", 0, "Conforming"),
                    R("STS", "QA / QC Documentation", 1, "Conforming"),
                    R("STS", "QA / QC Documentation", 2, "Conforming"),
                    R("STS", "JHA / JSA", 0, "Conforming"),
                    R("STS", "JHA / JSA", 1, "Conforming"),
                    R("STS", "JHA / JSA", 2, "Conforming"),
                    R("STS", "JHA / JSA", 3, "Conforming"),
                    R("STS", "JHA / JSA", 4, "Conforming"),
                    R("STS", "JHA / JSA", 5, "Conforming"),
                    R("STS", "JHA / JSA", 6, "Conforming"),
                    R("STS", "JHA / JSA", 7, "Conforming"),
                    R("STS", "JHA / JSA", 8, "Conforming"),
                    R("STS", "Culture / Attitudes", 0, "Conforming"),
                    R("STS", "Culture / Attitudes", 1, "Conforming"),
                    R("STS", "Culture / Attitudes", 2, "Conforming"),
                    R("STS", "Culture / Attitudes", 3, "Conforming"),
                    R("STS", "Culture / Attitudes", 4, "Conforming"),
                    R("STS", "Culture / Attitudes", 5, "Conforming"),
                },
                questionsByDivSection),

            // #10 — STS, Mar 24, ExxonMobil Baytown — NC with open CA
            BuildAudit(stsDiv, stsVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "STS-2603-023",
                    Client = "ExxonMobil Baytown Complex",
                    PM = "Kevin Melancon",
                    Unit = "Unit 9 / Sulfur Recovery",
                    Location = "Baytown, TX",
                    AuditDate = new DateOnly(2026, 3, 24),
                    Auditor = "S. Guillory",
                    Shift = "DAY",
                    WorkDescription = "Refractory repair in sulfur recovery unit SRU-901",
                },
                submittedAt: new DateTime(2026, 3, 24, 15, 45, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 3, 24, 8, 0, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("STS", "Permitting", 0, "Conforming"),
                    R("STS", "Permitting", 1, "Conforming"),
                    R("STS", "Permitting", 2, "NA"),
                    R("STS", "Permitting", 3, "Conforming"),
                    R("STS", "Personal Protective Equipment", 0, "Conforming"),
                    R("STS", "Personal Protective Equipment", 1, "Conforming"),
                    R("STS", "Personal Protective Equipment", 2, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 1, "NA"),
                    R("STS", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 6, "NonConforming", "GFCI receptacle missing on two drop cords near refractory anchor point — work stopped in that zone", caDesc: "Replace missing GFCI receptacles and conduct electrical inspection of all drop cords for SRU-901 job", caAssignedTo: "Kevin Melancon", caDaysOffset: -13),
                    R("STS", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("STS", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 1, "NA"),
                    R("STS", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 4, "NA"),
                    R("STS", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("STS", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("STS", "Scaffolds", 0, "Conforming"),
                    R("STS", "Scaffolds", 1, "Conforming"),
                    R("STS", "Scaffolds", 2, "Conforming"),
                    R("STS", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("STS", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("STS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("STS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("STS", "Daily Job Logs", 0, "Conforming"),
                    R("STS", "QA / QC Documentation", 0, "Conforming"),
                    R("STS", "QA / QC Documentation", 1, "Conforming"),
                    R("STS", "QA / QC Documentation", 2, "Conforming"),
                    R("STS", "JHA / JSA", 0, "Conforming"),
                    R("STS", "JHA / JSA", 1, "Conforming"),
                    R("STS", "JHA / JSA", 2, "Conforming"),
                    R("STS", "JHA / JSA", 3, "Conforming"),
                    R("STS", "JHA / JSA", 4, "Conforming"),
                    R("STS", "JHA / JSA", 5, "Conforming"),
                    R("STS", "JHA / JSA", 6, "Conforming"),
                    R("STS", "JHA / JSA", 7, "Conforming"),
                    R("STS", "JHA / JSA", 8, "Conforming"),
                    R("STS", "Culture / Attitudes", 0, "Conforming"),
                    R("STS", "Culture / Attitudes", 1, "Conforming"),
                    R("STS", "Culture / Attitudes", 2, "Conforming"),
                    R("STS", "Culture / Attitudes", 3, "Conforming"),
                    R("STS", "Culture / Attitudes", 4, "Conforming"),
                    R("STS", "Culture / Attitudes", 5, "Conforming"),
                },
                questionsByDivSection),

            // ── STG — 2 audits ────────────────────────────────────────────────

            // #11 — STG, Feb 3, Marathon Galveston Bay
            BuildAudit(stgDiv, stgVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "STG-2602-014",
                    Client = "Marathon Petroleum Galveston Bay",
                    PM = "Travis Gaspard",
                    Unit = "Unit 5 / Fluid Catalytic Cracker",
                    Location = "Texas City, TX",
                    AuditDate = new DateOnly(2026, 2, 3),
                    Auditor = "R. Thibodaux",
                    Shift = "DAY",
                    WorkDescription = "FCC catalyst handling and cyclone inspection during turnaround",
                },
                submittedAt: new DateTime(2026, 2, 3, 16, 55, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 2, 3, 7, 30, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("STG", "Permitting", 0, "Conforming"),
                    R("STG", "Permitting", 1, "Conforming"),
                    R("STG", "Permitting", 2, "Conforming"),
                    R("STG", "Permitting", 3, "Conforming"),
                    R("STG", "Permitting", 4, "Conforming"),
                    R("STG", "Permitting", 5, "Conforming"),
                    R("STG", "Personal Protective Equipment", 0, "Conforming"),
                    R("STG", "Personal Protective Equipment", 1, "Conforming"),
                    R("STG", "Personal Protective Equipment", 2, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 1, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 1, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("STG", "Scaffolds", 0, "Conforming"),
                    R("STG", "Scaffolds", 1, "Conforming"),
                    R("STG", "Scaffolds", 2, "Conforming"),
                    R("STG", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("STG", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("STG", "Lock-Out / Tag-Out", 2, "Conforming"),
                    R("STG", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("STG", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("STG", "JHA / JSA", 0, "Conforming"),
                    R("STG", "JHA / JSA", 1, "Conforming"),
                    R("STG", "JHA / JSA", 2, "Conforming"),
                    R("STG", "JHA / JSA", 3, "Conforming"),
                    R("STG", "Training / Dispatch", 0, "Conforming"),
                    R("STG", "Training / Dispatch", 1, "Conforming"),
                    R("STG", "Training / Dispatch", 2, "Conforming"),
                },
                questionsByDivSection),

            // #12 — STG, Mar 11, Valero Texas City — NC with open CA
            BuildAudit(stgDiv, stgVer, "JobSite",
                header: new AuditHeader
                {
                    JobNumber = "STG-2603-029",
                    Client = "Valero Texas City",
                    PM = "Travis Gaspard",
                    Unit = "Unit 2 / Alkylation",
                    Location = "Texas City, TX",
                    AuditDate = new DateOnly(2026, 3, 11),
                    Auditor = "S. Armstrong",
                    Shift = "NIGHT",
                    WorkDescription = "HF alkylation unit scaffolding and maintenance on acid settler V-201",
                },
                submittedAt: new DateTime(2026, 3, 11, 6, 15, 0, DateTimeKind.Utc),
                createdAt: new DateTime(2026, 3, 10, 19, 0, 0, DateTimeKind.Utc),
                responses: new List<ResponseSeed>
                {
                    R("STG", "Permitting", 0, "NonConforming", "Night shift permit had incorrect start time — permit voided and reissued before work resumed", correctedOnSite: true),
                    R("STG", "Permitting", 1, "Conforming"),
                    R("STG", "Permitting", 2, "Conforming"),
                    R("STG", "Permitting", 3, "Conforming"),
                    R("STG", "Permitting", 4, "NA"),
                    R("STG", "Permitting", 5, "Conforming"),
                    R("STG", "Personal Protective Equipment", 0, "Conforming"),
                    R("STG", "Personal Protective Equipment", 1, "Conforming"),
                    R("STG", "Personal Protective Equipment", 2, "Warning", "1 employee's fall protection harness D-ring worn — replaced before re-entry"),
                    R("STG", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 1, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("STG", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 1, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("STG", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("STG", "Scaffolds", 0, "Conforming"),
                    R("STG", "Scaffolds", 1, "Conforming"),
                    R("STG", "Scaffolds", 2, "NonConforming", "Scaffold platform on level 3 not fully planked — gap of approx 14 inches on north face", caDesc: "Complete scaffold planking on V-201 Level 3 north face; document re-inspection by scaffold competent person", caAssignedTo: "Travis Gaspard", caDaysOffset: -26),
                    R("STG", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("STG", "Lock-Out / Tag-Out", 1, "Conforming"),
                    R("STG", "Lock-Out / Tag-Out", 2, "Conforming"),
                    R("STG", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("STG", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    R("STG", "JHA / JSA", 0, "Conforming"),
                    R("STG", "JHA / JSA", 1, "Conforming"),
                    R("STG", "JHA / JSA", 2, "Conforming"),
                    R("STG", "JHA / JSA", 3, "Conforming"),
                    R("STG", "Training / Dispatch", 0, "Conforming"),
                    R("STG", "Training / Dispatch", 1, "Conforming"),
                    R("STG", "Training / Dispatch", 2, "Conforming"),
                },
                questionsByDivSection),
        };

        // Persist each audit
        var now = DateTime.UtcNow;
        foreach (var (audit, header, responses) in audits)
        {
            db.Audits.Add(audit);
            db.SaveChanges(); // need audit.Id

            header.AuditId = audit.Id;
            header.CreatedAt = audit.CreatedAt;
            header.CreatedBy = System;
            db.AuditHeaders.Add(header);

            var findingsSaved = new List<(AuditFinding finding, string? caDesc, string? caAssignedTo, int? caDaysOffset)>();

            foreach (var rs in responses)
            {
                var (divCode, sectionName, qIdx, status, comment, correctedOnSite, caDesc, caAssignedTo, caDaysOffset) = rs;
                if (!questionsByDivSection.TryGetValue((divCode, sectionName), out var qList) || qIdx >= qList.Count)
                    continue;

                var (questionId, questionText) = qList[qIdx];

                var response = new AuditResponse
                {
                    AuditId = audit.Id,
                    QuestionId = questionId,
                    QuestionTextSnapshot = questionText,
                    SectionNameSnapshot = sectionName,
                    Status = status,
                    Comment = comment,
                    CorrectedOnSite = correctedOnSite,
                    CorrectiveActionRequired = status == "NonConforming" && caDesc != null,
                    CreatedAt = audit.CreatedAt,
                    CreatedBy = System,
                };
                db.AuditResponses.Add(response);

                if (status == "NonConforming")
                {
                    var finding = new AuditFinding
                    {
                        AuditId = audit.Id,
                        QuestionId = questionId,
                        QuestionTextSnapshot = questionText,
                        Description = comment,
                        CorrectedOnSite = correctedOnSite,
                        CreatedAt = audit.CreatedAt,
                        CreatedBy = System,
                    };
                    db.AuditFindings.Add(finding);
                    findingsSaved.Add((finding, caDesc, caAssignedTo, caDaysOffset));
                }
            }

            db.SaveChanges(); // responses + findings

            // Corrective actions (need finding IDs)
            foreach (var (finding, caDesc, caAssignedTo, caDaysOffset) in findingsSaved)
            {
                if (caDesc == null) continue;

                var dueDate = DateOnly.FromDateTime(audit.CreatedAt.AddDays(14)); // standard 14-day rule
                var caCreatedAt = audit.CreatedAt;

                // Negative offset = was due in the past (overdue)
                string caStatus = "Open";

                db.CorrectiveActions.Add(new CorrectiveAction
                {
                    FindingId = finding.Id,
                    AuditId = audit.Id,
                    Description = caDesc,
                    AssignedTo = caAssignedTo,
                    DueDate = dueDate,
                    Status = caStatus,
                    EvidenceRequired = false,
                    CreatedAt = caCreatedAt,
                    CreatedBy = System,
                });
            }

            db.SaveChanges();
        }
    }

    // ── Builders ──────────────────────────────────────────────────────────────

    private static (Audit audit, AuditHeader header, List<ResponseSeed> responses) BuildAudit(
        Models.Audit.Division div,
        Models.Audit.AuditTemplateVersion ver,
        string auditType,
        AuditHeader header,
        DateTime submittedAt,
        DateTime createdAt,
        List<ResponseSeed> responses,
        Dictionary<(string, string), List<(int, string)>> _)
    {
        var audit = new Audit
        {
            DivisionId = div.Id,
            TemplateVersionId = ver.Id,
            AuditType = auditType,
            Status = "Submitted",
            SubmittedAt = submittedAt,
            CreatedAt = createdAt,
            CreatedBy = System,
        };
        return (audit, header, responses);
    }

    private static ResponseSeed R(
        string divCode,
        string section,
        int qIdx,
        string status,
        string? comment = null,
        bool correctedOnSite = false,
        string? caDesc = null,
        string? caAssignedTo = null,
        int? caDaysOffset = null)
        => new(divCode, section, qIdx, status, comment, correctedOnSite, caDesc, caAssignedTo, caDaysOffset);

    private record ResponseSeed(
        string DivCode,
        string SectionName,
        int QIdx,
        string Status,
        string? Comment,
        bool CorrectedOnSite,
        string? CaDesc,
        string? CaAssignedTo,
        int? CaDaysOffset);
}
