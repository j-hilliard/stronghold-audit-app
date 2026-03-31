# ADR-001: SQL Server / Azure SQL as Primary Database (not Cosmos DB)

**Status:** Accepted  
**Date:** 2026-03-31  

## Decision

Use **SQL Server / Azure SQL** as the system of record for all audit data.
Cosmos DB is not used unless a specific workload benefit is identified and approved via a new ADR.

## Context

Two options were evaluated: Azure SQL (relational) and Cosmos DB (NoSQL document store).

The audit system requires:
- Templates with versioned questions linked to audits
- Responses linked to specific audits and questions
- Findings linked to responses
- Corrective actions linked to findings
- Reporting that queries across audits by division, date, auditor, and question
- Conformance trend analysis over time

## Reasoning

| Factor | Azure SQL | Cosmos DB |
|---|---|---|
| Relational data (templates → versions → sections → questions) | Natural — foreign keys, joins | Requires denormalization or complex partition strategies |
| Reporting (cross-audit queries) | Standard SQL queries | Expensive scans unless heavily denormalized |
| Version locking (audit keeps its template snapshot) | JOIN on TemplateVersionId | Manageable but no native referential integrity |
| Familiar tooling (.NET / EF Core) | First-class support | Supported but added complexity |
| Horizontal scale need | Not required at this scale | Cosmos DB's strength — not needed here |
| Cost | Predictable DTUs | Per-request billing can be unpredictable |

## Consequences

- All audit data lives in SQL Server locally and Azure SQL in production
- EF Core handles migrations; all schema changes go through the migration governance process
- If a future use case (e.g., real-time audit collaboration, IoT sensor data) benefits from Cosmos DB, a new ADR is opened at that time
