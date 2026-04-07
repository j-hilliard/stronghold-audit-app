# DEF-0008 Handoff: Incident Employee Save Returns 409

## Summary
- Area: Incident Management (`/incident-management/incidents/:id`)
- Severity: High
- Symptom: `Save Changes` fails when adding a new employee to an existing incident.
- API result: `PUT /v1/IncidentReport/{id}` returns `409 Conflict`.

## Reproduction (Validated)
1. Open an incident that already has at least one employee.
2. Click `Add Employee`, fill `ID` + `Full name`.
3. Click `Save Changes`.
4. Observe save failure.
5. Re-open incident: newly added employee is missing.

## Evidence
- API runtime log: `api-5045-local.out.log`
- Endpoint response: `PUT http://localhost:5045/v1/IncidentReport/{id} -> 409`
- DB verification:
  - `SELECT COUNT(*) FROM safety.incident_employee_involved WHERE incident_report_id = '{id}'`
  - Count does not increase after failed save.

## Root Cause
`UpdateIncidentHandler` replaces child collections, but new `IncidentEmployeeInvolved` entries are being persisted as `UPDATE` statements instead of `INSERT`.  
EF then expects one row affected for a new ID that does not yet exist, causing `DbUpdateConcurrencyException`.

Location:
- `Api/Domain/IncidentReports/UpdateIncident.cs`

Observed SQL pattern in log:
- `UPDATE [safety].[incident_employee_involved] ... WHERE [id] = @newEmployeeId`

## Recommended Fix
In `UpdateIncidentHandler`, when rebuilding `EmployeesInvolved` and `Actions`:
1. Treat new child rows as `Added` explicitly.
2. Do not re-use incoming IDs for replacement rows.
3. Keep delete-and-reinsert deterministic.

Suggested approach:
- Always assign new IDs for replacement rows in update flow, or
- Attach rows with explicit `EntityState.Added` for new entries and `Modified` only for truly existing rows.

## Regression Test Added (QA)
- File: `webapp/tests/e2e/incident-employee-persistence-api.spec.ts`
- Test: `incident update persists newly added employee`
- Gate condition:
  - Incident update must return `200` or `202` (not `409`).
  - Follow-up `GET` must contain newly added employee marker.
