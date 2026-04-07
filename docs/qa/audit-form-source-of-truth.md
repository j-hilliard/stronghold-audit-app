# Audit Form Source Of Truth

This project is being built from the files in:

`C:\Users\joseph.hilliard\OneDrive - Quanta Services Management Partnership, L.P\Desktop\Audit Form`

## Baseline Files
- `SHC_Compliance_Audit_Tool.html` (primary behavior baseline)
- `Blank Audit Rev12.xlsm` (legacy workflow/reference)
- `Evergreen Job Site audit form.docx` (content/reference)
- `Audit Tool Upgrade - Proposal to Audit Team (002).docx` (scope/reference)

## Non-Negotiable Behavior Baseline (From `SHC_Compliance_Audit_Tool.html`)
- Division-specific checklists (database-backed in app, not hardcoded in Vue).
- Status controls per question: `Conforming`, `Non-Conforming`, `Warning`, `N/A`.
- Non-Conforming reveals comment area and corrected-on-site toggle.
- Corrected-on-site behavior:
  - `Yes` marks corrected state.
  - `Not Corrected` preserves finding for review.
- Live summary counts and score.
- Score formula: `Conforming / (Conforming + Non-Conforming + Warning)`.
- `N/A` excluded from score denominator.
- Required field and unanswered-item validation before submit/export.
- Section collapse/expand (individual + collapse all + expand all).
- Save/reopen behavior and draft recovery behavior.
- PDF export behavior.
- Review/email preparation behavior with division routing.

## QA Enforcement Rule
- No feature is approved unless behavior matches the source baseline above.
- Any deviation must be logged in `docs/qa/defect-log.md` with evidence and explicit disposition (`accepted change` vs `defect`).

## Current Critical Defect
- `DEF-0001` (New Audit division cards rendering/selection mismatch) remains open and blocks approval.
