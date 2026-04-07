<template>
    <div class="print-form" ref="formEl">
        <!-- Print header -->
        <div class="print-header">
            <div class="print-header-top">
                <h1>{{ template ? `${template.divisionCode} Compliance Audit` : 'Compliance Audit' }}</h1>
                <p class="print-subtitle">{{ template?.divisionName }}</p>
            </div>

            <!-- Header fields grid -->
            <div class="header-grid">
                <div class="header-field"><span class="field-label">Job Number</span><div class="field-line"></div></div>
                <div class="header-field"><span class="field-label">Date</span><div class="field-line"></div></div>
                <div class="header-field"><span class="field-label">Client</span><div class="field-line"></div></div>
                <div class="header-field"><span class="field-label">Location</span><div class="field-line"></div></div>
                <div class="header-field"><span class="field-label">Unit</span><div class="field-line"></div></div>
                <div class="header-field"><span class="field-label">Time</span><div class="field-line"></div></div>
                <div class="header-field"><span class="field-label">Shift</span><div class="field-line"></div></div>
                <div class="header-field"><span class="field-label">Project Manager</span><div class="field-line"></div></div>
                <div class="header-field"><span class="field-label">Auditor</span><div class="field-line"></div></div>
                <div class="header-field full-width"><span class="field-label">Work Description</span><div class="field-line"></div></div>
            </div>
        </div>

        <!-- Loading -->
        <div v-if="loading" class="loading-msg">Loading template…</div>

        <!-- Sections -->
        <div v-else-if="template">
            <div v-for="section in template.sections" :key="section.id" class="section">
                <div class="section-header">
                    <span class="section-name">{{ section.name }}</span>
                    <span class="section-count">{{ section.questions.length }} items</span>
                </div>

                <table class="question-table">
                    <thead>
                        <tr>
                            <th class="col-num">#</th>
                            <th class="col-question">Question</th>
                            <th class="col-status">C</th>
                            <th class="col-status">NC</th>
                            <th class="col-status">W</th>
                            <th class="col-status" v-if="true">N/A</th>
                            <th class="col-comment">Comments / Corrective Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="(q, idx) in section.questions" :key="q.versionQuestionId">
                            <td class="col-num">{{ idx + 1 }}</td>
                            <td class="col-question">{{ q.questionText }}</td>
                            <td class="col-status"><span class="checkbox">☐</span></td>
                            <td class="col-status"><span class="checkbox">☐</span></td>
                            <td class="col-status"><span class="checkbox">☐</span></td>
                            <td class="col-status" v-if="q.allowNA"><span class="checkbox">☐</span></td>
                            <td class="col-status" v-else></td>
                            <td class="col-comment"><div class="comment-line"></div></td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Score summary box -->
            <div class="score-box">
                <table class="score-table">
                    <thead><tr>
                        <th>Conforming</th>
                        <th>Non-Conforming</th>
                        <th>Warning</th>
                        <th>N/A</th>
                        <th>Total Scored</th>
                        <th>Score %</th>
                    </tr></thead>
                    <tbody><tr>
                        <td><div class="score-cell"></div></td>
                        <td><div class="score-cell"></div></td>
                        <td><div class="score-cell"></div></td>
                        <td><div class="score-cell"></div></td>
                        <td><div class="score-cell"></div></td>
                        <td><div class="score-cell"></div></td>
                    </tr></tbody>
                </table>
            </div>

            <!-- Signature block -->
            <div class="signature-block">
                <div class="sig-line"><span class="sig-label">Auditor Signature</span><div class="sig-space"></div></div>
                <div class="sig-line"><span class="sig-label">Date</span><div class="sig-space"></div></div>
            </div>
        </div>

        <div v-else class="loading-msg">Template not found for this division.</div>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient, type TemplateDto } from '@/apiclient/auditClient';

const route = useRoute();
const apiStore = useApiStore();
const loading = ref(true);
const template = ref<TemplateDto | null>(null);

