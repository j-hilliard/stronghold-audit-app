# API Contract

Base URL: `https://localhost:7221/api/v1` (local dev)

Auth: Bearer token (Phase 2+). Phase 1 can use local dev bypass.

Interactive docs: `https://localhost:7221/swagger`

---

## Divisions

### GET /divisions
Returns all active divisions.

**Auth:** Any authenticated user

**Response**
```json
[
  { "id": 1, "code": "TKIE", "name": "TKIE Division", "auditType": "JobSite" },
  { "id": 9, "code": "FACILITY", "name": "Facility Division", "auditType": "Facility" }
]
```

---

## Templates

### GET /templates/active?divisionId={id}
Returns the active published template for a division, with sections and questions.

**Auth:** Auditor+

**Response**
```json
{
  "templateVersionId": 3,
  "versionNumber": 2,
  "divisionCode": "TKIE",
  "sections": [
    {
      "id": 12,
      "name": "Permitting",
      "displayOrder": 1,
      "questions": [
        {
          "id": 45,
          "questionText": "Is the Permit Correct and being followed?",
          "displayOrder": 1,
          "allowNA": true,
          "requireCommentOnNC": true,
          "isScoreable": true
        }
      ]
    }
  ]
}
```

---

## Audits

### POST /audits
Creates a new draft audit and locks to the current active template version.

**Auth:** Auditor+

**Request**
```json
{ "divisionId": 1 }
```

**Response**
```json
{ "auditId": 42 }
```

---

### GET /audits
Returns audits visible to the current user scope.

**Auth:** Auditor+ (scoped), AuditManager+ (broader scope)

**Query params:** `divisionId`, `status`, `fromDate`, `toDate`, `auditor`

---

### GET /audits/{id}
Returns full audit detail with responses.

**Auth:** Auditor (own scope), Manager/Reviewer (authorized scope)

---

### PUT /audits/{id}/responses
Batch upsert responses for a draft audit. Rejected for submitted/finalized audits.

**Auth:** Auditor (own draft scope)

**Request**
```json
{
  "header": {
    "jobNumber": "I25-036",
    "date": "2026-03-31",
    "client": "Client Co",
    "location": "Houston, TX",
    "pm": "J. Smith",
    "auditor": "T. Jones",
    "shift": "DAY"
  },
  "responses": [
    { "questionId": 45, "status": "Conforming", "comment": null, "correctedOnSite": false },
    { "questionId": 46, "status": "NonConforming", "comment": "PPE not worn", "correctedOnSite": true }
  ]
}
```

---

### POST /audits/{id}/submit
Submits audit, validates required fields, and generates findings for non-conforming responses.

**Auth:** Auditor (own scope)

---

### GET /audits/{id}/review
Returns review summary with counts, score, and non-conformance list.

**Auth:** Auditor (own scope), Reviewer/Manager (authorized scope)

---

## Admin - Templates

### GET /admin/templates?divisionId={id}
Returns template versions for a division.

**Auth:** TemplateAdmin or SystemAdmin

---

### POST /admin/templates/{versionId}/clone
Clones the active version into a draft.

**Auth:** TemplateAdmin or SystemAdmin

---

### POST /admin/versions/{draftId}/questions
Adds a question to a draft section.

**Auth:** TemplateAdmin or SystemAdmin

**Request**
```json
{
  "sectionId": 12,
  "questionText": "New question text",
  "displayOrder": 5,
  "allowNA": true,
  "requireCommentOnNC": true,
  "isScoreable": true
}
```

---

### DELETE /admin/versions/{draftId}/questions/{questionId}
Archives a question and removes it from the draft version.

**Auth:** TemplateAdmin or SystemAdmin

---

### PUT /admin/versions/{draftId}/questions/reorder
Persists drag/drop question order in a draft section.

**Auth:** TemplateAdmin or SystemAdmin

---

### PUT /admin/versions/{draftId}/sections/reorder
Persists drag/drop section order in a draft version.

**Auth:** TemplateAdmin or SystemAdmin

---

### PUT /admin/versions/{draftId}/publish
Publishes draft to active and supersedes prior active version.

**Auth:** TemplateAdmin or SystemAdmin

---

### GET /admin/questions/archived
Returns archived questions for audit trail review.

**Auth:** TemplateAdmin or SystemAdmin

---

## Admin - Email Routing

### GET /admin/email-routing?divisionId={id}
Returns email routing rules for a division.

**Auth:** TemplateAdmin or SystemAdmin

---

### PUT /admin/email-routing
Upserts email routing rules for a division.

**Auth:** TemplateAdmin or SystemAdmin

**Request**
```json
{
  "divisionId": 1,
  "emailAddresses": ["reviewer@company.com", "manager@company.com"]
}
```

---

## Reporting and KPI

### GET /reports/kpi
Returns KPI summary cards for caller's permitted scope.

**Auth:** AuditManager, AuditReviewer, ReadOnlyViewer, ExecutiveViewer

**Query params:** `divisionId`, `siteId`, `fromDate`, `toDate`, `auditorId`, `status`

---

### GET /reports/trends
Returns trend series grouped by reporting category.

**Auth:** AuditManager, AuditReviewer, ReadOnlyViewer, ExecutiveViewer

**Query params:** `divisionId`, `siteId`, `fromDate`, `toDate`, `groupBy`

---

### GET /reports/findings
Returns findings table rows for the current filter scope.

**Auth:** AuditManager, AuditReviewer, ReadOnlyViewer, ExecutiveViewer

**Query params:** `divisionId`, `siteId`, `fromDate`, `toDate`, `severity`, `status`, `questionId`

---

### GET /reports/corrective-actions
Returns corrective action aging and overdue data.

**Auth:** AuditManager, AuditReviewer, CorrectiveActionOwner, ReadOnlyViewer, ExecutiveViewer

**Query params:** `divisionId`, `siteId`, `owner`, `dueFrom`, `dueTo`, `status`

---

## Security Rule
- All list and reporting endpoints must enforce role + scope filtering server-side.
- Responses must never include rows outside authorized scope.

---

## Error Responses

All errors use ASP.NET Core Problem Details.

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Audit is not in Draft status and cannot be modified."
}
```

| Status | Meaning |
|---|---|
| 400 | Validation failed or business rule violation |
| 401 | Not authenticated |
| 403 | Authenticated but not authorized |
| 404 | Record not found |
| 409 | Conflict (example: modifying submitted audit) |
| 500 | Unexpected server error |
