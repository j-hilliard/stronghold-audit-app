<template>
    <div class="print-doc" ref="formEl">
        <div v-if="loading" class="loading-msg">Loading template…</div>

        <template v-else-if="template">
            <table class="audit-table">
                <thead>
                    <!-- Banner row -->
                    <tr class="hdr-row-banner">
                        <td colspan="10" class="banner-cell">
                            <div class="banner-inner">
                                <img v-if="logoUrl" :src="logoUrl" alt="" class="banner-logo" />
                                <span v-else class="banner-division-code">{{ template.divisionCode }}</span>
                                <span class="banner-title">{{ template.divisionCode }} Compliance Audit</span>
                            </div>
                        </td>
                    </tr>
                    <!-- Row 1: Job # | Time | Client | Location -->
                    <tr class="hdr-row">
                        <td class="hdr-label">Job #</td>
                        <td class="hdr-value hdr-line" colspan="2"></td>
                        <td class="hdr-label">Time</td>
                        <td class="hdr-value hdr-line"></td>
                        <td class="hdr-label">Client</td>
                        <td class="hdr-value hdr-line" colspan="2"></td>
                        <td class="hdr-label">Location</td>
                        <td class="hdr-value hdr-line"></td>
                    </tr>
                    <!-- Row 2: Date | Unit | Shift -->
                    <tr class="hdr-row">
                        <td class="hdr-label">Date</td>
                        <td class="hdr-value hdr-line" colspan="2"></td>
                        <td class="hdr-label">Unit</td>
                        <td class="hdr-value hdr-line" colspan="3"></td>
                        <td class="hdr-label">Shift</td>
                        <td class="hdr-value hdr-line" colspan="2"></td>
                    </tr>
                    <!-- Row 3: Work Description | PM | Auditor -->
                    <tr class="hdr-row">
                        <td class="hdr-label" colspan="2">Work Description</td>
                        <td class="hdr-value hdr-line" colspan="3"></td>
                        <td class="hdr-label" colspan="2">Project Manager</td>
                        <td class="hdr-value hdr-line"></td>
                        <td class="hdr-label">Auditor</td>
                        <td class="hdr-value hdr-line"></td>
                    </tr>
                </thead>

                <tbody>
                    <!-- Score summary row -->
                    <tr class="score-row">
                        <td colspan="10" class="score-td">
                            <div class="score-block">
                                <div class="score-circle">
                                    <span class="score-pct"></span>
                                    <span class="score-label">Score</span>
                                </div>
                                <div class="score-stats">
                                    <div class="stat-item stat-conforming">
                                        <span class="stat-count">&nbsp;</span>
                                        <span class="stat-name">Conforming</span>
                                    </div>
                                    <div class="stat-item stat-nc">
                                        <span class="stat-count">&nbsp;</span>
                                        <span class="stat-name">Non-Conforming</span>
                                    </div>
                                    <div class="stat-item stat-warning">
                                        <span class="stat-count">&nbsp;</span>
                                        <span class="stat-name">Warning</span>
                                    </div>
                                    <div class="stat-item stat-na">
                                        <span class="stat-count">&nbsp;</span>
                                        <span class="stat-name">N/A</span>
                                    </div>
                                    <div class="stat-item stat-total">
                                        <span class="stat-count">&nbsp;</span>
                                        <span class="stat-name">Total Scored</span>
                                    </div>
                                </div>
                            </div>
                        </td>
                    </tr>

                    <!-- Sections -->
                    <template v-for="section in template.sections" :key="section.id">
                        <!-- Section header = column label row -->
                        <tr class="section-header-row">
                            <td class="sec-col-num">#</td>
                            <td colspan="4" class="section-name-cell">{{ section.name }}</td>
                            <td class="sec-col-cb">C</td>
                            <td class="sec-col-cb">NC</td>
                            <td class="sec-col-cb">W</td>
                            <td class="sec-col-cb">N/A</td>
                            <td class="sec-col-comment">Comments / Corrective Action</td>
                        </tr>
                        <!-- Question rows -->
                        <tr
                            v-for="(q, idx) in section.questions"
                            :key="q.versionQuestionId"
                            class="question-row"
                            :class="{ 'row-alt': idx % 2 === 1 }"
                        >
                            <td class="q-num">{{ idx + 1 }}</td>
                            <td colspan="4" class="q-text">{{ q.questionText }}</td>
                            <td class="q-cb"><span class="checkbox">☐</span></td>
                            <td class="q-cb"><span class="checkbox">☐</span></td>
                            <td class="q-cb"><span class="checkbox">☐</span></td>
                            <td class="q-cb">
                                <span v-if="q.allowNA" class="checkbox">☐</span>
                            </td>
                            <td class="q-comment"></td>
                        </tr>
                    </template>

                    <!-- Signature row -->
                    <tr class="sig-row">
                        <td colspan="6" class="sig-cell">
                            <span class="sig-label">Auditor Signature</span>
                            <div class="sig-line"></div>
                        </td>
                        <td colspan="4" class="sig-cell">
                            <span class="sig-label">Date</span>
                            <div class="sig-line"></div>
                        </td>
                    </tr>
                </tbody>
            </table>
        </template>

        <div v-else class="loading-msg">No active template found for this division.</div>
    </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type TemplateDto } from '@/apiclient/auditClient';

