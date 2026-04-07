/**
 * useReportDraft — draft persistence + autosave.
 *
 * This is the ONLY file in the codebase that calls:
 *   JSON.parse(blocksJson) as ReportBlock[]
 *   JSON.stringify(blocks)
 *
 * All other consumers work with ReportBlock[] — never raw JSON strings.
 */
import { ref, watch } from 'vue';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient } from '@/apiclient/auditClient';
import type { ReportDraftDto, CreateReportDraftRequest, UpdateReportDraftRequest } from '@/apiclient/auditClient';
import type { ReportBlock } from '../types/report-block';

export interface DraftMeta {
    id: number | null;
    divisionId: number;
    divisionCode: string;
    title: string;
    period: string;
    dateFrom: string | null;
    dateTo: string | null;
    rowVersion: string | null;
    createdBy: string;
}

export function useReportDraft() {
    const apiStore = useApiStore();
    const meta = ref<DraftMeta>({
        id: null,
        divisionId: 0,
        divisionCode: '',
        title: '',
        period: '',
        dateFrom: null,
        dateTo: null,
        rowVersion: null,
        createdBy: '',
    });
    const blocks = ref<ReportBlock[]>([]);
    const isDirty = ref(false);
    const saving = ref(false);
    const saveError = ref<string | null>(null);
    const lastSavedAt = ref<Date | null>(null);

    let autosaveTimer: ReturnType<typeof setTimeout> | null = null;

    // Mark dirty when blocks change (after initial load)
    watch(blocks, () => { isDirty.value = true; }, { deep: true });

    function getClient() {
        return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
    }

    /** Deserialize blocksJson from the API into typed ReportBlock[]. */
    function deserialize(json: string): ReportBlock[] {
        try {
            const parsed = JSON.parse(json);
            return Array.isArray(parsed) ? (parsed as ReportBlock[]) : [];
        } catch {
            return [];
        }
    }

    /** Serialize ReportBlock[] back to JSON for the API. */
    function serialize(blockList: ReportBlock[]): string {
        return JSON.stringify(blockList);
    }

    /** Load an existing draft by id. Populates meta + blocks. */
    async function loadDraft(id: number): Promise<void> {
        const client = getClient();
        const dto: ReportDraftDto = await client.getReportDraft(id);
        meta.value = {
            id: dto.id,
            divisionId: dto.divisionId,
            divisionCode: dto.divisionCode,
            title: dto.title,
            period: dto.period,
            dateFrom: dto.dateFrom ?? null,
            dateTo: dto.dateTo ?? null,
            rowVersion: dto.rowVersion,
            createdBy: dto.createdBy,
        };
        blocks.value = deserialize(dto.blocksJson);
        isDirty.value = false;
        saveError.value = null;
    }

    /** Save current state. Creates if id is null, updates otherwise. */
    async function save(): Promise<void> {
        if (saving.value) return;
        saving.value = true;
        saveError.value = null;

        const client = getClient();
        const blocksJson = serialize(blocks.value);

        try {
            if (meta.value.id === null) {
                // Create
                const req: CreateReportDraftRequest = {
                    divisionId: meta.value.divisionId,
                    title: meta.value.title || 'Untitled Draft',
                    period: meta.value.period,
                    dateFrom: meta.value.dateFrom,
                    dateTo: meta.value.dateTo,
                    blocksJson,
                };
                const newId = await client.createReportDraft(req);
                // Reload to get rowVersion and server-assigned id
                await loadDraft(newId);
            } else {
                // Update — must include rowVersion for optimistic concurrency
                if (!meta.value.rowVersion) throw new Error('rowVersion missing — cannot update draft.');
                const req: UpdateReportDraftRequest = {
                    title: meta.value.title || 'Untitled Draft',
                    period: meta.value.period,
                    dateFrom: meta.value.dateFrom,
                    dateTo: meta.value.dateTo,
                    blocksJson,
                    rowVersion: meta.value.rowVersion,
                };
                await client.updateReportDraft(meta.value.id, req);
                // Reload to get updated rowVersion
                await loadDraft(meta.value.id);
            }

            isDirty.value = false;
            lastSavedAt.value = new Date();
        } catch (e: unknown) {
            if (e && typeof e === 'object' && 'response' in e) {
                const resp = (e as { response: { status: number } }).response;
                if (resp?.status === 409) {
                    saveError.value = 'This draft was modified in another session. Please reload to get the latest version.';
                    return;
                }
            }
            saveError.value = e instanceof Error ? e.message : 'Save failed.';
        } finally {
            saving.value = false;
        }
    }

    /** Schedule autosave 3 seconds after the last change. */
    function scheduleAutosave() {
        if (autosaveTimer) clearTimeout(autosaveTimer);
        autosaveTimer = setTimeout(() => {
            if (isDirty.value) save();
        }, 3000);
    }

    /** Delete the current draft from the server. */
    async function deleteDraft(): Promise<void> {
        if (meta.value.id === null) return;
        const client = getClient();
        await client.deleteReportDraft(meta.value.id);
        meta.value.id = null;
        meta.value.rowVersion = null;
        blocks.value = [];
        isDirty.value = false;
    }

    /** Update a single block by id without triggering a full reload. */
    function updateBlock(updated: ReportBlock) {
        const idx = blocks.value.findIndex(b => b.id === updated.id);
        if (idx >= 0) blocks.value[idx] = updated;
    }

    /** Remove a block by id. */
    function removeBlock(id: string) {
        blocks.value = blocks.value.filter(b => b.id !== id);
    }

    /** Replace all blocks (e.g. after regeneration). Preserves dirty flag. */
    function setBlocks(newBlocks: ReportBlock[]) {
        blocks.value = newBlocks;
    }

    return {
        meta,
        blocks,
        isDirty,
        saving,
        saveError,
        lastSavedAt,
        loadDraft,
        save,
        scheduleAutosave,
        deleteDraft,
        updateBlock,
        removeBlock,
        setBlocks,
    };
}
