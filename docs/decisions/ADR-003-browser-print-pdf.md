# ADR-003: Browser Print for PDF Export (not Server-Side PDF Library)

**Status:** Accepted  
**Date:** 2026-03-31  

## Decision

PDF export uses the browser's native print-to-PDF via `window.print()` with `@media print` CSS,
not a server-side PDF library (e.g., QuestPDF, PuppeteerSharp, iTextSharp).

## Context

The audit form needs to be exportable as a PDF for record-keeping and submission.

## Reasoning

- The prototype already uses browser print successfully — behavior is proven
- No server-side dependency means no extra NuGet packages, no render pipeline, no file storage
- The audit form already renders correctly in the browser; print CSS just hides the toolbar and action buttons
- Browser PDF output matches exactly what the auditor sees on screen — no rendering discrepancies
- Works entirely offline if the network is unavailable
- Filename suggestion is shown as a tooltip (same as prototype); the auditor sets the filename in the browser's Save As dialog

## Tradeoffs Accepted

- The auditor must use the browser's print dialog and select "Save as PDF" — not a single-click download
- PDF styling is limited to what CSS can express in print mode
- No server-side audit trail of the exact PDF file generated (the ProcessLog records that a PDF export was initiated, but not the file itself)

## When to Revisit

If the requirement changes to: automated PDF archival, bulk PDF generation, or PDF attachments
sent via email (not just the mailto: workflow), open a new ADR to evaluate server-side options.