const route = useRoute();
const apiStore = useApiStore();
const formEl = ref<HTMLElement | null>(null);
const loading = ref(true);
const template = ref<TemplateDto | null>(null);

const LOGO_MAP: Record<string, string> = {
    ETS:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/ETS-Logo-color-full.png',
    STS:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/STS-Logo-Full-Clr@2x-2.png',
    STG:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/STG-Logo-Full-Clr@2x-3.png',
    SHI:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/SHI-Logo-Full-color.png',
    TKIE: 'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/TKIE-Logo-Full-Clr@2x-2.png',
    CSL:  'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/CSL-Logo-Full-Clr@2x-3.png',
    CC:   'https://www.thestrongholdcompanies.com/wp-content/uploads/2021/08/CC-Logo-Full-Clr@2x-1.png',
};

const logoUrl = computed(() => {
    const code = template.value?.divisionCode?.toUpperCase() ?? '';
    return LOGO_MAP[code] ?? null;
});

onMounted(async () => {
    const stored = sessionStorage.getItem('print-blank-form-data');
    if (stored) {
        sessionStorage.removeItem('print-blank-form-data');
        try {
            template.value = JSON.parse(stored) as TemplateDto;
        } catch { /* fall through */ }
    }

    if (!template.value) {
        const divisionId = Number(route.params.divisionId);
        if (!isNaN(divisionId) && divisionId > 0) {
            try {
                const client = new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
                template.value = await client.getActiveTemplate(divisionId);
            } catch { /* template stays null */ }
        }
    }

    loading.value = false;
    await nextTick();

    // Wait for logos to load before printing
    const imgs = Array.from(document.querySelectorAll<HTMLImageElement>('img'));
    await Promise.all(
        imgs.map(img =>
            img.complete
                ? Promise.resolve()
                : new Promise<void>(resolve => { img.onload = () => resolve(); img.onerror = () => resolve(); })
        )
    );

    const formNode = document.querySelector('.print-doc') as HTMLElement | null;
    if (formNode) {
        const printRoot = document.createElement('div');
        printRoot.id = 'print-root';
        printRoot.appendChild(formNode);
        document.body.appendChild(printRoot);
    }
    window.print();
});
</script>

<style scoped>
* {
    font-family: Arial, Helvetica, sans-serif;
    font-size: 9pt;
    box-sizing: border-box;
}

.print-doc {
    background: #fff;
    color: #000;
    padding: 0;
    margin: 0;
}

.loading-msg {
    padding: 20px;
    font-size: 12pt;
    color: #666;
}

/* ── Main table ───────────────────────────────────────────────────────────── */
.audit-table {
    width: 100%;
    border-collapse: collapse;
    table-layout: auto;
}

.audit-table td {
    border: 1px solid #000;
    padding: 3px 5px;
    vertical-align: middle;
    overflow-wrap: break-word;
    word-break: break-word;
}

thead {
    display: table-header-group;
}

