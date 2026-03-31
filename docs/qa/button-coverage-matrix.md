# Button Coverage Matrix

## Scope
- Strict gate scope: Shared shell + current incident management flows.
- Future strict scope: Audit module + shared shell (as audit pages are introduced).

| Coverage ID | Scope | Route | UI Element | Expected Behavior | Test Case |
|---|---|---|---|---|---|
| shared-shell-menu-button | Shared shell | `/incident-management/incidents` | Menu button | Clickable; shell nav remains available | `button-contract.spec.ts` |
| shared-shell-profile-menu-button | Shared shell | `/incident-management/incidents` | Profile button | Opens profile menu with logout action | `button-contract.spec.ts` |
| shared-shell-logo-home-link | Shared shell | `/incident-management/incidents` | Header logo | Navigates to dashboard | `button-contract.spec.ts` |
| incident-list-new-incident-button | Incident mgmt | `/incident-management/incidents` | New Incident | Navigates to new incident form | `button-contract.spec.ts` |
| incident-list-search-button | Incident mgmt | `/incident-management/incidents` | Search icon button | Executes list filtering | `button-contract.spec.ts` |
| incident-form-cancel-button | Incident mgmt | `/incident-management/incidents/new` | Cancel | Returns to list without submit | `button-contract.spec.ts` |
| incident-form-save-draft-button | Incident mgmt | `/incident-management/incidents/new` | Save Draft | Persists draft and enters edit mode | `button-contract.spec.ts` |
| incident-form-submit-button | Incident mgmt | `/incident-management/incidents/new` | Submit | Saves and returns to list | `button-contract.spec.ts` |
| reference-table-add-company-button | Incident mgmt | `/incident-management/ref-tables` | Add Company | Opens create dialog | `button-contract.spec.ts` |
| reference-table-regions-tab | Incident mgmt | `/incident-management/ref-tables` | Regions tab | Switches tab content | `button-contract.spec.ts` |
| reference-table-lookup-tab | Incident mgmt | `/incident-management/ref-tables` | Lookup Tables tab | Switches tab content | `button-contract.spec.ts` |

## Update Rule
- Every newly introduced button in strict scope must be added here before merge.
