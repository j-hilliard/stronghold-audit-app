# API Contract

Base URL: `https://localhost:7221/api/v1` (local dev)

Auth: Bearer token (Phase 2+). Phase 1: auth bypass via `LocalDevAuthHandler`.

Full interactive docs available at: `https://localhost:7221/swagger`

---

## Divisions

### GET /divisions
Returns all active divisions.

**Auth:** Any authenticated user  
**Response:**
```json
[
  { "id": 1, "code": "TKIE", "name": "TKIE Division", "auditType": "JobSite" },
  { "id": 9, "code": "FACILITY", "name": "Facility Division", "auditType": "Facility" }
]
```

---

## Templates

### GET /templates/active?divisionId={id}
Returns the active published template for a division, with all sections and questions.

**Auth:** Auditor+  
**Response:**
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
          "questionText": "Is the Permit Correct & being followed?",
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
Creates a new draft audit. Locks to the current active template version.

**Auth:** Auditor+  
**Request:**
```json
{ "divisionId": 1 }
```
**Response:** `{ "auditId": 42 }`

---

### GET /audits
Returns audit list for the current user's accessible divisions.

**Auth:** Auditor+ (scoped to own division/audits); AuditManager+ (all)  
**Query params:** `divisionId`, `status`, `fromDate`, `toDate`, `auditor`

---

### GET /audits/{id}
Returns a full audit with all responses.

**Auth:** Auditor (own audits only); DivisionManager+ (all in division)

---

### PUT /audits/{id}/responses
Saves all responses for a draft audit (batch upsert). Rejected if audit is Submitted.

**Auth:** Auditor (own drafts only)  
**Request:**
```json
{
  "header": { "jobNumber": "I25-036", "date": "2026-03-31", "client": "Client Co", "location": "Houston, TX", "pm": "J. Smith", "auditor": "T. Jones", "shift": "DAY" },
  "responses": [
    { "questionId": 45, "status": "Conforming", "comment": null, "correctedOnSite": false },
    { "questionId": 46, "status": "NonConforming", "comment": "PPE not worn", "correctedOnSite": true }
  ]
}
```

---

### POST /audits/{id}/submit
Submits the audit. Validates all required header fields. Generates AuditFindings from NonConforming responses.

**Auth:** Auditor (own audits only)

---

### GET /audits/{id}/review
Returns the review summary: header, score, conforming/NC/warning/NA counts, NC item list.

**Auth:** Auditor (own); DivisionManager+

---

## Admin — Templates

### GET /admin/templates?divisionId={id}
Returns all template versions for a division.

**Auth:** SystemAdmin

---

### POST /admin/templates/{versionId}/clone
Clones the active version into a new Draft. Only one Draft per template at a time.

**Auth:** SystemAdmin

---

### POST /admin/versions/{draftId}/questions
Adds a question to a draft version section.

**Auth:** SystemAdmin  
**Request:**
```json
{ "sectionId": 12, "questionText": "New question text", "displayOrder": 5, "allowNA": true, "requireCommentOnNC": true, "isScoreable": true }
```

---

### DELETE /admin/versions/{draftId}/questions/{questionId}
Archives the question and removes it from the draft version. **Does not delete the question record.**

**Auth:** SystemAdmin

---

### PUT /admin/versions/{draftId}/questions/reorder
Updates display order for questions in a section.

**Auth:** SystemAdmin

---

### PUT /admin/versions/{draftId}/publish
Publishes the draft → Active. Previous Active → Superseded.

**Auth:** SystemAdmin

---

### GET /admin/questions/archived
Returns all archived questions for audit trail purposes.

**Auth:** SystemAdmin

---

## Admin — Email Routing

### GET /admin/email-routing?divisionId={id}
Returns email routing rules for a division.

**Auth:** SystemAdmin

---

### PUT /admin/email-routing
Upserts email routing rules for a division.

**Auth:** SystemAdmin  
**Request:**
```json
{
  "divisionId": 1,
  "emailAddresses": ["reviewer@company.com", "manager@company.com"]
}
```

---

## Error Responses

All errors follow the standard ASP.NET Core problem details format:

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
| 403 | Authenticated but not authorized for this action |
| 404 | Record not found |
| 409 | Conflict (e.g., attempt to modify a Submitted audit) |
| 500 | Unexpected server error (logged to ProcessLog) |