/* ── Banner ───────────────────────────────────────────────────────────────── */
.banner-cell {
    background: #1a3a5c;
    padding: 6px 10px;
    border: none;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.banner-inner {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
}

.banner-logo {
    max-height: 44px;
    max-width: 180px;
    object-fit: contain;
    filter: brightness(0) invert(1);
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.banner-division-code {
    font-size: 18pt;
    font-weight: bold;
    color: #fff;
}

.banner-title {
    font-size: 13pt;
    font-weight: bold;
    color: #fff;
    text-align: right;
    opacity: 0.92;
}

/* ── Header field rows ────────────────────────────────────────────────────── */
.hdr-label {
    background: #f0f0f0;
    font-weight: bold;
    font-size: 8pt;
    white-space: nowrap;
    width: 8%;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.hdr-value { font-weight: normal; }

.hdr-line { min-height: 20px; }

/* ── Score summary row ────────────────────────────────────────────────────── */
.score-td {
    background: #f8fafc;
    padding: 8px 10px;
    border-bottom: 2px solid #1a3a5c;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.score-block {
    display: flex;
    align-items: center;
    gap: 16px;
}

.score-circle {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    width: 60px;
    height: 60px;
    border-radius: 50%;
    border: 3px solid #94a3b8;
    color: #64748b;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.score-pct   { font-size: 14pt; font-weight: bold; line-height: 1; min-height: 18px; }
.score-label { font-size: 7pt; }

.score-stats { display: flex; gap: 12px; }

.stat-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    min-width: 56px;
    padding: 4px 8px;
    border-radius: 4px;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.stat-conforming { background: #dcfce7; color: #166534; }
.stat-nc         { background: #fee2e2; color: #991b1b; }
.stat-warning    { background: #fef9c3; color: #854d0e; }
.stat-na         { background: #f3f4f6; color: #374151; }
.stat-total      { background: #e0f2fe; color: #0369a1; }

.stat-count { font-size: 16pt; font-weight: bold; line-height: 1; min-height: 20px; }
.stat-name  { font-size: 7pt; text-align: center; }

/* ── Section header row ───────────────────────────────────────────────────── */
.section-header-row td {
    background: #1a3a5c;
    color: #fff;
    font-weight: bold;
    padding: 4px 5px;
    break-after: avoid;
    print-color-adjust: exact;
    -webkit-print-color-adjust: exact;
}

.sec-col-num     { font-size: 8pt; text-align: center; width: 3%; }
.section-name-cell { font-size: 9pt; letter-spacing: 0.3px; }
.sec-col-cb      { font-size: 8pt; text-align: center; width: 5%; }
.sec-col-comment { font-size: 8pt; width: 28%; }

/* ── Question rows ────────────────────────────────────────────────────────── */
.question-row { break-inside: avoid; }

.question-row.row-alt td { background: #f9fafb; }

.q-num {
    text-align: center;
    font-weight: bold;
    color: #6b7280;
    font-size: 8pt;
    width: 3%;
}

.q-text { font-size: 9pt; line-height: 1.4; }

.q-cb {
    text-align: center;
    width: 5%;
}

.checkbox { font-size: 13pt; line-height: 1; }

.q-comment { width: 28%; min-height: 28px; }

/* ── Signature row ────────────────────────────────────────────────────────── */
.sig-row { break-inside: avoid; }

.sig-cell { padding: 8px 10px 12px; vertical-align: bottom; }

.sig-label {
    font-size: 8pt;
    font-weight: bold;
    text-transform: uppercase;
    color: #555;
    display: block;
    margin-bottom: 16px;
}

.sig-line {
    border-bottom: 1px solid #555;
    min-height: 2px;
}

/* ── Print overrides ──────────────────────────────────────────────────────── */
@media print {
    * {
        print-color-adjust: exact !important;
        -webkit-print-color-adjust: exact !important;
    }
    .print-doc { width: 100%; }
    .audit-table { page-break-inside: auto; }
    .question-row { page-break-inside: avoid; break-inside: avoid; }
    .section-header-row { page-break-after: avoid; break-after: avoid; }
    .score-row { page-break-inside: avoid; break-inside: avoid; }
    .sig-row { page-break-inside: avoid; break-inside: avoid; }
}

@media screen {
    .print-doc {
        max-width: 1100px;
        margin: 0 auto;
        padding: 20px;
        background: #fff;
        box-shadow: 0 0 20px rgba(0,0,0,0.15);
    }
}
</style>