onMounted(async () => {
    const divisionId = Number(route.params.divisionId);
    if (!isNaN(divisionId) && divisionId > 0) {
        try {
            const client = new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
            template.value = await client.getActiveTemplate(divisionId);
        } catch {
            // template stays null
        }
    }
    loading.value = false;
    // Auto-print after DOM renders
    setTimeout(() => window.print(), 400);
});
</script>

<style scoped>
.print-form {
    font-family: Arial, sans-serif;
    font-size: 11px;
    color: #000;
    background: #fff;
    padding: 12px 16px;
    max-width: 1000px;
    margin: 0 auto;
}

.print-header {
    border: 2px solid #1a3a5c;
    border-radius: 4px;
    margin-bottom: 12px;
    overflow: hidden;
}

.print-header-top {
    background: #1a3a5c;
    color: #fff;
    padding: 8px 12px;
}

.print-header-top h1 {
    font-size: 16px;
    font-weight: bold;
    margin: 0;
}

.print-subtitle {
    font-size: 11px;
    margin: 2px 0 0;
    opacity: 0.85;
}

.header-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 8px;
    padding: 8px 12px;
}

.header-field {
    display: flex;
    flex-direction: column;
    gap: 2px;
}

.header-field.full-width {
    grid-column: 1 / -1;
}

.field-label {
    font-size: 9px;
    font-weight: bold;
    text-transform: uppercase;
    color: #555;
    letter-spacing: 0.5px;
}

.field-line {
    border-bottom: 1px solid #999;
    min-height: 16px;
}

.section {
    margin-bottom: 10px;
    break-inside: avoid;
}

.section-header {
    background: #1a3a5c;
    color: #fff;
    padding: 4px 8px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-radius: 2px 2px 0 0;
}

.section-name {
    font-weight: bold;
    font-size: 11px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
}

.section-count {
    font-size: 9px;
    opacity: 0.8;
}

.question-table {
    width: 100%;
    border-collapse: collapse;
    border: 1px solid #ccc;
}

.question-table th {
    background: #e8edf2;
    border: 1px solid #bbb;
    padding: 3px 4px;
    font-size: 9px;
    font-weight: bold;
    text-align: center;
}

.question-table td {
    border: 1px solid #ccc;
    padding: 3px 4px;
    vertical-align: top;
}

.col-num {
    width: 24px;
    text-align: center;
    color: #555;
    font-size: 10px;
}

.col-question {
    width: auto;
    font-size: 10px;
    line-height: 1.4;
}

.col-status {
    width: 28px;
    text-align: center;
}

.checkbox {
    font-size: 13px;
    line-height: 1;
}

.col-comment {
    width: 30%;
}

.comment-line {
    border-bottom: 1px solid #bbb;
    min-height: 28px;
    margin-top: 12px;
}

.score-box {
    margin-top: 10px;
    border: 2px solid #1a3a5c;
    border-radius: 4px;
    overflow: hidden;
}

.score-table {
    width: 100%;
    border-collapse: collapse;
}

.score-table th {
    background: #1a3a5c;
    color: #fff;
    border: 1px solid #1a3a5c;
    padding: 4px;
    text-align: center;
    font-size: 9px;
}

.score-table td {
    border: 1px solid #ccc;
    padding: 2px 4px;
}

.score-cell {
    min-height: 22px;
    border-bottom: 1px solid #bbb;
}

.signature-block {
    margin-top: 12px;
    display: grid;
    grid-template-columns: 2fr 1fr;
    gap: 16px;
}

.sig-line {
    display: flex;
    flex-direction: column;
    gap: 2px;
}

.sig-label {
    font-size: 9px;
    font-weight: bold;
    text-transform: uppercase;
    color: #555;
}

.sig-space {
    border-bottom: 1px solid #999;
    min-height: 22px;
}

.loading-msg {
    padding: 40px;
    text-align: center;
    color: #555;
}

/* Screen: show a print button and dark background */
@media screen {
    body { background: #334 !important; }
    .print-form {
        box-shadow: 0 4px 20px rgba(0,0,0,0.4);
        border-radius: 4px;
        min-height: 100vh;
    }
}

@media print {
    body { margin: 0 !important; background: #fff !important; }
    .print-form { padding: 6px 8px; box-shadow: none; }
    .section { break-inside: avoid; }
}
</style>
