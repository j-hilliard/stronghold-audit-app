# Score Calculation

## Formula

```
Score = Conforming ÷ (Conforming + NonConforming + Warning) × 100
```

Rounded to the nearest whole number and displayed as a percentage.

## What Is Excluded

| Response | Included in denominator? |
|---|---|
| Conforming | Yes |
| Non-Conforming | Yes |
| Warning | Yes |
| N/A | **No** — item does not apply to this job/site |
| Unanswered | **No** — item has not been responded to yet |

N/A and unanswered items are excluded because they do not represent a pass or fail —
including them would artificially lower the score when many items are not applicable.

## Edge Cases

| Scenario | Score displayed |
|---|---|
| All items are N/A | `—` (dash — no applicable items, score undefined) |
| All items are Conforming | `100%` |
| All items are Non-Conforming | `0%` |
| Mix with some unanswered | Score calculated on answered items only |
| No questions in section | Section shows `0/0` progress |

## Live Update

The score recalculates instantly on every status change (no save required).
The `ScoreSummaryBar` component subscribes to the `auditStore.calculateScore()` computed property.

## Section Progress Counter

Each section header shows `answered / total` for that section (e.g., `5/13`).
- Answered = any response that is not blank (Conforming, NonConforming, Warning, or N/A all count)
- Total = all questions in the section regardless of rules

## Implementation Reference

```
// auditStore.ts
const calculateScore = computed(() => {
    const conforming = responses.filter(r => r.status === 'Conforming').length
    const nonConforming = responses.filter(r => r.status === 'NonConforming').length
    const warning = responses.filter(r => r.status === 'Warning').length
    const applicable = conforming + nonConforming + warning
    if (applicable === 0) return null  // renders as '—'
    return Math.round((conforming / applicable) * 100)
})
```
