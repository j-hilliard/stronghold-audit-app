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
        divisions.TryGetValue("CSL", out var cslDiv); versions.TryGetValue("CSL", out var cslVer);
        divisions.TryGetValue("FACILITY", out var facilityDiv); versions.TryGetValue("FACILITY", out var facilityVer);
        divisions.TryGetValue("SHI_RT", out var shirtDiv); versions.TryGetValue("SHI_RT", out var shirtVer);
        divisions.TryGetValue("SHI_RA", out var shiraDiv); versions.TryGetValue("SHI_RA", out var shiraVer);

        var audits = new List<(Audit audit, AuditHeader header, List<ResponseSeed> responses)>
        {
            // ── SHI — Q1 audits (Jan-Mar 2026) ───────────────────────────────

            // #1 — SHI, Jan 14, Gray Oak Pipeline — POOR start to Q1 (7 NCs, score ~83%)
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
                    // Permitting — 2 NCs
                    R("SHI", "Permitting", 0, "NonConforming", "Hot work permit expired at time of audit — crew working without valid permit, work halted", caDesc: "Re-train crew on permit verification before work start; require foreman sign-off on permit expiry check", caAssignedTo: "Marcus Webb", caDaysOffset: -80),
                    R("SHI", "Permitting", 1, "Conforming"),
                    R("SHI", "Permitting", 2, "NonConforming", "MSDS sheets for two chemicals not on site — retrieved after 45-minute delay"),
                    R("SHI", "Permitting", 3, "NA"),
                    // PPE — 2 NCs
                    R("SHI", "Personal Protective Equipment", 0, "NonConforming", "3 employees not wearing required face shields for grinding operations — corrected on site", correctedOnSite: true),
                    R("SHI", "Personal Protective Equipment", 1, "Conforming"),
                    R("SHI", "Personal Protective Equipment", 2, "NonConforming", "Fall protection harness on employee #4 failed inspection — tagged out and replaced"),
                    // Equipment — 1 NC
                    R("SHI", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 1, "NonConforming", "Crane inspection paperwork missing — crane placed out of service until docs produced", caDesc: "Maintain complete equipment inspection binder on-site at all times for all rigging equipment", caAssignedTo: "Marcus Webb", caDaysOffset: -80),
                    R("SHI", "Equipment & Equipment Inspection", 2, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 3, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 9, "Conforming"),
                    // Job Site — 1 NC
                    R("SHI", "Job Site & Confined Space Condition", 0, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 1, "NonConforming", "Barricade tape down on north side — scaffold access uncontrolled; corrected during audit", correctedOnSite: true),
                    R("SHI", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 4, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 7, "Conforming"),
                    // Scaffolds — 1 NC
                    R("SHI", "Scaffolds", 0, "NonConforming", "Scaffold platform missing mid-rail on south elevation — work stopped, rail installed", correctedOnSite: true),
                    R("SHI", "Scaffolds", 1, "Conforming"),
                    R("SHI", "Scaffolds", 2, "Conforming"),
                    // LOTO — conforming
                    R("SHI", "Lock-Out / Tag-Out", 0, "Conforming"),
                    R("SHI", "Lock-Out / Tag-Out", 1, "Conforming"),
                    // Sign-in — conforming
                    R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
                    R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
                    // JHA — conforming
                    R("SHI", "JHA / JSA", 0, "Conforming"),
                    R("SHI", "JHA / JSA", 1, "Conforming"),
                    R("SHI", "JHA / JSA", 2, "Conforming"),
                    R("SHI", "JHA / JSA", 3, "Conforming"),
                    R("SHI", "JHA / JSA", 4, "Conforming"),
                    R("SHI", "JHA / JSA", 5, "Conforming"),
                    R("SHI", "JHA / JSA", 6, "Conforming"),
                    R("SHI", "JHA / JSA", 7, "Conforming"),
                    R("SHI", "JHA / JSA", 8, "Conforming"),
                    // Culture — 1 Warning
                    R("SHI", "Culture / Attitudes", 0, "Warning", "New crew members unfamiliar with site-specific hazards — supervisor addressed during audit"),
                    R("SHI", "Culture / Attitudes", 1, "Conforming"),
                    R("SHI", "Culture / Attitudes", 2, "Conforming"),
                    R("SHI", "Culture / Attitudes", 3, "Conforming"),
                    R("SHI", "Culture / Attitudes", 4, "Conforming"),
                    R("SHI", "Culture / Attitudes", 5, "Conforming"),
                },
                questionsByDivSection),

            // #2 — SHI, Feb 6, Valero Port Arthur — still struggling (8 NCs, score ~79%)
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
                    R("SHI", "Permitting", 0, "NonConforming", "Blasting permit missing client co-signature — work proceeded without proper authorization", caDesc: "All hot work / blasting permits require client co-signature before work commences; PM to confirm with client rep", caAssignedTo: "Marcus Webb", caDaysOffset: -60),
                    R("SHI", "Permitting", 1, "Conforming"),
                    R("SHI", "Permitting", 2, "Conforming"),
                    R("SHI", "Permitting", 3, "Conforming"),
                    R("SHI", "Personal Protective Equipment", 0, "NonConforming", "2 employees not wearing splash goggles as required on permit — verbal correction issued, goggles donned immediately", correctedOnSite: true),
                    R("SHI", "Personal Protective Equipment", 1, "NonConforming", "Blasting hood not worn during active blasting phase — employee removed from task", caDesc: "Conduct mandatory PPE audit walk-through with crew before every blasting operation", caAssignedTo: "Marcus Webb", caDaysOffset: -60),
                    R("SHI", "Personal Protective Equipment", 2, "Warning", "1 harness tag expired — tagged out and replaced"),
                    R("SHI", "Equipment & Equipment Inspection", 0, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 1, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 2, "NonConforming", "Blasting pot pressure relief valve not inspected for current shift — equipment removed from service", caDesc: "Shift foreman to verify and document blasting equipment inspection before each shift start", caAssignedTo: "Marcus Webb", caDaysOffset: -60),
                    R("SHI", "Equipment & Equipment Inspection", 3, "NonConforming", "Air monitor calibration paperwork not on-site — paperwork retrieved from foreman's truck within 30 min", correctedOnSite: true),
                    R("SHI", "Equipment & Equipment Inspection", 4, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 5, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 6, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 7, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 8, "Conforming"),
                    R("SHI", "Equipment & Equipment Inspection", 9, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 0, "NonConforming", "Blasting debris impacting adjacent work area without exclusion zone — work halted", caDesc: "Establish minimum 30-ft exclusion zone for all blasting operations; post spotter at perimeter", caAssignedTo: "Marcus Webb", caDaysOffset: -60),
                    R("SHI", "Job Site & Confined Space Condition", 1, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 2, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 3, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 4, "NonConforming", "Housekeeping in work area unacceptable — material piled against scaffold legs creating trip/fall hazard", correctedOnSite: true),
                    R("SHI", "Job Site & Confined Space Condition", 5, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 6, "Conforming"),
                    R("SHI", "Job Site & Confined Space Condition", 7, "Conforming"),
                    R("SHI", "Scaffolds", 0, "Conforming"),
                    R("SHI", "Scaffolds", 1, "Conforming"),
                    R("SHI", "Scaffolds", 2, "NonConforming", "Scaffold tag not current for lower platform — access restricted until inspected and re-tagged"),
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

        audits.AddRange(BuildHistoricalAudits(shiDiv, shiVer, etsDiv, etsVer, tkieDiv, tkieVer, stsDiv, stsVer, stgDiv, stgVer,
            cslDiv, cslVer, facilityDiv, facilityVer, shirtDiv, shirtVer, shiraDiv, shiraVer));

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

    // ── Historical audits: Q2 2024–Q4 2025 (all 9 divisions) ────────────────

    private static List<(Audit audit, AuditHeader header, List<ResponseSeed> responses)> BuildHistoricalAudits(
        Models.Audit.Division shiDiv, Models.Audit.AuditTemplateVersion shiVer,
        Models.Audit.Division etsDiv, Models.Audit.AuditTemplateVersion etsVer,
        Models.Audit.Division tkieDiv, Models.Audit.AuditTemplateVersion tkieVer,
        Models.Audit.Division stsDiv, Models.Audit.AuditTemplateVersion stsVer,
        Models.Audit.Division stgDiv, Models.Audit.AuditTemplateVersion stgVer,
        Models.Audit.Division? cslDiv, Models.Audit.AuditTemplateVersion? cslVer,
        Models.Audit.Division? facilityDiv, Models.Audit.AuditTemplateVersion? facilityVer,
        Models.Audit.Division? shirtDiv, Models.Audit.AuditTemplateVersion? shirtVer,
        Models.Audit.Division? shiraDiv, Models.Audit.AuditTemplateVersion? shiraVer)
    {
        var list = new List<(Audit audit, AuditHeader header, List<ResponseSeed> responses)>();

        void Add(Models.Audit.Division div, Models.Audit.AuditTemplateVersion ver,
            AuditHeader hdr, DateTime created, DateTime submitted, List<ResponseSeed> r)
            => list.Add((new Audit
            {
                DivisionId = div.Id,
                TemplateVersionId = ver.Id,
                AuditType = "JobSite",
                Status = "Submitted",
                SubmittedAt = submitted,
                CreatedAt = created,
                CreatedBy = System,
            }, hdr, r));

        // ── Q2 2024 — SHI (higher NC rate: equipment + permitting) ──────────
        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2404-011", Client = "Valero Port Arthur", PM = "Marcus Webb", Unit = "Unit 3 / Crude Distillation", Location = "Port Arthur, TX", AuditDate = new DateOnly(2024, 4, 15), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Catalyst handling and vessel internal access on CDU-100" },
            new DateTime(2024, 4, 15, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 4, 15, 16, 15, 0, DateTimeKind.Utc),
            SHIBase(
                ("Equipment & Equipment Inspection", 5, "NonConforming", "Defective angle grinder with cracked guard found in active tool crib — not tagged out; removed from service during audit", false, "Tag all defective tools immediately; inspect full tool inventory within 24 hours", "Marcus Webb"),
                ("Permitting", 3, "NonConforming", "Confined space log absent — entry in progress with no atmospheric readings documented; work halted until log produced", false, "Ensure confined space log is on-site before any entry begins; foreman to provide written acknowledgment", "Marcus Webb")));

        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2405-023", Client = "Motiva Port Arthur", PM = "Derrick Fontenot", Unit = "Unit 5 / Coker", Location = "Port Arthur, TX", AuditDate = new DateOnly(2024, 5, 20), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Coke drum head removal and drum internal cleaning" },
            new DateTime(2024, 5, 20, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 5, 20, 15, 45, 0, DateTimeKind.Utc),
            SHIBase(
                ("Lock-Out / Tag-Out", 0, "NonConforming", "Employee working on pump without personal LOTO lock — work stopped immediately, LOTO applied", true, "Conduct site-wide LOTO refresher training for all crew; document completion in E-Chart within 5 business days", "Derrick Fontenot"),
                ("Personal Protective Equipment", 2, "Warning", "Fall protection harness color code tags expired on two employees — replaced before re-entry", false, null, null)));

        // ── Q2 2024 — ETS (PPE and equipment NCs) ───────────────────────────
        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2404-007", Client = "Enterprise Products Partners", PM = "Angela Broussard", Unit = "Unit 2 / Propylene Splitter", Location = "Mont Belvieu, TX", AuditDate = new DateOnly(2024, 4, 11), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Heat exchanger cleaning and retubing on fractionation unit" },
            new DateTime(2024, 4, 11, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 4, 11, 16, 0, 0, DateTimeKind.Utc),
            ETSBase(
                ("Personal Protective Equipment", 1, "NonConforming", "3 employees performing confined space work with no respirator fit-test records on-site; entries suspended until records produced", false, "Retrieve and file current respirator fit-test documentation for all confined space entrants; confirm on-site availability before next permit", "Angela Broussard"),
                ("Equipment & Equipment Inspection", 5, "NonConforming", "Two power tools with cracked guards not tagged out — found in active tool crib; removed during audit", true, "Implement daily tool inspection checklist for Mont Belvieu crew; PM to confirm compliance within 7 days", "Angela Broussard")));

        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2405-019", Client = "LyondellBasell", PM = "Keith Landry", Unit = "Unit 4 / Polyethylene Reactor", Location = "La Porte, TX", AuditDate = new DateOnly(2024, 5, 22), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Reactor vessel cleaning and screen replacement on PE-400" },
            new DateTime(2024, 5, 22, 7, 45, 0, DateTimeKind.Utc), new DateTime(2024, 5, 22, 15, 30, 0, DateTimeKind.Utc),
            ETSBase(
                ("Job Site & Confined Space Condition", 1, "NonConforming", "Retrieval device not rigged at confined space entry — work in progress with no rescue capability; halted until setup complete", true, "Ensure retrieval system is rigged before any confined space entry begins; document in confined space permit", "Keith Landry"),
                ("Scaffolds", 0, "Warning", "Scaffold per-shift inspection form not completed for current shift — foreman completed during audit", false, null, null),
                ("Personal Protective Equipment", 0, "Warning", "1 employee missing required FR sleeve on chemical exposure task — corrected immediately", false, null, null)));

        // ── Q3 2024 — SHI (JHA and permitting issues) ───────────────────────
        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2407-034", Client = "Gray Oak Pipeline", PM = "Marcus Webb", Unit = "Unit 1 / Meter Station", Location = "Corpus Christi, TX", AuditDate = new DateOnly(2024, 7, 9), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Meter run replacement and piping tie-in at Station 7" },
            new DateTime(2024, 7, 9, 8, 15, 0, DateTimeKind.Utc), new DateTime(2024, 7, 9, 15, 50, 0, DateTimeKind.Utc),
            SHIBase(
                ("JHA / JSA", 4, "NonConforming", "Daily JHA not updated to reflect pipe excavation hazard added mid-shift; work on excavation halted until JHA revised", false, "Crew must revise and re-sign JHA any time scope or hazards change during shift; PM to verify JHA currency at each scope change", "Marcus Webb"),
                ("Equipment & Equipment Inspection", 3, "Warning", "Equipment inspection form for excavator not on foreman's person — produced from tool trailer within 10 minutes", false, null, null)));

        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2408-047", Client = "Chevron Phillips Cedar Bayou", PM = "Derrick Fontenot", Unit = "Unit 7 / Ethylene Cracker", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 8, 27), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Furnace tube replacement and refractory repair on F-701" },
            new DateTime(2024, 8, 27, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 8, 27, 16, 0, 0, DateTimeKind.Utc),
            SHIBase(
                ("Permitting", 1, "NonConforming", "Emergency procedures binder not accessible in work area — found in superintendent vehicle 100 yards from work site; repositioned during audit", true, "Position emergency procedures binder within arm's reach of active work area; foreman to verify at each shift start", "Derrick Fontenot"),
                ("Job Site & Confined Space Condition", 6, "Warning", "Housekeeping deteriorating in west staging area — loose debris near access ladder; crew corrected during audit", false, null, null)));

        // ── Q3 2024 — ETS (PPE + housekeeping NCs) ──────────────────────────
        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2407-011", Client = "INEOS Phenol", PM = "Angela Broussard", Unit = "Unit 3 / Phenol Column", Location = "Pasadena, TX", AuditDate = new DateOnly(2024, 7, 16), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Column internals replacement and tray deck inspection on PC-301" },
            new DateTime(2024, 7, 16, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 7, 16, 16, 10, 0, DateTimeKind.Utc),
            ETSBase(
                ("Personal Protective Equipment", 0, "NonConforming", "4 employees not wearing required chemical splash goggles during phenol wash preparation — corrected immediately on site", true, "Conduct focused PPE re-briefing covering chemical-specific requirements for INEOS Phenol site; confirm within 7 days", "Angela Broussard"),
                ("Job Site & Confined Space Condition", 6, "NonConforming", "Housekeeping around column base severely lacking — tripping hazards from unsecured debris; corrected during audit", true, "PM to establish housekeeping inspection at each shift start; document in daily job log going forward", "Angela Broussard")));

        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2408-024", Client = "Huntsman Pigments", PM = "Keith Landry", Unit = "Unit 8 / TiO2 Reactor", Location = "Port Neches, TX", AuditDate = new DateOnly(2024, 8, 29), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Reactor vessel inspection and nozzle repair on R-801" },
            new DateTime(2024, 8, 29, 7, 45, 0, DateTimeKind.Utc), new DateTime(2024, 8, 29, 15, 40, 0, DateTimeKind.Utc),
            ETSBase(
                ("Equipment & Equipment Inspection", 3, "NonConforming", "Air monitor present but calibration paperwork not on-site; records found in PM vehicle — repositioned during audit", true, "Air monitor calibration paperwork must be kept at the air monitor at all times; PM to confirm compliance within 5 business days", "Keith Landry"),
                ("Permitting", 1, "Warning", "Permit PPE section missing chemical-specific glove rating — annotated during audit to reflect correct requirement", false, null, null)));

        // ── Q4 2024 — SHI (PPE + job site NCs) ─────────────────────────────
        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2410-018", Client = "ExxonMobil Baytown", PM = "Marcus Webb", Unit = "Unit 2 / Hydrocracker", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 10, 8), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Reactor internals removal and basket cleaning on HC-200" },
            new DateTime(2024, 10, 8, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 8, 16, 30, 0, DateTimeKind.Utc),
            SHIBase(
                ("Personal Protective Equipment", 0, "NonConforming", "Employee accessing hot work area without required flame-resistant coveralls — work stopped; employee sent to change into FRC", true, "Conduct PPE audit for full crew to verify FRC compliance; PM to confirm before next shift start", "Marcus Webb"),
                ("Equipment & Equipment Inspection", 7, "Warning", "Extension cords without current quarter color coding — replaced with properly coded cords during audit", false, null, null)));

        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2411-031", Client = "Valero Texas City", PM = "Derrick Fontenot", Unit = "Unit 4 / FCCU", Location = "Texas City, TX", AuditDate = new DateOnly(2024, 11, 19), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Catalytic cracker riser piping replacement and insulation" },
            new DateTime(2024, 11, 19, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 11, 19, 15, 55, 0, DateTimeKind.Utc),
            SHIBase(
                ("Job Site & Confined Space Condition", 1, "NonConforming", "Retrieval tripod at confined space entry carries incorrect SWL rating tag — certified equipment brought to site before entry permitted", false, "Replace all non-certified retrieval equipment; obtain current certification; document in confined space permit packet", "Derrick Fontenot")));

        // ── Q4 2024 — ETS (equipment recurring issue) ───────────────────────
        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2410-015", Client = "Enterprise Products Partners", PM = "Angela Broussard", Unit = "Unit 5 / Butane Splitter", Location = "Mont Belvieu, TX", AuditDate = new DateOnly(2024, 10, 14), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Heat exchanger retubing and bundle cleaning on E-502" },
            new DateTime(2024, 10, 14, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 14, 16, 0, 0, DateTimeKind.Utc),
            ETSBase(
                ("Equipment & Equipment Inspection", 0, "NonConforming", "Guard removed from right-angle grinder in active use — guard reinstalled immediately; work halted until verified safe", true, "Mandatory toolbox talk on tool guarding requirements; PM to confirm all tools inspected before next shift start", "Angela Broussard"),
                ("Equipment & Equipment Inspection", 5, "NonConforming", "Defective circular saw blade cracked along body — saw removed from service; blade and tool logged as defective", false, "Implement pre-shift tool inspection checklist; document and remove defective tools before work begins each day", "Angela Broussard")));

        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2411-028", Client = "LyondellBasell", PM = "Keith Landry", Unit = "Unit 2 / Ethylene Splitter", Location = "La Porte, TX", AuditDate = new DateOnly(2024, 11, 21), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Cryogenic pipe insulation repair on low-temp service lines" },
            new DateTime(2024, 11, 21, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 11, 21, 15, 45, 0, DateTimeKind.Utc),
            ETSBase(
                ("Permitting", 2, "NonConforming", "2 employees on permit not listed as working crew — performing work not on permit; supervisor corrected and reissued permit on site", true, "All employees performing work must be listed on permit before work begins; crew to confirm at permit review", "Keith Landry"),
                ("Personal Protective Equipment", 2, "Warning", "1 harness D-ring showing wear beyond acceptable limits — replaced before employee re-entered work area", false, null, null)));

        // ── Q1 2025 — SHI (improving, 1 NC each) ────────────────────────────
        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2501-015", Client = "Motiva Port Arthur", PM = "Marcus Webb", Unit = "Unit 8 / Hydrogen Plant", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 1, 22), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Reformer tube inspection and catalyst sampling on H-800" },
            new DateTime(2025, 1, 22, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 22, 15, 40, 0, DateTimeKind.Utc),
            SHIBase(
                ("Personal Protective Equipment", 2, "Warning", "1 harness missing current quarter color code sticker — sticker applied on-site during audit", false, null, null)));

        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2502-029", Client = "Chevron Phillips Cedar Bayou", PM = "Derrick Fontenot", Unit = "Unit 9 / EDC Plant", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 2, 14), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "EDC column internal access and downcomer cleaning" },
            new DateTime(2025, 2, 14, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 14, 16, 20, 0, DateTimeKind.Utc),
            SHIBase(
                ("JHA / JSA", 3, "NonConforming", "STRONG cards not collected at end of morning shift — cards found in stack on table unreviewed; PM collected and reviewed immediately", true, "PM to collect and review STRONG cards at shift change without exception; confirm in daily job log", "Derrick Fontenot")));

        // ── Q1 2025 — ETS (PPE + LOTO NCs) ─────────────────────────────────
        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2501-009", Client = "Chevron Phillips Cedar Bayou", PM = "Angela Broussard", Unit = "Unit 11 / Ethylene Cracker", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 1, 15), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Convection section cleaning and burner tip replacement on F-1100" },
            new DateTime(2025, 1, 15, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 15, 16, 0, 0, DateTimeKind.Utc),
            ETSBase(
                ("Personal Protective Equipment", 1, "NonConforming", "Respirator fit-test records missing for 2 of 6 confined space entrants; entries halted until records produced from PM vehicle", true, "Store respirator fit-test documentation with confined space entry permit packet; confirm on-site availability during permit validation", "Angela Broussard"),
                ("Lock-Out / Tag-Out", 2, "NonConforming", "Satellite LOTO box opened by foreman master key without all crew locks applied — two employees corrected after verbal instruction", false, "Review LOTO satellite box procedure with all crew; no master key access until all personal locks are applied and verified", "Angela Broussard")));

        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2502-022", Client = "INEOS Phenol", PM = "Keith Landry", Unit = "Unit 2 / Cumene Column", Location = "Pasadena, TX", AuditDate = new DateOnly(2025, 2, 24), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Column tray replacement and downcomer seal installation" },
            new DateTime(2025, 2, 24, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 2, 24, 15, 50, 0, DateTimeKind.Utc),
            ETSBase(
                ("JHA / JSA", 1, "NonConforming", "Daily JHA not signed by 3 employees prior to work start — employees signed after work was under way; work halted briefly", true, "Daily JHA must be signed before any work begins; supervisor to verify signatures during toolbox meeting", "Keith Landry"),
                ("Equipment & Equipment Inspection", 0, "Warning", "Grinder guard slightly loose but functional — tightened on-site; no hazard presented", false, null, null)));

        // ── Q2 2025 — SHI (good quarter, minimal issues) ────────────────────
        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2504-022", Client = "Gray Oak Pipeline", PM = "Marcus Webb", Unit = "Unit 3 / Separator Station", Location = "Corpus Christi, TX", AuditDate = new DateOnly(2025, 4, 14), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Separator vessel inspection and outlet nozzle repair" },
            new DateTime(2025, 4, 14, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 4, 14, 15, 30, 0, DateTimeKind.Utc),
            SHIBase(
                ("Equipment & Equipment Inspection", 3, "Warning", "Foreman's equipment inspection card from prior day — updated card completed during audit walk", false, null, null)));

        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2505-041", Client = "ExxonMobil Beaumont", PM = "Derrick Fontenot", Unit = "Unit 5 / Lubricants", Location = "Beaumont, TX", AuditDate = new DateOnly(2025, 5, 27), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Lubricant column trays and heat exchanger access" },
            new DateTime(2025, 5, 27, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 5, 27, 15, 45, 0, DateTimeKind.Utc),
            SHIBase(
                ("Scaffolds", 2, "NonConforming", "Scaffold platform level 2 has accumulated debris creating slip hazard — crew swept and cleared during audit", true, null, null)));

        // ── Q2 2025 — ETS (improving, 1 NC each) ────────────────────────────
        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2504-013", Client = "Enterprise Products Partners", PM = "Angela Broussard", Unit = "Unit 6 / NGL Splitter", Location = "Mont Belvieu, TX", AuditDate = new DateOnly(2025, 4, 7), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Column head removal and tray deck inspection on T-601" },
            new DateTime(2025, 4, 7, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 4, 7, 15, 40, 0, DateTimeKind.Utc),
            ETSBase(
                ("Permitting", 1, "NonConforming", "Permit PPE section missing required chemical-resistant gloves for acid cleaning task — permit corrected and re-signed on site", true, null, null)));

        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2505-027", Client = "Huntsman Pigments", PM = "Keith Landry", Unit = "Unit 3 / Chlorination Reactor", Location = "Port Neches, TX", AuditDate = new DateOnly(2025, 5, 19), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Chlorination reactor nozzle replacement and flange inspection" },
            new DateTime(2025, 5, 19, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 5, 19, 15, 30, 0, DateTimeKind.Utc),
            ETSBase(
                ("Equipment & Equipment Inspection", 7, "Warning", "One cord without current color code — replaced with properly coded cord on-site", false, null, null)));

        // ── Q3 2025 — SHI (excellent, near-clean) ───────────────────────────
        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2507-016", Client = "Valero Port Arthur", PM = "Marcus Webb", Unit = "Unit 6 / Alkylation", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 7, 8), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "HF acid neutralizer cleaning and nozzle replacement" },
            new DateTime(2025, 7, 8, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 7, 8, 15, 55, 0, DateTimeKind.Utc),
            SHIBase());

        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2508-033", Client = "LyondellBasell Corpus Christi", PM = "Derrick Fontenot", Unit = "Unit 2 / Ethylene Oxide", Location = "Corpus Christi, TX", AuditDate = new DateOnly(2025, 8, 19), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "EO column tray replacement and bed support inspection" },
            new DateTime(2025, 8, 19, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 8, 19, 16, 5, 0, DateTimeKind.Utc),
            SHIBase(
                ("Culture / Attitudes", 0, "Warning", "Supervisor initially resistant to audit process — attitude improved after brief discussion with PM; no safety practices impacted", false, null, null)));

        // ── Q3 2025 — ETS (near-clean) ───────────────────────────────────────
        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2507-018", Client = "LyondellBasell", PM = "Angela Broussard", Unit = "Unit 7 / Acetaldehyde", Location = "La Porte, TX", AuditDate = new DateOnly(2025, 7, 21), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Acetaldehyde column head removal and internal inspection" },
            new DateTime(2025, 7, 21, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 7, 21, 15, 45, 0, DateTimeKind.Utc),
            ETSBase(
                ("Personal Protective Equipment", 2, "Warning", "1 harness sternum strap worn and fraying — harness removed from service and replacement issued before re-entry", false, null, null)));

        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2508-031", Client = "Chevron Phillips Cedar Bayou", PM = "Keith Landry", Unit = "Unit 4 / 1-Hexene", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 8, 11), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Oligomerization reactor internal access and basket retrieval" },
            new DateTime(2025, 8, 11, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 8, 11, 15, 30, 0, DateTimeKind.Utc),
            ETSBase());

        // ── Q4 2025 — SHI (slight uptick) ───────────────────────────────────
        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2510-025", Client = "Motiva Port Arthur", PM = "Marcus Webb", Unit = "Unit 4 / Crude Desalter", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 10, 7), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Desalter vessel cleaning and mixing element replacement" },
            new DateTime(2025, 10, 7, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 7, 16, 10, 0, DateTimeKind.Utc),
            SHIBase(
                ("Job Site & Confined Space Condition", 6, "NonConforming", "Housekeeping in confined space staging area unacceptable — loose flange bolts and debris on walking surface near entry; corrected before entry permitted", false, "PM to establish mandatory housekeeping inspection before any confined space entry; document in daily log", "Marcus Webb")));

        Add(shiDiv, shiVer,
            new AuditHeader { JobNumber = "SHI-2511-038", Client = "Gray Oak Pipeline", PM = "Derrick Fontenot", Unit = "Unit 2 / Booster Compression", Location = "Corpus Christi, TX", AuditDate = new DateOnly(2025, 11, 18), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Compressor cylinder replacement and rod packing service on K-201" },
            new DateTime(2025, 11, 18, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 11, 18, 16, 0, 0, DateTimeKind.Utc),
            SHIBase(
                ("Personal Protective Equipment", 2, "Warning", "Color code sticker missing from one employee's lanyard — replaced on-site during audit", false, null, null)));

        // ── Q4 2025 — ETS (one NC per audit) ────────────────────────────────
        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2510-022", Client = "Enterprise Products Partners", PM = "Angela Broussard", Unit = "Unit 9 / Isobutane Splitter", Location = "Mont Belvieu, TX", AuditDate = new DateOnly(2025, 10, 9), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Splitter column head and tray replacement on T-902" },
            new DateTime(2025, 10, 9, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 9, 16, 15, 0, DateTimeKind.Utc),
            ETSBase(
                ("JHA / JSA", 1, "NonConforming", "Daily JHA revisions not captured when scope changed from confined space access to hot work — revised JHA prepared during audit and signed by crew", true, "Crew must revise and re-sign JHA any time scope or hazard changes; PM to verify JHA currency at each scope change", "Angela Broussard")));

        Add(etsDiv, etsVer,
            new AuditHeader { JobNumber = "ETS-2511-036", Client = "INEOS Phenol", PM = "Keith Landry", Unit = "Unit 6 / Acetone Splitter", Location = "Pasadena, TX", AuditDate = new DateOnly(2025, 11, 25), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Column internal access and weld repair on AS-601 bottom head" },
            new DateTime(2025, 11, 25, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 11, 25, 15, 50, 0, DateTimeKind.Utc),
            ETSBase(
                ("Job Site & Confined Space Condition", 6, "Warning", "Staging area near column base had minor debris accumulation — corrected during audit walk-through", false, null, null)));

        // ── Q4 2024 — TKIE (PPE recurring + Equipment) ─────────────────────
        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2410-008", Client = "Kinder Morgan", PM = "Josh Hebert", Unit = "Pipeline Station 3", Location = "Beaumont, TX", AuditDate = new DateOnly(2024, 10, 10), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Mainline valve maintenance and cathodic protection survey" },
            new DateTime(2024, 10, 10, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 10, 15, 50, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Personal Protective Equipment", 0, "NonConforming", "3 employees not wearing required FR clothing for hot work task near pipeline — work stopped; employees changed into FRC before resuming", false, "Conduct site PPE audit before each hot work permit is issued; PM to confirm FR compliance in toolbox safety meeting", "Josh Hebert"),
                ("Equipment & Equipment Inspection", 4, "NonConforming", "Portable gas monitor not calibrated within current shift — all atmospheric testing suspended until calibration documented", false, "Calibrate and document all gas detection equipment at shift start; keep calibration log at monitor", "Josh Hebert"),
                ("Personal Protective Equipment", 2, "Warning", "1 employee color-code tag on harness expired — new sticker applied on-site", false, null, null)));

        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2411-017", Client = "Plains All American Pipeline", PM = "Josh Hebert", Unit = "Pump Station 4", Location = "Victoria, TX", AuditDate = new DateOnly(2024, 11, 14), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Pump mechanical seal replacement and coupling alignment" },
            new DateTime(2024, 11, 14, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 11, 14, 15, 40, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Personal Protective Equipment", 1, "NonConforming", "Employees performing coupling alignment without required face shield — shields retrieved and donned on-site during audit", true, null, null),
                ("JHA / JSA", 3, "Warning", "JHA not updated after equipment change mid-shift — supervisor updated during audit walk", false, null, null)));

        // ── Q4 2024 — STS (Equipment NC + mostly clean) ─────────────────────
        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2410-011", Client = "ExxonMobil Baytown", PM = "Kevin Melancon", Unit = "Unit 4 / Reformer", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 10, 21), Auditor = "S. Guillory", Shift = "DAY", WorkDescription = "Reformer furnace tube access and refractory inspection" },
            new DateTime(2024, 10, 21, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 21, 16, 5, 0, DateTimeKind.Utc),
            STSBase(
                ("Equipment & Equipment Inspection", 6, "NonConforming", "GFCI receptacle missing from extension cord in wet work area — work halted in zone until GFCI installed", false, "Ensure all extension cords in wet or outdoor areas use GFCI protection; verify daily in pre-shift inspection", "Kevin Melancon"),
                ("Personal Protective Equipment", 1, "Warning", "1 harness sternum strap fraying near buckle — replaced before employee re-entered elevated area", false, null, null)));

        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2411-022", Client = "Motiva Port Arthur", PM = "Kevin Melancon", Unit = "Unit 7 / Vacuum Tower", Location = "Port Arthur, TX", AuditDate = new DateOnly(2024, 11, 18), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Vacuum tower wash section internals replacement and tray inspection" },
            new DateTime(2024, 11, 18, 7, 45, 0, DateTimeKind.Utc), new DateTime(2024, 11, 18, 15, 50, 0, DateTimeKind.Utc),
            STSBase());

        // ── Q4 2024 — STG (Permitting + Scaffold + Job Site problems) ────────
        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2410-009", Client = "Marathon Galveston Bay", PM = "Travis Gaspard", Unit = "Unit 3 / Coker", Location = "Texas City, TX", AuditDate = new DateOnly(2024, 10, 16), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Coke drum deheading operations and drum interior access" },
            new DateTime(2024, 10, 16, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 16, 16, 20, 0, DateTimeKind.Utc),
            STGBase(
                ("Permitting", 0, "NonConforming", "Confined space permit not signed by client rep prior to entry beginning — work halted; permit obtained and co-signed before re-entry", false, "Confirm client permit co-signature before any confined space entry; PM to verify as part of permit validation", "Travis Gaspard"),
                ("Scaffolds", 2, "NonConforming", "Level 2 scaffold platform missing end-boards — employees exposed to fall-through hazard; work stopped until boards installed", false, "Inspect all scaffold platforms for completeness before shift start; document in daily scaffold inspection log", "Travis Gaspard"),
                ("Job Site & Confined Space Condition", 3, "NonConforming", "Barricade inadequate on east side of coke drum entry area — pedestrian traffic unrestricted near overhead hazard; corrected during audit", true, null, null)));

        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2411-021", Client = "Valero Texas City", PM = "Travis Gaspard", Unit = "Unit 6 / Desulfurization", Location = "Texas City, TX", AuditDate = new DateOnly(2024, 11, 20), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Reactor bed support screen replacement and insulation repair" },
            new DateTime(2024, 11, 20, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 11, 20, 15, 45, 0, DateTimeKind.Utc),
            STGBase(
                ("Permitting", 2, "NonConforming", "Hot work permit did not list all ignition sources present in area — permit revised and re-signed before work resumed", true, null, null),
                ("Equipment & Equipment Inspection", 5, "NonConforming", "Angle grinder with cracked guard found in use — equipment removed from service immediately; guard replaced", false, "Conduct mandatory pre-shift tool inspection; all damaged guards must be replaced before tool use", "Travis Gaspard")));

        // ── Q1 2025 — TKIE (PPE recurring, JHA NC) ──────────────────────────
        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2501-006", Client = "Kinder Morgan", PM = "Josh Hebert", Unit = "Metering Station 9", Location = "Beaumont, TX", AuditDate = new DateOnly(2025, 1, 16), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Meter run calibration and meter tube replacement" },
            new DateTime(2025, 1, 16, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 16, 15, 55, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Personal Protective Equipment", 0, "NonConforming", "Employee performing pipe grinding without required face shield — work stopped; shield retrieved and worn before resuming", true, null, null),
                ("JHA / JSA", 5, "NonConforming", "JHA missing excavation-specific hazards despite active trenching in work area — JHA revised and re-signed on-site", true, "Ensure JHA includes all active hazards at work site including any trench or excavation hazards; supervisor to verify at job start", "Josh Hebert")));

        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2502-014", Client = "Plains All American Pipeline", PM = "Josh Hebert", Unit = "Pump Station 7", Location = "Victoria, TX", AuditDate = new DateOnly(2025, 2, 19), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Centrifugal pump overhaul and impeller replacement" },
            new DateTime(2025, 2, 19, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 2, 19, 15, 40, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Equipment & Equipment Inspection", 5, "NonConforming", "Torque wrench calibration sticker expired — tool removed from service until re-calibrated and documented", false, "Maintain current calibration documentation for all precision tools; calibration log to be kept with each tool", "Josh Hebert"),
                ("Personal Protective Equipment", 2, "Warning", "1 harness lanyard color-code missing — sticker applied during audit", false, null, null)));

        // ── Q1 2025 — STS (Equipment NC + LOTO NC) ──────────────────────────
        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2501-007", Client = "Motiva Port Arthur", PM = "Kevin Melancon", Unit = "Unit 3 / Alkylation", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 1, 23), Auditor = "S. Guillory", Shift = "DAY", WorkDescription = "HF alkylation reactor maintenance and heat exchanger cleaning" },
            new DateTime(2025, 1, 23, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 23, 16, 10, 0, DateTimeKind.Utc),
            STSBase(
                ("Lock-Out / Tag-Out", 0, "NonConforming", "Equipment found energized with only one lock applied — second required lock not in place; work halted and LOTO re-applied with all locks", false, "Verify all isolation points are locked before work begins; LOTO review to be performed at every shift change", "Kevin Melancon")));

        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2502-016", Client = "ExxonMobil Baytown", PM = "Kevin Melancon", Unit = "Unit 11 / Crude Distillation", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 2, 27), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Crude distillation column tray replacement and downcomer cleaning" },
            new DateTime(2025, 2, 27, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 2, 27, 15, 50, 0, DateTimeKind.Utc),
            STSBase(
                ("Equipment & Equipment Inspection", 3, "NonConforming", "Air monitor present at confined space entry but calibration paperwork not available on-site — retrieved from PM truck and placed at entry point", true, null, null)));

        // ── Q1 2025 — STG (Permitting + Scaffold recurring) ─────────────────
        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2501-007", Client = "Marathon Galveston Bay", PM = "Travis Gaspard", Unit = "Unit 1 / Crude Distillation", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 1, 14), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Crude distillation column internals inspection and tray replacement" },
            new DateTime(2025, 1, 14, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 14, 16, 15, 0, DateTimeKind.Utc),
            STGBase(
                ("Permitting", 1, "NonConforming", "Hot work permit expiration time already passed — crew working without valid permit; work halted, new permit obtained", false, "Check permit expiration time before and during work; crew foreman to verify permit validity every 2 hours during extended tasks", "Travis Gaspard"),
                ("Scaffolds", 2, "NonConforming", "Scaffold tie-backs not installed at 4:1 ratio on south elevation — scaffold unstable; crew evacuated until ties added", false, "Scaffold competent person to verify tie-back spacing before crew is allowed on scaffold; document in daily inspection", "Travis Gaspard"),
                ("Job Site & Confined Space Condition", 6, "NonConforming", "Debris accumulation on walking surface at base of tower access — multiple trip hazards noted; corrected during audit", true, null, null)));

        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2502-019", Client = "Valero Texas City", PM = "Travis Gaspard", Unit = "Unit 4 / FCCU", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 2, 26), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "FCC regenerator cyclone inspection and refractory repair" },
            new DateTime(2025, 2, 26, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 2, 26, 15, 55, 0, DateTimeKind.Utc),
            STGBase(
                ("Permitting", 0, "NonConforming", "Two employees on confined space permit not listed on entry log — work halted until permit updated and re-verified", true, null, null),
                ("Equipment & Equipment Inspection", 8, "NonConforming", "Ventilation blower ductwork not fully secured at confined space entry — duct could slip and cut off air supply; secured before entry continued", false, "Verify ventilation duct integrity and connections before each confined space entry; document check in CS permit", "Travis Gaspard")));

        // ── Q2 2025 — TKIE (Equipment + LOTO) ───────────────────────────────
        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2504-011", Client = "Kinder Morgan", PM = "Josh Hebert", Unit = "Pipeline Station 11", Location = "Beaumont, TX", AuditDate = new DateOnly(2025, 4, 10), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Mainline pig launcher inspection and seal replacement" },
            new DateTime(2025, 4, 10, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 4, 10, 15, 45, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Equipment & Equipment Inspection", 7, "NonConforming", "Extension cord in active use without current quarter color code — replaced with properly coded cord on-site", true, null, null),
                ("Personal Protective Equipment", 0, "Warning", "1 employee with torn FR shirt sleeve — replaced before task resumed", false, null, null)));

        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2505-024", Client = "Plains All American Pipeline", PM = "Josh Hebert", Unit = "Pump Station 15", Location = "Victoria, TX", AuditDate = new DateOnly(2025, 5, 22), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Pump casing and impeller replacement on PS-15-100 series" },
            new DateTime(2025, 5, 22, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 5, 22, 15, 35, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Lock-Out / Tag-Out", 1, "NonConforming", "Lock-out device removed before all crew members were clear of equipment — re-LOTO applied immediately; crew re-briefed", false, "All personal LOTO locks must remain applied until each employee personally removes their lock; review LOTO removal procedure with crew", "Josh Hebert")));

        // ── Q2 2025 — STS (JHA NC + clean) ──────────────────────────────────
        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2504-013", Client = "ExxonMobil Baytown", PM = "Kevin Melancon", Unit = "Unit 6 / Hydrotreater", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 4, 15), Auditor = "S. Guillory", Shift = "DAY", WorkDescription = "Hydrotreater reactor internals inspection and catalyst sampling" },
            new DateTime(2025, 4, 15, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 4, 15, 16, 0, 0, DateTimeKind.Utc),
            STSBase(
                ("JHA / JSA", 2, "NonConforming", "JHA did not account for crane lift hazard during vessel head removal — work halted; JHA updated and re-signed before crane operations", true, null, null)));

        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2505-027", Client = "Motiva Port Arthur", PM = "Kevin Melancon", Unit = "Unit 5 / Propane Recovery", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 5, 20), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Propane recovery column overhead system maintenance and valve replacement" },
            new DateTime(2025, 5, 20, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 5, 20, 15, 45, 0, DateTimeKind.Utc),
            STSBase(
                ("Personal Protective Equipment", 2, "Warning", "Harness inspection tag not current — employee harness inspected and tagged during audit walk", false, null, null)));

        // ── Q2 2025 — STG (Permitting + Job Site) ────────────────────────────
        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2504-012", Client = "Marathon Galveston Bay", PM = "Travis Gaspard", Unit = "Unit 7 / Reformer", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 4, 8), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Reformer furnace tube replacement and burner inspection" },
            new DateTime(2025, 4, 8, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 4, 8, 16, 10, 0, DateTimeKind.Utc),
            STGBase(
                ("Permitting", 3, "NonConforming", "Safe work permit not specific to confined space entry — general permit used without confined space addendum; work halted until correct permit issued", false, "Use permit type-specific to the work being done; confined space addendum mandatory for all vessel entries", "Travis Gaspard"),
                ("Job Site & Confined Space Condition", 5, "NonConforming", "Temporary lighting inside vessel not rated for Class I Division 1 area — replaced with intrinsically safe fixtures before re-entry", false, "Verify all electrical equipment used inside process vessels carries appropriate area classification rating before use", "Travis Gaspard")));

        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2505-024", Client = "Valero Texas City", PM = "Travis Gaspard", Unit = "Unit 8 / Aromatics", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 5, 15), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Aromatics column tray cleaning and manway seal replacement" },
            new DateTime(2025, 5, 15, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 5, 15, 15, 50, 0, DateTimeKind.Utc),
            STGBase(
                ("Scaffolds", 1, "NonConforming", "Scaffold platform at elevation 45-ft missing required midrail on two sides — access restricted until rails installed and verified by competent person", false, "Scaffold inspection by competent person required before each shift; document all guardrail systems are complete", "Travis Gaspard"),
                ("Personal Protective Equipment", 2, "Warning", "Two employees wearing harnesses from prior quarter without updated color-code stickers — stickers applied on-site", false, null, null)));

        // ── Q3 2025 — TKIE (PPE NC, near-clean) ─────────────────────────────
        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2507-018", Client = "Kinder Morgan", PM = "Josh Hebert", Unit = "Compressor Station 2", Location = "Beaumont, TX", AuditDate = new DateOnly(2025, 7, 15), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Compressor rod packing replacement and valve servicing" },
            new DateTime(2025, 7, 15, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 7, 15, 16, 0, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Personal Protective Equipment", 0, "NonConforming", "Employee performing grinding without face shield on compressor station — corrected immediately on-site; face shield donned before work resumed", true, null, null),
                ("QA / QC Documentation", 1, "Warning", "Daily QC log not completed for morning work activities — foreman completed entries during audit", false, null, null)));

        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2508-031", Client = "Plains All American Pipeline", PM = "Josh Hebert", Unit = "Pump Station 20", Location = "Victoria, TX", AuditDate = new DateOnly(2025, 8, 26), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Main pump rotor replacement and bearing housing inspection" },
            new DateTime(2025, 8, 26, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 8, 26, 15, 45, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Personal Protective Equipment", 2, "Warning", "1 employee lanyard missing current color-code sticker — sticker applied during audit walk", false, null, null)));

        // ── Q3 2025 — STS (near-clean) ───────────────────────────────────────
        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2507-018", Client = "Motiva Port Arthur", PM = "Kevin Melancon", Unit = "Unit 14 / Isomerization", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 7, 17), Auditor = "S. Guillory", Shift = "DAY", WorkDescription = "Isomerization reactor bed unloading and screen replacement" },
            new DateTime(2025, 7, 17, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 7, 17, 16, 5, 0, DateTimeKind.Utc),
            STSBase(
                ("Equipment & Equipment Inspection", 8, "Warning", "Spare equipment left in staging area not properly stacked and secured — repositioned and blocked before leaving staging zone", false, null, null)));

        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2508-031", Client = "ExxonMobil Baytown", PM = "Kevin Melancon", Unit = "Unit 2 / Polyethylene", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 8, 19), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "PE reactor nozzle replacement and vessel entry for inspection" },
            new DateTime(2025, 8, 19, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 8, 19, 15, 50, 0, DateTimeKind.Utc),
            STSBase());

        // ── Q3 2025 — STG (Scaffold + PPE) ──────────────────────────────────
        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2507-016", Client = "Marathon Galveston Bay", PM = "Travis Gaspard", Unit = "Unit 9 / Hydrocracker", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 7, 8), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Hydrocracker reactor basket removal and catalyst unloading" },
            new DateTime(2025, 7, 8, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 7, 8, 16, 15, 0, DateTimeKind.Utc),
            STGBase(
                ("Scaffolds", 0, "NonConforming", "Scaffold daily inspection not performed for current shift — inspection completed and documented during audit; crew temporarily removed pending completion", true, null, null),
                ("Personal Protective Equipment", 1, "NonConforming", "2 employees inside confined space with full-face respirators where half-mask was listed on permit — permit revised to full-face on-site", true, null, null),
                ("Job Site & Confined Space Condition", 2, "Warning", "Minor housekeeping concern at scaffold base — cleared during audit", false, null, null)));

        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2508-028", Client = "Valero Texas City", PM = "Travis Gaspard", Unit = "Unit 11 / Lube Oil", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 8, 14), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Lube oil column tray replacement and drain line modifications" },
            new DateTime(2025, 8, 14, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 8, 14, 15, 45, 0, DateTimeKind.Utc),
            STGBase(
                ("Permitting", 1, "NonConforming", "Permit not re-signed after scope extension added column drain line work — permit voided and reissued with all employees signing", true, null, null),
                ("Equipment & Equipment Inspection", 5, "NonConforming", "Defective pneumatic wrench with cracked housing found in tool crib — removed from service and tagged out; replacement sourced", false, "All pneumatic tools to be inspected before each use; cracked housings are grounds for immediate removal from service", "Travis Gaspard")));

        // ── Q4 2025 — TKIE (PPE NC + Equipment NC) ──────────────────────────
        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2510-022", Client = "Kinder Morgan", PM = "Josh Hebert", Unit = "Meter Station 14", Location = "Beaumont, TX", AuditDate = new DateOnly(2025, 10, 14), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Turbine meter replacement and ultrasonic meter calibration run" },
            new DateTime(2025, 10, 14, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 14, 16, 0, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Personal Protective Equipment", 0, "NonConforming", "Employee performing grinding task without face shield — corrected immediately; work stopped until face shield worn", true, null, null),
                ("Culture / Attitudes", 3, "Warning", "Two employees initially dismissed safety briefing — addressed by foreman; attitude improved before audit concluded", false, null, null)));

        Add(tkieDiv, tkieVer,
            new AuditHeader { JobNumber = "TKIE-2511-037", Client = "Plains All American Pipeline", PM = "Josh Hebert", Unit = "Pump Station 23", Location = "Victoria, TX", AuditDate = new DateOnly(2025, 11, 19), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Pump motor coupling inspection and seal replacement on PS-23-300 series" },
            new DateTime(2025, 11, 19, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 11, 19, 15, 50, 0, DateTimeKind.Utc),
            TKIEBase(
                ("Personal Protective Equipment", 1, "NonConforming", "Employees performing coupling inspection without required face shields in pinch point hazard zone — corrected during audit", true, null, null),
                ("Equipment & Equipment Inspection", 6, "NonConforming", "Portable gas detector low battery alarm active — operator unaware; battery replaced before confined space entries continued", false, "Gas detection equipment batteries to be checked and documented at shift start; low battery alarm must halt operations until resolved", "Josh Hebert")));

        // ── Q4 2025 — STS (Equipment NC + Permitting) ────────────────────────
        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2510-024", Client = "Motiva Port Arthur", PM = "Kevin Melancon", Unit = "Unit 18 / Naphtha Hydrotreater", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 10, 20), Auditor = "S. Guillory", Shift = "DAY", WorkDescription = "Hydrotreater reactor internals removal and distributor tray replacement" },
            new DateTime(2025, 10, 20, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 20, 16, 10, 0, DateTimeKind.Utc),
            STSBase(
                ("Permitting", 2, "NonConforming", "Excavation/ground disturbance permit missing for soil work adjacent to unit — work halted; permit obtained before soil disturbance resumed", false, "Ground disturbance permit required for any excavation within facility fence; obtain before mobilizing any digging equipment", "Kevin Melancon")));

        Add(stsDiv, stsVer,
            new AuditHeader { JobNumber = "STS-2511-036", Client = "ExxonMobil Baytown", PM = "Kevin Melancon", Unit = "Unit 7 / Olefins", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 11, 13), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Olefin column tray replacement and downcomer repair" },
            new DateTime(2025, 11, 13, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 11, 13, 15, 55, 0, DateTimeKind.Utc),
            STSBase(
                ("Equipment & Equipment Inspection", 9, "NonConforming", "Defective vacuum hose on portable vacuum unit — cracked body presenting air-quality hazard in enclosed area; equipment taken out of service", false, "Inspect all vacuum equipment before use inside enclosed work areas; cracked or deteriorated hoses grounds for immediate removal from service", "Kevin Melancon"),
                ("Personal Protective Equipment", 2, "Warning", "Employee harness sternum strap partially disengaged — corrected before continued elevated work", false, null, null)));

        // ── Q4 2025 — STG (Permitting + Scaffold + Equipment recurring) ───────
        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2510-019", Client = "Marathon Galveston Bay", PM = "Travis Gaspard", Unit = "Unit 12 / Visbreaker", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 10, 9), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Visbreaker soaker vessel internal cleaning and coil inspection" },
            new DateTime(2025, 10, 9, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 9, 16, 20, 0, DateTimeKind.Utc),
            STGBase(
                ("Permitting", 0, "NonConforming", "Confined space permit missing rescue team contact information — permit pulled and revised before entry allowed to continue", false, "Rescue team information is mandatory on confined space permits; PM to verify before any entry begins", "Travis Gaspard"),
                ("Scaffolds", 2, "NonConforming", "Scaffold platform at level 3 missing toe-boards on two sides — crew evacuated platform until toe-boards installed and verified", false, "Toe-boards required on all open-sided scaffold platforms; scaffold competent person to verify before crew access", "Travis Gaspard"),
                ("Equipment & Equipment Inspection", 4, "NonConforming", "Inadequate lighting inside vessel with only one string of lights — second string added before confined space entries resumed", false, "Minimum two independent light sources required in confined spaces; verify lighting adequacy in permit before entry", "Travis Gaspard")));

        Add(stgDiv, stgVer,
            new AuditHeader { JobNumber = "STG-2511-032", Client = "Valero Texas City", PM = "Travis Gaspard", Unit = "Unit 14 / Propylene Recovery", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 11, 18), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Propylene column head removal and tray deck inspection" },
            new DateTime(2025, 11, 18, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 11, 18, 15, 50, 0, DateTimeKind.Utc),
            STGBase(
                ("Permitting", 2, "NonConforming", "Confined space permit did not list atmospheric monitor model/serial number — permit annotated and re-verified on-site before entry resumed", true, null, null),
                ("Job Site & Confined Space Condition", 1, "NonConforming", "Retrieval winch not rigged at confined space entry point — work halted; retrieval system set up before entry permitted", false, "Retrieval system must be physically rigged and ready before any employee enters a permit-required confined space", "Travis Gaspard")));

        // ── CSL / FACILITY / SHI_RT / SHI_RA historical audits ──────────────
        if (cslDiv != null && cslVer != null)
        {
            void AddCSL(AuditHeader hdr, DateTime c, DateTime s, List<ResponseSeed> r) => Add(cslDiv, cslVer, hdr, c, s, r);

            // Q2 2024 — CSL (BSI system issues + cell phones in unit)
            AddCSL(new AuditHeader { JobNumber = "CSL-2404-001", Client = "ExxonMobil Baytown", PM = "Rick Navarro", Unit = "CDU-4 Confined Entry", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 4, 9), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "CDU-4 vessel internal inspection and catalyst recovery" },
                new DateTime(2024, 4, 9, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 4, 9, 16, 10, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 0, "NonConforming", "Cell phones found on two employees entering unit — phones confiscated and employees debriefed", false, "Issue zero-tolerance reminder to full crew; PM to document acknowledgment signatures", "Rick Navarro"),
                    ("CSL Specific", 7, "NonConforming", "BSI/Blackbox system inspection tag expired — system removed from service and re-inspected before entries resumed", false, "Verify BSI system inspection tag at start of every job; document in daily log", "Rick Navarro"),
                    ("Personal Protective Equipment", 0, "NonConforming", "3 employees not wearing required chemical-resistant gloves during catalyst handling — corrected on site", true, null, null)));

            AddCSL(new AuditHeader { JobNumber = "CSL-2405-008", Client = "Chevron Phillips Cedar Bayou", PM = "Linda Tran", Unit = "PE-Reactor Vessel", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 5, 14), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Polyethylene reactor vessel cleaning and nozzle inspection" },
                new DateTime(2024, 5, 14, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 14, 16, 30, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 9, "NonConforming", "Loading hopper checklist not completed before hopper engaged — work paused until checklist signed off", false, "Require checklist sign-off before any hopper operation begins; supervisor to verify", "Linda Tran"),
                    ("CSL Specific", 10, "NonConforming", "Life support packing list not on site — serial numbers could not be cross-referenced during audit; list retrieved from office", false, "Life support packing list must be on site before any confined entry begins", "Linda Tran")));

            // Q3 2024 — CSL (rescue kit incomplete + PPE)
            AddCSL(new AuditHeader { JobNumber = "CSL-2407-012", Client = "INEOS Phenol Pasadena", PM = "Rick Navarro", Unit = "Phenol Column Internal", Location = "Pasadena, TX", AuditDate = new DateOnly(2024, 7, 22), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Column internal cleaning and tray inspection" },
                new DateTime(2024, 7, 22, 7, 45, 0, DateTimeKind.Utc), new DateTime(2024, 7, 22, 16, 0, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 8, "NonConforming", "Rescue kit on site but missing required emergency oxygen cylinder — job halted until cylinder supplied from yard", false, "Rescue kit inventory must be verified complete before any confined entry permit is issued", "Rick Navarro"),
                    ("Permitting", 0, "NonConforming", "Confined space entry permit did not reflect current atmospheric readings — permit voided, fresh readings taken, permit reissued", false, "Atmospheric readings must be verified at time of permit issuance and documented with timestamp", "Rick Navarro")));

            AddCSL(new AuditHeader { JobNumber = "CSL-2408-021", Client = "LyondellBasell La Porte", PM = "Linda Tran", Unit = "Ethylene Cracker Vessel", Location = "La Porte, TX", AuditDate = new DateOnly(2024, 8, 6), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Cracker furnace tube internal inspection and cleaning" },
                new DateTime(2024, 8, 6, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 8, 6, 15, 45, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 11, "NonConforming", "Helmet serial numbers on site did not match packing list — two helmets swapped from another job without updating documentation", false, "All CSL equipment transferred between jobs must be re-documented in the life support packing list before use", "Linda Tran"),
                    ("Personal Protective Equipment", 1, "Warning", "FR coveralls on one employee had torn knee patch — replaced with spare before confined entry", false, null, null)));

            // Q4 2024 — CSL (video labeling + BSI)
            AddCSL(new AuditHeader { JobNumber = "CSL-2410-034", Client = "Valero Port Arthur", PM = "Rick Navarro", Unit = "Crude Distillation Vessel", Location = "Port Arthur, TX", AuditDate = new DateOnly(2024, 10, 15), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "CDU vessel internal cleaning and baffle inspection" },
                new DateTime(2024, 10, 15, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 10, 15, 16, 15, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 2, "NonConforming", "Video recordings not labeled with shift or job number — mislabeled media could not be assigned to specific entry; re-labeled during audit", true, "All video media must be labeled at time of recording with date, shift, job number, equipment and phase", "Rick Navarro"),
                    ("Job Site & Confined Space Condition", 1, "NonConforming", "Retrieval system not rigged at entry point when first entrant arrived — work halted; system rigged and verified before entry resumed", false, "Retrieval system must be physically in place before attendant signs off entry permit", "Rick Navarro")));

            AddCSL(new AuditHeader { JobNumber = "CSL-2411-041", Client = "Marathon Galveston Bay", PM = "Linda Tran", Unit = "Visbreaker Vessel", Location = "Texas City, TX", AuditDate = new DateOnly(2024, 11, 19), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Visbreaker soaker vessel cleaning and nozzle inspection" },
                new DateTime(2024, 11, 19, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 11, 19, 16, 0, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 3, "Warning", "Job logs present but missing timeline entries for two entry periods — corrected by crew during audit", false, null, null)));

            // Q1 2025 — CSL (improving)
            AddCSL(new AuditHeader { JobNumber = "CSL-2501-052", Client = "Motiva Port Arthur", PM = "Rick Navarro", Unit = "Coker Vessel", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 1, 14), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Coke drum internal cleaning and head inspection" },
                new DateTime(2025, 1, 14, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 1, 14, 16, 0, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 0, "NonConforming", "Cell phone found on one employee entering unit — phone secured and employee received written reminder", false, "Cell phone policy re-briefing required at next crew safety meeting; PM to document attendance", "Rick Navarro")));

            AddCSL(new AuditHeader { JobNumber = "CSL-2503-068", Client = "ExxonMobil Baytown", PM = "Linda Tran", Unit = "FCC Reactor Vessel", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 3, 18), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "FCC reactor internal inspection and cat fines removal" },
                new DateTime(2025, 3, 18, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 18, 16, 30, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 5, "Warning", "Module trailer interior moderately cluttered — crew organized during audit walkthrough", false, null, null)));

            // Q2 2025 — CSL (near clean)
            AddCSL(new AuditHeader { JobNumber = "CSL-2504-079", Client = "Chevron Phillips Cedar Bayou", PM = "Rick Navarro", Unit = "Ethylene Tower Vessel", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 4, 22), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "C2 splitter column internal cleaning and tray deck inspection" },
                new DateTime(2025, 4, 22, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 4, 22, 16, 0, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 7, "Warning", "BSI system inspection log present but signature block for day shift not completed — signed during audit", false, null, null)));

            AddCSL(new AuditHeader { JobNumber = "CSL-2506-087", Client = "INEOS Phenol Pasadena", PM = "Linda Tran", Unit = "Phenol Splitter Vessel", Location = "Pasadena, TX", AuditDate = new DateOnly(2025, 6, 10), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Phenol splitter vessel internal inspection and spray nozzle replacement" },
                new DateTime(2025, 6, 10, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 10, 16, 15, 0, DateTimeKind.Utc),
                CSLBase());

            // Q3 2025 — CSL (clean)
            AddCSL(new AuditHeader { JobNumber = "CSL-2507-091", Client = "Valero Port Arthur", PM = "Rick Navarro", Unit = "Crude Tower Vessel", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 7, 8), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Atmospheric distillation tower internal cleaning" },
                new DateTime(2025, 7, 8, 7, 45, 0, DateTimeKind.Utc), new DateTime(2025, 7, 8, 16, 0, 0, DateTimeKind.Utc),
                CSLBase());

            AddCSL(new AuditHeader { JobNumber = "CSL-2509-103", Client = "Marathon Galveston Bay", PM = "Linda Tran", Unit = "VDU Vessel", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 9, 17), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "Vacuum distillation vessel internal inspection and packing removal" },
                new DateTime(2025, 9, 17, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 17, 16, 30, 0, DateTimeKind.Utc),
                CSLBase(
                    ("Permitting", 1, "Warning", "Emergency procedures binder present but not within arm's reach of entry point — repositioned during audit", false, null, null)));

            // Q4 2025 — CSL (one NC)
            AddCSL(new AuditHeader { JobNumber = "CSL-2510-112", Client = "ExxonMobil Baytown", PM = "Rick Navarro", Unit = "CDU-2 Vessel", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 10, 7), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "CDU-2 vessel internal cleaning and internal coating inspection" },
                new DateTime(2025, 10, 7, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 10, 7, 16, 0, 0, DateTimeKind.Utc),
                CSLBase(
                    ("CSL Specific", 0, "NonConforming", "One employee found with personal cell phone on wrist mount — corrected immediately; written reminder issued", true, null, null)));

            AddCSL(new AuditHeader { JobNumber = "CSL-2511-119", Client = "Motiva Port Arthur", PM = "Linda Tran", Unit = "Coker B Vessel", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 11, 20), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "Coke drum B internal cleaning and refractory inspection" },
                new DateTime(2025, 11, 20, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 20, 16, 15, 0, DateTimeKind.Utc),
                CSLBase());
        }

        if (facilityDiv != null && facilityVer != null)
        {
            void AddFAC(AuditHeader hdr, DateTime c, DateTime s, List<ResponseSeed> r) => Add(facilityDiv, facilityVer, hdr, c, s, r);

            // Q2 2024 — FACILITY (signage + housekeeping issues)
            AddFAC(new AuditHeader { JobNumber = "FAC-2404-001", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Main Office Building", Location = "Houston, TX", AuditDate = new DateOnly(2024, 4, 3), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q2 2024 facility safety compliance audit" },
                new DateTime(2024, 4, 3, 9, 0, 0, DateTimeKind.Utc), new DateTime(2024, 4, 3, 15, 0, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Required Building Signage", 0, "NonConforming", "OSHA 300 form not posted in February as required — April audit finding; posted during audit walkthrough", true, "Set calendar reminder for posting Form 300 each February 1; HSM to verify posting by Feb 5", "Christina Moore"),
                    ("Housekeeping", 0, "NonConforming", "Break room and shop area accumulating debris — crew did not have designated disposal containers nearby; containers added", false, "Establish daily housekeeping check for all common areas; document in facility log", "Christina Moore"),
                    ("Environmental", 0, "NonConforming", "Visible oil stain on concrete pad near fuel storage — source identified as leaking drum bung; drum replaced and stain treated", false, "Inspect fuel storage area monthly and document condition; designate HSM as responsible party", "Christina Moore")));

            AddFAC(new AuditHeader { JobNumber = "FAC-2406-008", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Yard / Shop Building", Location = "Houston, TX", AuditDate = new DateOnly(2024, 6, 18), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q2 2024 yard and shop facility audit" },
                new DateTime(2024, 6, 18, 9, 0, 0, DateTimeKind.Utc), new DateTime(2024, 6, 18, 15, 30, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Hazardous Material Storage", 0, "NonConforming", "Gas cylinders in storage area not properly chained — two cylinders free-standing; chained during audit", true, "Chain all gas cylinders at rest; inspect monthly", "Christina Moore"),
                    ("Equipment", 0, "NonConforming", "Forklift daily inspection log missing for last 3 days — crew had been using checklist on phone not paper; paper log produced retroactively", false, "Maintain paper daily inspection log in forklift cab; supervisor to verify at start of each shift", "Christina Moore")));

            // Q3 2024 — FACILITY (environmental + PPE)
            AddFAC(new AuditHeader { JobNumber = "FAC-2407-011", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Main Office Building", Location = "Houston, TX", AuditDate = new DateOnly(2024, 7, 16), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q3 2024 office facility compliance audit" },
                new DateTime(2024, 7, 16, 9, 0, 0, DateTimeKind.Utc), new DateTime(2024, 7, 16, 15, 0, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Environmental", 4, "NonConforming", "Spill kits staged but employees unaware of proper usage — training conducted during audit walkthrough", false, "Conduct spill response training for all yard/shop employees; document in training records within 30 days", "Christina Moore"),
                    ("Required Building Signage", 10, "NonConforming", "Fire extinguisher in shop area missing monthly inspection tag — extinguisher tagged out and replaced until inspection completed", false, "Monthly fire extinguisher inspections must be documented on tag; assign designated inspector role", "Christina Moore")));

            AddFAC(new AuditHeader { JobNumber = "FAC-2408-019", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Yard / Shop Building", Location = "Houston, TX", AuditDate = new DateOnly(2024, 8, 27), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q3 2024 yard and shop facility audit" },
                new DateTime(2024, 8, 27, 9, 0, 0, DateTimeKind.Utc), new DateTime(2024, 8, 27, 15, 30, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Hazardous Material Storage", 1, "NonConforming", "Flammable liquids stored in open metal cabinet without proper labeling — cabinet labeled and inventory list posted during audit", true, "All flammables storage cabinets must be labeled with hazard and contents inventory", "Christina Moore")));

            // Q4 2024 — FACILITY (improving)
            AddFAC(new AuditHeader { JobNumber = "FAC-2410-024", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Main Office Building", Location = "Houston, TX", AuditDate = new DateOnly(2024, 10, 8), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q4 2024 office facility compliance audit" },
                new DateTime(2024, 10, 8, 9, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 8, 15, 0, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Required Building Signage", 4, "NonConforming", "Emergency phone numbers list not posted at secondary exit — list printed and posted during audit", true, null, null)));

            AddFAC(new AuditHeader { JobNumber = "FAC-2411-031", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Yard / Shop Building", Location = "Houston, TX", AuditDate = new DateOnly(2024, 11, 12), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q4 2024 yard and shop facility audit" },
                new DateTime(2024, 11, 12, 9, 0, 0, DateTimeKind.Utc), new DateTime(2024, 11, 12, 15, 30, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Housekeeping", 1, "Warning", "Storage containers in yard not fully stable in one section — rearranged during audit to ensure stability", false, null, null)));

            // Q1 2025 — FACILITY (near clean)
            AddFAC(new AuditHeader { JobNumber = "FAC-2501-038", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Main Office Building", Location = "Houston, TX", AuditDate = new DateOnly(2025, 1, 21), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q1 2025 facility compliance audit" },
                new DateTime(2025, 1, 21, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 21, 15, 0, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Required Building Signage", 10, "Warning", "Fire extinguisher inspection tag filled out but monthly date entry smudged and unreadable — extinguisher re-tagged", false, null, null)));

            AddFAC(new AuditHeader { JobNumber = "FAC-2503-045", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Yard / Shop Building", Location = "Houston, TX", AuditDate = new DateOnly(2025, 3, 18), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q1 2025 yard and shop facility audit" },
                new DateTime(2025, 3, 18, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 18, 15, 30, 0, DateTimeKind.Utc),
                FacilityBase());

            // Q2 2025 — FACILITY (clean)
            AddFAC(new AuditHeader { JobNumber = "FAC-2504-051", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Main Office Building", Location = "Houston, TX", AuditDate = new DateOnly(2025, 4, 15), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q2 2025 office facility compliance audit" },
                new DateTime(2025, 4, 15, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 4, 15, 15, 0, 0, DateTimeKind.Utc),
                FacilityBase());

            AddFAC(new AuditHeader { JobNumber = "FAC-2506-059", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Yard / Shop Building", Location = "Houston, TX", AuditDate = new DateOnly(2025, 6, 24), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q2 2025 yard facility audit" },
                new DateTime(2025, 6, 24, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 24, 15, 30, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Environmental", 3, "Warning", "Secondary containment around portable storage tank missing bung plug — plug installed during audit", false, null, null)));

            // Q3 2025 — FACILITY
            AddFAC(new AuditHeader { JobNumber = "FAC-2508-064", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Main Office Building", Location = "Houston, TX", AuditDate = new DateOnly(2025, 8, 5), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q3 2025 office facility compliance audit" },
                new DateTime(2025, 8, 5, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 8, 5, 15, 0, 0, DateTimeKind.Utc),
                FacilityBase());

            AddFAC(new AuditHeader { JobNumber = "FAC-2509-071", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Yard / Shop Building", Location = "Houston, TX", AuditDate = new DateOnly(2025, 9, 10), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q3 2025 yard facility audit" },
                new DateTime(2025, 9, 10, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 15, 30, 0, DateTimeKind.Utc),
                FacilityBase(
                    ("Housekeeping", 0, "Warning", "Shop floor needed sweeping near lathe station — crew addressed immediately during walkthrough", false, null, null)));

            // Q4 2025 — FACILITY (clean)
            AddFAC(new AuditHeader { JobNumber = "FAC-2510-077", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Main Office Building", Location = "Houston, TX", AuditDate = new DateOnly(2025, 10, 14), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q4 2025 office facility compliance audit" },
                new DateTime(2025, 10, 14, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 14, 15, 0, 0, DateTimeKind.Utc),
                FacilityBase());

            AddFAC(new AuditHeader { JobNumber = "FAC-2511-083", Client = "Stronghold Companies HQ", PM = "Christina Moore", Unit = "Yard / Shop Building", Location = "Houston, TX", AuditDate = new DateOnly(2025, 11, 18), Auditor = "J. Hilliard", Shift = "DAY", WorkDescription = "Q4 2025 yard facility audit" },
                new DateTime(2025, 11, 18, 9, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 18, 15, 30, 0, DateTimeKind.Utc),
                FacilityBase());
        }

        if (shirtDiv != null && shirtVer != null)
        {
            void AddRT(AuditHeader hdr, DateTime c, DateTime s, List<ResponseSeed> r) => Add(shirtDiv, shirtVer, hdr, c, s, r);

            // Q2 2024 — SHI_RT (dosimeter issues + permit)
            AddRT(new AuditHeader { JobNumber = "SRT-2404-001", Client = "ExxonMobil Baytown", PM = "Dale Hebert", Unit = "Unit 6 / Desulfurizer", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 4, 8), Auditor = "R. Thibodaux", Shift = "NIGHT", WorkDescription = "Radiographic testing of weld seams on desulfurizer vessel" },
                new DateTime(2024, 4, 8, 19, 0, 0, DateTimeKind.Utc), new DateTime(2024, 4, 9, 3, 30, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 1, "NonConforming", "Personal dosimeter on trainee not alarming at test threshold — removed from service; trainee used backup dosimeter for remainder of shift", false, "Test all alarming dosimeters before entering controlled area; document pass/fail in survey report", "Dale Hebert"),
                    ("SHI (RT) Specific", 5, "NonConforming", "One survey meter battery depleted mid-shift — spare battery not in truck; operations paused until replacement retrieved from yard", false, "Verify spare battery presence at start of every RT shift; note in SIP pre-shift checklist", "Dale Hebert"),
                    ("Permitting", 0, "NonConforming", "Radiographer's card not on person at start of work — located in toolbox; card moved to personal possession for remainder of job", true, null, null)));

            AddRT(new AuditHeader { JobNumber = "SRT-2405-009", Client = "Motiva Port Arthur", PM = "Dale Hebert", Unit = "Unit 3 / Crude Heater", Location = "Port Arthur, TX", AuditDate = new DateOnly(2024, 5, 21), Auditor = "S. Armstrong", Shift = "NIGHT", WorkDescription = "RT inspection of crude heater coil weld seams" },
                new DateTime(2024, 5, 21, 20, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 22, 4, 0, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 3, "NonConforming", "High radiation sign positioned behind collimator instead of around source perimeter — repositioned during audit", true, "High radiation signs must surround the source at the correct distance per state regulations; radiographer to verify placement before exposure", "Dale Hebert"),
                    ("SHI (RT) Specific", 2, "NonConforming", "Survey report did not include pocket dosimeter records for the current shift — records produced from previous day only; current shift records added", false, "Pocket dosimeter records must be current and within survey report binder at all times during RT operations", "Dale Hebert")));

            // Q3 2024 — SHI_RT (source locking + barricades)
            AddRT(new AuditHeader { JobNumber = "SRT-2407-014", Client = "Valero Port Arthur", PM = "Dale Hebert", Unit = "Unit 9 / HDS", Location = "Port Arthur, TX", AuditDate = new DateOnly(2024, 7, 11), Auditor = "C. Wyatt", Shift = "NIGHT", WorkDescription = "Radiographic weld inspection on HDS reactor piping" },
                new DateTime(2024, 7, 11, 19, 30, 0, DateTimeKind.Utc), new DateTime(2024, 7, 12, 3, 45, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 6, "NonConforming", "Exposure device lock not engaged between shots — source container left unlocked during film change; crew corrected immediately", true, "Lock must be re-engaged on exposure device whenever source is not being actively used; radiographer to verbally confirm lock after each shot", "Dale Hebert"),
                    ("SHI (RT) Specific", 4, "Warning", "Collimator alignment slightly off-center for current shot geometry — adjusted before exposure", false, null, null)));

            AddRT(new AuditHeader { JobNumber = "SRT-2409-022", Client = "Enterprise Products Partners", PM = "Dale Hebert", Unit = "Pipeline Tie-In", Location = "Mont Belvieu, TX", AuditDate = new DateOnly(2024, 9, 17), Auditor = "R. Thibodaux", Shift = "NIGHT", WorkDescription = "Radiographic inspection of pipeline butt weld tie-in" },
                new DateTime(2024, 9, 17, 20, 0, 0, DateTimeKind.Utc), new DateTime(2024, 9, 18, 4, 0, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 3, "NonConforming", "Caution signs used as inner barricade instead of high radiation signs — corrected before exposure", false, "High radiation area signs required at inner perimeter; caution signs only at outer boundary", "Dale Hebert")));

            // Q4 2024 — SHI_RT (vehicle + labeling)
            AddRT(new AuditHeader { JobNumber = "SRT-2410-028", Client = "Chevron Phillips Cedar Bayou", PM = "Dale Hebert", Unit = "Unit 4 / Ethylene Cracker", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 10, 22), Auditor = "S. Armstrong", Shift = "NIGHT", WorkDescription = "RT weld inspection on ethylene cracker transfer line" },
                new DateTime(2024, 10, 22, 19, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 23, 3, 30, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 9, "NonConforming", "RT truck did not have SHI company name visible on driver side — label had peeled; replacement label applied on site", true, null, null),
                    ("SHI (RT) Specific", 1, "NonConforming", "Radiographer trainee personal dosimeter battery low — dosimeter exchanged for spare unit before exposure began", false, "Inspect all dosimeter batteries at start of each shift; replace any showing low battery indicator", "Dale Hebert")));

            AddRT(new AuditHeader { JobNumber = "SRT-2411-036", Client = "INEOS Phenol Pasadena", PM = "Dale Hebert", Unit = "Unit 2 / Phenol Column", Location = "Pasadena, TX", AuditDate = new DateOnly(2024, 11, 13), Auditor = "C. Wyatt", Shift = "NIGHT", WorkDescription = "RT inspection of phenol column nozzle welds" },
                new DateTime(2024, 11, 13, 20, 0, 0, DateTimeKind.Utc), new DateTime(2024, 11, 14, 4, 15, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 8, "Warning", "Transport container radioactive labels correctly filled out but activity values slightly faded — labels replaced as precautionary measure", false, null, null)));

            // Q1 2025 — SHI_RT (improving)
            AddRT(new AuditHeader { JobNumber = "SRT-2501-042", Client = "Valero Texas City", PM = "Dale Hebert", Unit = "Unit 11 / Reformer", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 1, 14), Auditor = "R. Thibodaux", Shift = "NIGHT", WorkDescription = "Radiographic inspection of reformer furnace tube welds" },
                new DateTime(2025, 1, 14, 19, 30, 0, DateTimeKind.Utc), new DateTime(2025, 1, 15, 4, 0, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 5, "NonConforming", "Survey meter #2 battery inspection not documented in pre-shift log — inspection conducted and documented during audit; meters confirmed operable", false, "Pre-shift battery inspection for both survey meters must be documented in survey report before first exposure", "Dale Hebert")));

            AddRT(new AuditHeader { JobNumber = "SRT-2502-051", Client = "Marathon Galveston Bay", PM = "Dale Hebert", Unit = "Pipeline Repair", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 2, 19), Auditor = "S. Armstrong", Shift = "NIGHT", WorkDescription = "RT weld inspection on pipeline repair tie-in" },
                new DateTime(2025, 2, 19, 20, 0, 0, DateTimeKind.Utc), new DateTime(2025, 2, 20, 4, 30, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 2, "Warning", "Survey report had minor date entry error for first exposure — corrected with initials during audit", false, null, null)));

            // Q2 2025 — SHI_RT (near clean)
            AddRT(new AuditHeader { JobNumber = "SRT-2504-058", Client = "ExxonMobil Baytown", PM = "Dale Hebert", Unit = "Unit 7 / Alkylation", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 4, 9), Auditor = "C. Wyatt", Shift = "NIGHT", WorkDescription = "RT inspection of alkylation unit piping welds" },
                new DateTime(2025, 4, 9, 19, 0, 0, DateTimeKind.Utc), new DateTime(2025, 4, 10, 3, 30, 0, DateTimeKind.Utc),
                SHIRTBase());

            AddRT(new AuditHeader { JobNumber = "SRT-2506-067", Client = "Motiva Port Arthur", PM = "Dale Hebert", Unit = "Unit 5 / Coker Piping", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 6, 17), Auditor = "R. Thibodaux", Shift = "NIGHT", WorkDescription = "RT inspection of coker unit piping replacement welds" },
                new DateTime(2025, 6, 17, 20, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 18, 4, 0, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 11, "Warning", "High radiation cones in truck — all four present but one had minor crack in base; replaced with spare from yard before next job", false, null, null)));

            // Q3 2025 — SHI_RT (clean)
            AddRT(new AuditHeader { JobNumber = "SRT-2507-072", Client = "Valero Port Arthur", PM = "Dale Hebert", Unit = "Unit 9 / HDS Piping", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 7, 22), Auditor = "S. Armstrong", Shift = "NIGHT", WorkDescription = "RT inspection of HDS unit piping welds" },
                new DateTime(2025, 7, 22, 19, 30, 0, DateTimeKind.Utc), new DateTime(2025, 7, 23, 4, 0, 0, DateTimeKind.Utc),
                SHIRTBase());

            AddRT(new AuditHeader { JobNumber = "SRT-2509-081", Client = "Chevron Phillips Cedar Bayou", PM = "Dale Hebert", Unit = "Unit 4 / Pipeline Tie-In", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 9, 9), Auditor = "C. Wyatt", Shift = "NIGHT", WorkDescription = "RT weld inspection on new pipeline tie-in" },
                new DateTime(2025, 9, 9, 20, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 10, 4, 30, 0, DateTimeKind.Utc),
                SHIRTBase());

            // Q4 2025 — SHI_RT (clean)
            AddRT(new AuditHeader { JobNumber = "SRT-2510-089", Client = "INEOS Phenol Pasadena", PM = "Dale Hebert", Unit = "Unit 3 / Reactor Piping", Location = "Pasadena, TX", AuditDate = new DateOnly(2025, 10, 8), Auditor = "R. Thibodaux", Shift = "NIGHT", WorkDescription = "RT inspection of reactor piping nozzle welds" },
                new DateTime(2025, 10, 8, 19, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 9, 3, 30, 0, DateTimeKind.Utc),
                SHIRTBase());

            AddRT(new AuditHeader { JobNumber = "SRT-2511-097", Client = "Marathon Galveston Bay", PM = "Dale Hebert", Unit = "Reformer Piping", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 11, 12), Auditor = "S. Armstrong", Shift = "NIGHT", WorkDescription = "RT inspection of reformer piping replacement welds" },
                new DateTime(2025, 11, 12, 20, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 13, 4, 0, 0, DateTimeKind.Utc),
                SHIRTBase(
                    ("SHI (RT) Specific", 1, "Warning", "Trainee dosimeter alarm threshold recently adjusted — functioning correctly but pre-shift test not performed; test conducted during audit", false, null, null)));
        }

        if (shiraDiv != null && shiraVer != null)
        {
            void AddRA(AuditHeader hdr, DateTime c, DateTime s, List<ResponseSeed> r) => Add(shiraDiv, shiraVer, hdr, c, s, r);

            // Q2 2024 — SHI_RA (anchor + rescue plan issues)
            AddRA(new AuditHeader { JobNumber = "SRA-2404-001", Client = "ExxonMobil Baytown", PM = "Cody Boudreaux", Unit = "Unit 8 / Reactor External", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 4, 16), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "External rope access inspection of reactor vessel nozzles and piping" },
                new DateTime(2024, 4, 16, 7, 30, 0, DateTimeKind.Utc), new DateTime(2024, 4, 16, 16, 0, 0, DateTimeKind.Utc),
                SHIRABase(
                    ("SHI (RA) Specific", 1, "NonConforming", "Rescue plan not completed before work began — rope access work halted until written rescue plan signed off by PM", false, "Rescue plan must be completed and signed before any rope access work begins; PM to verify before first technician ascends", "Cody Boudreaux"),
                    ("SHI (RA) Specific", 3, "NonConforming", "Primary anchor rigged on non-load-rated beam — rigging removed and relocated to certified anchor point", false, "Anchor point load ratings must be verified and documented in rescue plan before rigging; competent person to sign off", "Cody Boudreaux"),
                    ("Personal Protective Equipment", 2, "NonConforming", "Fall protection harness on technician #2 had color-code tag expired — harness removed from service and replaced", false, "Harness color-code tag compliance must be verified in equipment inspection log before work begins", "Cody Boudreaux")));

            AddRA(new AuditHeader { JobNumber = "SRA-2405-007", Client = "Valero Port Arthur", PM = "Cody Boudreaux", Unit = "Unit 4 / Fractionator External", Location = "Port Arthur, TX", AuditDate = new DateOnly(2024, 5, 28), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "Rope access inspection of fractionator tower external shell and nozzles" },
                new DateTime(2024, 5, 28, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 5, 28, 16, 30, 0, DateTimeKind.Utc),
                SHIRABase(
                    ("SHI (RA) Specific", 2, "NonConforming", "Access zone not fully established before technicians rigged — one unsecured area left near adjacent vessel; zone completed before any ascent", false, "Access zone must encompass entire work area plus a minimum buffer; competent person to verify before ascending", "Cody Boudreaux"),
                    ("SHI (RA) Specific", 4, "NonConforming", "Equipment inspection log did not include ascenders for one technician — inspection conducted during audit; log updated", false, "Full equipment inspection documentation required for every technician every day; PM to verify log completeness before first ascent", "Cody Boudreaux")));

            // Q3 2024 — SHI_RA (PPE + rescue plan)
            AddRA(new AuditHeader { JobNumber = "SRA-2407-011", Client = "Chevron Phillips Cedar Bayou", PM = "Cody Boudreaux", Unit = "Unit 3 / Splitter External", Location = "Baytown, TX", AuditDate = new DateOnly(2024, 7, 9), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "External rope access inspection of C3 splitter tower shell" },
                new DateTime(2024, 7, 9, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 7, 9, 16, 0, 0, DateTimeKind.Utc),
                SHIRABase(
                    ("SHI (RA) Specific", 1, "NonConforming", "Rescue plan completed but not reviewed with full crew before work began — crew briefing conducted during audit", false, "All crew members including support/attendant must review and sign rescue plan before first ascent", "Cody Boudreaux"),
                    ("Personal Protective Equipment", 0, "NonConforming", "One technician not wearing required helmet with chin strap secured — corrected immediately on site", true, null, null)));

            AddRA(new AuditHeader { JobNumber = "SRA-2409-019", Client = "INEOS Phenol Pasadena", PM = "Cody Boudreaux", Unit = "Unit 6 / Phenol Column External", Location = "Pasadena, TX", AuditDate = new DateOnly(2024, 9, 24), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "External rope access NDT inspection of phenol column external shell" },
                new DateTime(2024, 9, 24, 7, 45, 0, DateTimeKind.Utc), new DateTime(2024, 9, 24, 16, 15, 0, DateTimeKind.Utc),
                SHIRABase(
                    ("SHI (RA) Specific", 3, "Warning", "Anchorage rigging was adequate but anchor inspection documentation did not specify load rating — documentation updated", false, null, null)));

            // Q4 2024 — SHI_RA (improving)
            AddRA(new AuditHeader { JobNumber = "SRA-2410-025", Client = "Motiva Port Arthur", PM = "Cody Boudreaux", Unit = "Unit 2 / Coker External", Location = "Port Arthur, TX", AuditDate = new DateOnly(2024, 10, 14), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "External rope access inspection of coke drum external shell and nozzles" },
                new DateTime(2024, 10, 14, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 10, 14, 16, 0, 0, DateTimeKind.Utc),
                SHIRABase(
                    ("SHI (RA) Specific", 4, "NonConforming", "Rope inspection log entries for carabiner set not completed — crew produced inspection records after 20-minute delay; future inspections to be performed at day start", false, "Equipment inspection log must be completed and signed at start of each workday before any ascending begins", "Cody Boudreaux")));

            AddRA(new AuditHeader { JobNumber = "SRA-2411-032", Client = "Marathon Galveston Bay", PM = "Cody Boudreaux", Unit = "Reformer External", Location = "Texas City, TX", AuditDate = new DateOnly(2024, 11, 5), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "External rope access inspection of reformer furnace stack" },
                new DateTime(2024, 11, 5, 8, 0, 0, DateTimeKind.Utc), new DateTime(2024, 11, 5, 16, 30, 0, DateTimeKind.Utc),
                SHIRABase(
                    ("SHI (RA) Specific", 2, "Warning", "Access zone boundary markers partially removed by adjacent contractor crew — zone re-established and communication sent to adjacent foreman", false, null, null)));

            // Q1 2025 — SHI_RA (near clean)
            AddRA(new AuditHeader { JobNumber = "SRA-2501-038", Client = "Valero Port Arthur", PM = "Cody Boudreaux", Unit = "Unit 7 / Splitter External", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 1, 21), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "External rope access inspection of vacuum splitter tower" },
                new DateTime(2025, 1, 21, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 21, 16, 0, 0, DateTimeKind.Utc),
                SHIRABase(
                    ("SHI (RA) Specific", 1, "Warning", "Rescue plan completed and signed but rescue team contact number was the old direct line — updated to current emergency number during audit", false, null, null)));

            AddRA(new AuditHeader { JobNumber = "SRA-2503-046", Client = "ExxonMobil Baytown", PM = "Cody Boudreaux", Unit = "Unit 9 / Reactor External", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 3, 11), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "External rope access inspection of high-pressure reactor vessel nozzles" },
                new DateTime(2025, 3, 11, 7, 30, 0, DateTimeKind.Utc), new DateTime(2025, 3, 11, 16, 0, 0, DateTimeKind.Utc),
                SHIRABase());

            // Q2 2025 — SHI_RA (clean)
            AddRA(new AuditHeader { JobNumber = "SRA-2504-053", Client = "Chevron Phillips Cedar Bayou", PM = "Cody Boudreaux", Unit = "Unit 5 / Polyethylene Tower External", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 4, 22), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "External rope access inspection of polyethylene splitter tower" },
                new DateTime(2025, 4, 22, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 4, 22, 16, 30, 0, DateTimeKind.Utc),
                SHIRABase());

            AddRA(new AuditHeader { JobNumber = "SRA-2506-061", Client = "INEOS Phenol Pasadena", PM = "Cody Boudreaux", Unit = "Unit 4 / Reactor External", Location = "Pasadena, TX", AuditDate = new DateOnly(2025, 6, 17), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "External rope access NDT on phenol reactor shell" },
                new DateTime(2025, 6, 17, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 17, 16, 0, 0, DateTimeKind.Utc),
                SHIRABase());

            // Q3 2025 — SHI_RA
            AddRA(new AuditHeader { JobNumber = "SRA-2507-067", Client = "Valero Port Arthur", PM = "Cody Boudreaux", Unit = "Atmospheric Tower External", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 7, 15), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "External rope access inspection of atmospheric distillation tower" },
                new DateTime(2025, 7, 15, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 7, 15, 16, 0, 0, DateTimeKind.Utc),
                SHIRABase());

            AddRA(new AuditHeader { JobNumber = "SRA-2509-075", Client = "Motiva Port Arthur", PM = "Cody Boudreaux", Unit = "Coker External", Location = "Port Arthur, TX", AuditDate = new DateOnly(2025, 9, 9), Auditor = "R. Thibodaux", Shift = "DAY", WorkDescription = "External rope access inspection of coke drum drum shell and top head" },
                new DateTime(2025, 9, 9, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 9, 9, 16, 30, 0, DateTimeKind.Utc),
                SHIRABase(
                    ("SHI (RA) Specific", 4, "Warning", "Technician rope inspection log complete but carabiner photos not included as required — photos uploaded by end of shift", false, null, null)));

            // Q4 2025 — SHI_RA (clean)
            AddRA(new AuditHeader { JobNumber = "SRA-2510-081", Client = "Marathon Galveston Bay", PM = "Cody Boudreaux", Unit = "Reformer Stack External", Location = "Texas City, TX", AuditDate = new DateOnly(2025, 10, 14), Auditor = "C. Wyatt", Shift = "DAY", WorkDescription = "External rope access inspection of reformer stack external shell" },
                new DateTime(2025, 10, 14, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 10, 14, 16, 0, 0, DateTimeKind.Utc),
                SHIRABase());

            AddRA(new AuditHeader { JobNumber = "SRA-2511-089", Client = "ExxonMobil Baytown", PM = "Cody Boudreaux", Unit = "Reactor Vessel External", Location = "Baytown, TX", AuditDate = new DateOnly(2025, 11, 19), Auditor = "S. Armstrong", Shift = "DAY", WorkDescription = "External rope access inspection of high-pressure reactor vessel external inspection ports" },
                new DateTime(2025, 11, 19, 8, 0, 0, DateTimeKind.Utc), new DateTime(2025, 11, 19, 16, 30, 0, DateTimeKind.Utc),
                SHIRABase());
        }

        return list;
    }

    // SHI all-conforming base with optional response overrides
    private static List<ResponseSeed> SHIBase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("SHI", "Permitting", 0, "Conforming"), R("SHI", "Permitting", 1, "Conforming"),
            R("SHI", "Permitting", 2, "NA"), R("SHI", "Permitting", 3, "NA"),
            R("SHI", "Personal Protective Equipment", 0, "Conforming"),
            R("SHI", "Personal Protective Equipment", 1, "Conforming"),
            R("SHI", "Personal Protective Equipment", 2, "Conforming"),
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
            R("SHI", "Job Site & Confined Space Condition", 0, "Conforming"),
            R("SHI", "Job Site & Confined Space Condition", 1, "Conforming"),
            R("SHI", "Job Site & Confined Space Condition", 2, "Conforming"),
            R("SHI", "Job Site & Confined Space Condition", 3, "Conforming"),
            R("SHI", "Job Site & Confined Space Condition", 4, "Conforming"),
            R("SHI", "Job Site & Confined Space Condition", 5, "Conforming"),
            R("SHI", "Job Site & Confined Space Condition", 6, "Conforming"),
            R("SHI", "Job Site & Confined Space Condition", 7, "Conforming"),
            R("SHI", "Scaffolds", 0, "Conforming"), R("SHI", "Scaffolds", 1, "Conforming"), R("SHI", "Scaffolds", 2, "Conforming"),
            R("SHI", "Lock-Out / Tag-Out", 0, "Conforming"), R("SHI", "Lock-Out / Tag-Out", 1, "Conforming"),
            R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
            R("SHI", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
            R("SHI", "JHA / JSA", 0, "Conforming"), R("SHI", "JHA / JSA", 1, "Conforming"),
            R("SHI", "JHA / JSA", 2, "Conforming"), R("SHI", "JHA / JSA", 3, "Conforming"),
            R("SHI", "JHA / JSA", 4, "Conforming"), R("SHI", "JHA / JSA", 5, "Conforming"),
            R("SHI", "JHA / JSA", 6, "Conforming"), R("SHI", "JHA / JSA", 7, "Conforming"),
            R("SHI", "JHA / JSA", 8, "Conforming"),
            R("SHI", "Culture / Attitudes", 0, "Conforming"), R("SHI", "Culture / Attitudes", 1, "Conforming"),
            R("SHI", "Culture / Attitudes", 2, "Conforming"), R("SHI", "Culture / Attitudes", 3, "Conforming"),
            R("SHI", "Culture / Attitudes", 4, "Conforming"), R("SHI", "Culture / Attitudes", 5, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "SHI", sec, idx, st, c, os, ca, at);
        return r;
    }

    // ETS all-conforming base with optional response overrides
    private static List<ResponseSeed> ETSBase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("ETS", "Permitting", 0, "Conforming"), R("ETS", "Permitting", 1, "Conforming"),
            R("ETS", "Permitting", 2, "Conforming"), R("ETS", "Permitting", 3, "Conforming"),
            R("ETS", "Permitting", 4, "NA"), R("ETS", "Permitting", 5, "NA"),
            R("ETS", "Personal Protective Equipment", 0, "Conforming"),
            R("ETS", "Personal Protective Equipment", 1, "Conforming"),
            R("ETS", "Personal Protective Equipment", 2, "Conforming"),
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
            R("ETS", "Scaffolds", 0, "Conforming"), R("ETS", "Scaffolds", 1, "Conforming"), R("ETS", "Scaffolds", 2, "Conforming"),
            R("ETS", "Lock-Out / Tag-Out", 0, "Conforming"), R("ETS", "Lock-Out / Tag-Out", 1, "Conforming"), R("ETS", "Lock-Out / Tag-Out", 2, "Conforming"),
            R("ETS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
            R("ETS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
            R("ETS", "JHA / JSA", 0, "Conforming"), R("ETS", "JHA / JSA", 1, "Conforming"),
            R("ETS", "JHA / JSA", 2, "Conforming"), R("ETS", "JHA / JSA", 3, "Conforming"),
            R("ETS", "Training / Dispatch", 0, "Conforming"), R("ETS", "Training / Dispatch", 1, "Conforming"), R("ETS", "Training / Dispatch", 2, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "ETS", sec, idx, st, c, os, ca, at);
        return r;
    }

    // TKIE all-conforming base (pipeline work — Scaffolds always NA, Equipment idx 0-1 NA)
    private static List<ResponseSeed> TKIEBase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("TKIE", "Permitting", 0, "Conforming"), R("TKIE", "Permitting", 1, "Conforming"),
            R("TKIE", "Permitting", 2, "NA"), R("TKIE", "Permitting", 3, "NA"),
            R("TKIE", "Personal Protective Equipment", 0, "Conforming"),
            R("TKIE", "Personal Protective Equipment", 1, "Conforming"),
            R("TKIE", "Personal Protective Equipment", 2, "Conforming"),
            R("TKIE", "Equipment & Equipment Inspection", 0, "NA"), R("TKIE", "Equipment & Equipment Inspection", 1, "NA"),
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
            R("TKIE", "Scaffolds", 0, "NA"), R("TKIE", "Scaffolds", 1, "NA"), R("TKIE", "Scaffolds", 2, "NA"),
            R("TKIE", "Lock-Out / Tag-Out", 0, "Conforming"), R("TKIE", "Lock-Out / Tag-Out", 1, "Conforming"),
            R("TKIE", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
            R("TKIE", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
            R("TKIE", "Daily Job Logs", 0, "Conforming"),
            R("TKIE", "QA / QC Documentation", 0, "Conforming"),
            R("TKIE", "QA / QC Documentation", 1, "Conforming"),
            R("TKIE", "QA / QC Documentation", 2, "Conforming"),
            R("TKIE", "JHA / JSA", 0, "Conforming"), R("TKIE", "JHA / JSA", 1, "Conforming"),
            R("TKIE", "JHA / JSA", 2, "Conforming"), R("TKIE", "JHA / JSA", 3, "Conforming"),
            R("TKIE", "JHA / JSA", 4, "Conforming"), R("TKIE", "JHA / JSA", 5, "Conforming"),
            R("TKIE", "JHA / JSA", 6, "Conforming"), R("TKIE", "JHA / JSA", 7, "Conforming"),
            R("TKIE", "JHA / JSA", 8, "Conforming"),
            R("TKIE", "Culture / Attitudes", 0, "Conforming"), R("TKIE", "Culture / Attitudes", 1, "Conforming"),
            R("TKIE", "Culture / Attitudes", 2, "Conforming"), R("TKIE", "Culture / Attitudes", 3, "Conforming"),
            R("TKIE", "Culture / Attitudes", 4, "Conforming"), R("TKIE", "Culture / Attitudes", 5, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "TKIE", sec, idx, st, c, os, ca, at);
        return r;
    }

    // STS all-conforming base (same section structure as TKIE)
    private static List<ResponseSeed> STSBase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("STS", "Permitting", 0, "Conforming"), R("STS", "Permitting", 1, "Conforming"),
            R("STS", "Permitting", 2, "Conforming"), R("STS", "Permitting", 3, "Conforming"),
            R("STS", "Personal Protective Equipment", 0, "Conforming"),
            R("STS", "Personal Protective Equipment", 1, "Conforming"),
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
            R("STS", "Scaffolds", 0, "Conforming"), R("STS", "Scaffolds", 1, "Conforming"), R("STS", "Scaffolds", 2, "Conforming"),
            R("STS", "Lock-Out / Tag-Out", 0, "Conforming"), R("STS", "Lock-Out / Tag-Out", 1, "Conforming"),
            R("STS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
            R("STS", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
            R("STS", "Daily Job Logs", 0, "Conforming"),
            R("STS", "QA / QC Documentation", 0, "Conforming"),
            R("STS", "QA / QC Documentation", 1, "Conforming"),
            R("STS", "QA / QC Documentation", 2, "Conforming"),
            R("STS", "JHA / JSA", 0, "Conforming"), R("STS", "JHA / JSA", 1, "Conforming"),
            R("STS", "JHA / JSA", 2, "Conforming"), R("STS", "JHA / JSA", 3, "Conforming"),
            R("STS", "JHA / JSA", 4, "Conforming"), R("STS", "JHA / JSA", 5, "Conforming"),
            R("STS", "JHA / JSA", 6, "Conforming"), R("STS", "JHA / JSA", 7, "Conforming"),
            R("STS", "JHA / JSA", 8, "Conforming"),
            R("STS", "Culture / Attitudes", 0, "Conforming"), R("STS", "Culture / Attitudes", 1, "Conforming"),
            R("STS", "Culture / Attitudes", 2, "Conforming"), R("STS", "Culture / Attitudes", 3, "Conforming"),
            R("STS", "Culture / Attitudes", 4, "Conforming"), R("STS", "Culture / Attitudes", 5, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "STS", sec, idx, st, c, os, ca, at);
        return r;
    }

    // STG all-conforming base (Permitting 6 items, JHA 4 items, Training/Dispatch — no Culture/Attitudes)
    private static List<ResponseSeed> STGBase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("STG", "Permitting", 0, "Conforming"), R("STG", "Permitting", 1, "Conforming"),
            R("STG", "Permitting", 2, "Conforming"), R("STG", "Permitting", 3, "Conforming"),
            R("STG", "Permitting", 4, "Conforming"), R("STG", "Permitting", 5, "Conforming"),
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
            R("STG", "Scaffolds", 0, "Conforming"), R("STG", "Scaffolds", 1, "Conforming"), R("STG", "Scaffolds", 2, "Conforming"),
            R("STG", "Lock-Out / Tag-Out", 0, "Conforming"), R("STG", "Lock-Out / Tag-Out", 1, "Conforming"), R("STG", "Lock-Out / Tag-Out", 2, "Conforming"),
            R("STG", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
            R("STG", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
            R("STG", "JHA / JSA", 0, "Conforming"), R("STG", "JHA / JSA", 1, "Conforming"),
            R("STG", "JHA / JSA", 2, "Conforming"), R("STG", "JHA / JSA", 3, "Conforming"),
            R("STG", "Training / Dispatch", 0, "Conforming"),
            R("STG", "Training / Dispatch", 1, "Conforming"),
            R("STG", "Training / Dispatch", 2, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "STG", sec, idx, st, c, os, ca, at);
        return r;
    }

    // CSL all-conforming base (same standard sections + CSL Specific, Other is heat-stress only)
    private static List<ResponseSeed> CSLBase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("CSL", "Permitting", 0, "Conforming"), R("CSL", "Permitting", 1, "Conforming"),
            R("CSL", "Permitting", 2, "Conforming"), R("CSL", "Permitting", 3, "Conforming"),
            R("CSL", "Personal Protective Equipment", 0, "Conforming"),
            R("CSL", "Personal Protective Equipment", 1, "Conforming"),
            R("CSL", "Personal Protective Equipment", 2, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 0, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 1, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 2, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 3, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 4, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 5, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 6, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 7, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 8, "Conforming"),
            R("CSL", "Equipment & Equipment Inspection", 9, "Conforming"),
            R("CSL", "Job Site & Confined Space Condition", 0, "Conforming"),
            R("CSL", "Job Site & Confined Space Condition", 1, "Conforming"),
            R("CSL", "Job Site & Confined Space Condition", 2, "Conforming"),
            R("CSL", "Job Site & Confined Space Condition", 3, "Conforming"),
            R("CSL", "Job Site & Confined Space Condition", 4, "Conforming"),
            R("CSL", "Job Site & Confined Space Condition", 5, "Conforming"),
            R("CSL", "Job Site & Confined Space Condition", 6, "Conforming"),
            R("CSL", "Job Site & Confined Space Condition", 7, "Conforming"),
            R("CSL", "Scaffolds", 0, "Conforming"), R("CSL", "Scaffolds", 1, "Conforming"), R("CSL", "Scaffolds", 2, "Conforming"),
            R("CSL", "Lock-Out / Tag-Out", 0, "Conforming"), R("CSL", "Lock-Out / Tag-Out", 1, "Conforming"),
            R("CSL", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
            R("CSL", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
            R("CSL", "JHA / JSA", 0, "Conforming"), R("CSL", "JHA / JSA", 1, "Conforming"),
            R("CSL", "JHA / JSA", 2, "Conforming"), R("CSL", "JHA / JSA", 3, "Conforming"),
            R("CSL", "JHA / JSA", 4, "Conforming"), R("CSL", "JHA / JSA", 5, "Conforming"),
            R("CSL", "JHA / JSA", 6, "Conforming"), R("CSL", "JHA / JSA", 7, "Conforming"),
            R("CSL", "JHA / JSA", 8, "Conforming"),
            R("CSL", "Culture / Attitudes", 0, "Conforming"), R("CSL", "Culture / Attitudes", 1, "Conforming"),
            R("CSL", "Culture / Attitudes", 2, "Conforming"), R("CSL", "Culture / Attitudes", 3, "Conforming"),
            R("CSL", "Culture / Attitudes", 4, "Conforming"), R("CSL", "Culture / Attitudes", 5, "Conforming"),
            R("CSL", "CSL Specific", 0, "Conforming"), R("CSL", "CSL Specific", 1, "Conforming"),
            R("CSL", "CSL Specific", 2, "Conforming"), R("CSL", "CSL Specific", 3, "Conforming"),
            R("CSL", "CSL Specific", 4, "Conforming"), R("CSL", "CSL Specific", 5, "Conforming"),
            R("CSL", "CSL Specific", 6, "Conforming"), R("CSL", "CSL Specific", 7, "Conforming"),
            R("CSL", "CSL Specific", 8, "Conforming"), R("CSL", "CSL Specific", 9, "Conforming"),
            R("CSL", "CSL Specific", 10, "Conforming"), R("CSL", "CSL Specific", 11, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "CSL", sec, idx, st, c, os, ca, at);
        return r;
    }

    // FACILITY all-conforming base
    private static List<ResponseSeed> FacilityBase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("FACILITY", "Required Building Signage", 0, "Conforming"),
            R("FACILITY", "Required Building Signage", 1, "Conforming"),
            R("FACILITY", "Required Building Signage", 2, "Conforming"),
            R("FACILITY", "Required Building Signage", 3, "Conforming"),
            R("FACILITY", "Required Building Signage", 4, "Conforming"),
            R("FACILITY", "Required Building Signage", 5, "Conforming"),
            R("FACILITY", "Required Building Signage", 6, "Conforming"),
            R("FACILITY", "Required Building Signage", 7, "Conforming"),
            R("FACILITY", "Required Building Signage", 8, "Conforming"),
            R("FACILITY", "Required Building Signage", 9, "Conforming"),
            R("FACILITY", "Required Building Signage", 10, "Conforming"),
            R("FACILITY", "Required Building Signage", 11, "Conforming"),
            R("FACILITY", "Housekeeping", 0, "Conforming"), R("FACILITY", "Housekeeping", 1, "Conforming"),
            R("FACILITY", "Environmental", 0, "Conforming"), R("FACILITY", "Environmental", 1, "Conforming"),
            R("FACILITY", "Environmental", 2, "Conforming"), R("FACILITY", "Environmental", 3, "Conforming"),
            R("FACILITY", "Environmental", 4, "Conforming"), R("FACILITY", "Environmental", 5, "Conforming"),
            R("FACILITY", "Environmental", 6, "Conforming"), R("FACILITY", "Environmental", 7, "Conforming"),
            R("FACILITY", "Equipment", 0, "Conforming"), R("FACILITY", "Equipment", 1, "Conforming"),
            R("FACILITY", "Equipment", 2, "Conforming"), R("FACILITY", "Equipment", 3, "Conforming"),
            R("FACILITY", "Equipment", 4, "Conforming"), R("FACILITY", "Equipment", 5, "Conforming"),
            R("FACILITY", "Equipment", 6, "Conforming"), R("FACILITY", "Equipment", 7, "Conforming"),
            R("FACILITY", "Equipment", 8, "Conforming"), R("FACILITY", "Equipment", 9, "Conforming"),
            R("FACILITY", "Equipment", 10, "Conforming"), R("FACILITY", "Equipment", 11, "Conforming"),
            R("FACILITY", "Equipment", 12, "Conforming"),
            R("FACILITY", "Hazardous Material Storage", 0, "Conforming"),
            R("FACILITY", "Hazardous Material Storage", 1, "Conforming"),
            R("FACILITY", "Hazardous Material Storage", 2, "Conforming"),
            R("FACILITY", "Hazardous Material Storage", 3, "Conforming"),
            R("FACILITY", "Personal Protective Equipment", 0, "Conforming"),
            R("FACILITY", "Personal Protective Equipment", 1, "Conforming"),
            R("FACILITY", "QA / QC", 0, "Conforming"), R("FACILITY", "QA / QC", 1, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "FACILITY", sec, idx, st, c, os, ca, at);
        return r;
    }

    // SHI (RT) all-conforming base (radiation technology work — no Daily Logs, QA/QC, Service Receipts)
    private static List<ResponseSeed> SHIRTBase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("SHI_RT", "Permitting", 0, "Conforming"), R("SHI_RT", "Permitting", 1, "Conforming"),
            R("SHI_RT", "Permitting", 2, "NA"), R("SHI_RT", "Permitting", 3, "NA"),
            R("SHI_RT", "Personal Protective Equipment", 0, "Conforming"),
            R("SHI_RT", "Personal Protective Equipment", 1, "Conforming"),
            R("SHI_RT", "Personal Protective Equipment", 2, "Conforming"),
            R("SHI_RT", "Equipment & Equipment Inspection", 0, "NA"),
            R("SHI_RT", "Equipment & Equipment Inspection", 1, "NA"),
            R("SHI_RT", "Equipment & Equipment Inspection", 2, "Conforming"),
            R("SHI_RT", "Equipment & Equipment Inspection", 3, "Conforming"),
            R("SHI_RT", "Equipment & Equipment Inspection", 4, "Conforming"),
            R("SHI_RT", "Equipment & Equipment Inspection", 5, "Conforming"),
            R("SHI_RT", "Equipment & Equipment Inspection", 6, "Conforming"),
            R("SHI_RT", "Equipment & Equipment Inspection", 7, "Conforming"),
            R("SHI_RT", "Equipment & Equipment Inspection", 8, "Conforming"),
            R("SHI_RT", "Equipment & Equipment Inspection", 9, "Conforming"),
            R("SHI_RT", "Job Site & Confined Space Condition", 0, "Conforming"),
            R("SHI_RT", "Job Site & Confined Space Condition", 1, "NA"),
            R("SHI_RT", "Job Site & Confined Space Condition", 2, "Conforming"),
            R("SHI_RT", "Job Site & Confined Space Condition", 3, "Conforming"),
            R("SHI_RT", "Job Site & Confined Space Condition", 4, "NA"),
            R("SHI_RT", "Job Site & Confined Space Condition", 5, "Conforming"),
            R("SHI_RT", "Job Site & Confined Space Condition", 6, "Conforming"),
            R("SHI_RT", "Job Site & Confined Space Condition", 7, "Conforming"),
            R("SHI_RT", "Scaffolds", 0, "NA"), R("SHI_RT", "Scaffolds", 1, "NA"), R("SHI_RT", "Scaffolds", 2, "NA"),
            R("SHI_RT", "Lock-Out / Tag-Out", 0, "Conforming"), R("SHI_RT", "Lock-Out / Tag-Out", 1, "Conforming"),
            R("SHI_RT", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
            R("SHI_RT", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
            R("SHI_RT", "JHA / JSA", 0, "Conforming"), R("SHI_RT", "JHA / JSA", 1, "Conforming"),
            R("SHI_RT", "JHA / JSA", 2, "Conforming"), R("SHI_RT", "JHA / JSA", 3, "Conforming"),
            R("SHI_RT", "JHA / JSA", 4, "Conforming"), R("SHI_RT", "JHA / JSA", 5, "Conforming"),
            R("SHI_RT", "JHA / JSA", 6, "Conforming"), R("SHI_RT", "JHA / JSA", 7, "Conforming"),
            R("SHI_RT", "JHA / JSA", 8, "Conforming"),
            R("SHI_RT", "Culture / Attitudes", 0, "Conforming"), R("SHI_RT", "Culture / Attitudes", 1, "Conforming"),
            R("SHI_RT", "Culture / Attitudes", 2, "Conforming"), R("SHI_RT", "Culture / Attitudes", 3, "Conforming"),
            R("SHI_RT", "Culture / Attitudes", 4, "Conforming"), R("SHI_RT", "Culture / Attitudes", 5, "Conforming"),
            R("SHI_RT", "SHI (RT) Specific", 0, "Conforming"), R("SHI_RT", "SHI (RT) Specific", 1, "Conforming"),
            R("SHI_RT", "SHI (RT) Specific", 2, "Conforming"), R("SHI_RT", "SHI (RT) Specific", 3, "Conforming"),
            R("SHI_RT", "SHI (RT) Specific", 4, "Conforming"), R("SHI_RT", "SHI (RT) Specific", 5, "Conforming"),
            R("SHI_RT", "SHI (RT) Specific", 6, "Conforming"), R("SHI_RT", "SHI (RT) Specific", 7, "Conforming"),
            R("SHI_RT", "SHI (RT) Specific", 8, "Conforming"), R("SHI_RT", "SHI (RT) Specific", 9, "Conforming"),
            R("SHI_RT", "SHI (RT) Specific", 10, "Conforming"), R("SHI_RT", "SHI (RT) Specific", 11, "Conforming"),
            R("SHI_RT", "SHI (RT) Specific", 12, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "SHI_RT", sec, idx, st, c, os, ca, at);
        return r;
    }

    // SHI (RA) all-conforming base (rope access — same standard sections + SHI (RA) Specific)
    private static List<ResponseSeed> SHIRABase(params (string sec, int idx, string st, string? c, bool os, string? ca, string? at)[] ov)
    {
        var r = new List<ResponseSeed>
        {
            R("SHI_RA", "Permitting", 0, "Conforming"), R("SHI_RA", "Permitting", 1, "Conforming"),
            R("SHI_RA", "Permitting", 2, "NA"), R("SHI_RA", "Permitting", 3, "NA"),
            R("SHI_RA", "Personal Protective Equipment", 0, "Conforming"),
            R("SHI_RA", "Personal Protective Equipment", 1, "Conforming"),
            R("SHI_RA", "Personal Protective Equipment", 2, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 0, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 1, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 2, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 3, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 4, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 5, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 6, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 7, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 8, "Conforming"),
            R("SHI_RA", "Equipment & Equipment Inspection", 9, "Conforming"),
            R("SHI_RA", "Job Site & Confined Space Condition", 0, "Conforming"),
            R("SHI_RA", "Job Site & Confined Space Condition", 1, "Conforming"),
            R("SHI_RA", "Job Site & Confined Space Condition", 2, "Conforming"),
            R("SHI_RA", "Job Site & Confined Space Condition", 3, "Conforming"),
            R("SHI_RA", "Job Site & Confined Space Condition", 4, "Conforming"),
            R("SHI_RA", "Job Site & Confined Space Condition", 5, "Conforming"),
            R("SHI_RA", "Job Site & Confined Space Condition", 6, "Conforming"),
            R("SHI_RA", "Job Site & Confined Space Condition", 7, "Conforming"),
            R("SHI_RA", "Scaffolds", 0, "NA"), R("SHI_RA", "Scaffolds", 1, "NA"), R("SHI_RA", "Scaffolds", 2, "NA"),
            R("SHI_RA", "Lock-Out / Tag-Out", 0, "Conforming"), R("SHI_RA", "Lock-Out / Tag-Out", 1, "Conforming"),
            R("SHI_RA", "Sign-In / Sign-Out Rosters - Toolbox Safety", 0, "Conforming"),
            R("SHI_RA", "Sign-In / Sign-Out Rosters - Toolbox Safety", 1, "Conforming"),
            R("SHI_RA", "JHA / JSA", 0, "Conforming"), R("SHI_RA", "JHA / JSA", 1, "Conforming"),
            R("SHI_RA", "JHA / JSA", 2, "Conforming"), R("SHI_RA", "JHA / JSA", 3, "Conforming"),
            R("SHI_RA", "JHA / JSA", 4, "Conforming"), R("SHI_RA", "JHA / JSA", 5, "Conforming"),
            R("SHI_RA", "JHA / JSA", 6, "Conforming"), R("SHI_RA", "JHA / JSA", 7, "Conforming"),
            R("SHI_RA", "JHA / JSA", 8, "Conforming"),
            R("SHI_RA", "Culture / Attitudes", 0, "Conforming"), R("SHI_RA", "Culture / Attitudes", 1, "Conforming"),
            R("SHI_RA", "Culture / Attitudes", 2, "Conforming"), R("SHI_RA", "Culture / Attitudes", 3, "Conforming"),
            R("SHI_RA", "Culture / Attitudes", 4, "Conforming"), R("SHI_RA", "Culture / Attitudes", 5, "Conforming"),
            R("SHI_RA", "SHI (RA) Specific", 0, "Conforming"), R("SHI_RA", "SHI (RA) Specific", 1, "Conforming"),
            R("SHI_RA", "SHI (RA) Specific", 2, "Conforming"), R("SHI_RA", "SHI (RA) Specific", 3, "Conforming"),
            R("SHI_RA", "SHI (RA) Specific", 4, "Conforming"),
        };
        foreach (var (sec, idx, st, c, os, ca, at) in ov)
            ApplyOv(r, "SHI_RA", sec, idx, st, c, os, ca, at);
        return r;
    }

    private static void ApplyOv(List<ResponseSeed> r, string div, string sec, int idx,
        string st, string? c, bool os, string? ca, string? at)
    {
        var i = r.FindIndex(x => x.DivCode == div && x.SectionName == sec && x.QIdx == idx);
        if (i >= 0) r[i] = new ResponseSeed(div, sec, idx, st, c, os, ca, at, null);
    }

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
