/**
 * useReportDraft — draft persistence + autosave.
 *
 * This is the ONLY file in the codebase that calls:
 *   JSON.parse(blocksJson) as ReportBlock[]
 *   JSON.stringify(blocks)
 *
 * All other consumers work with ReportBlock[] — never raw JSON strings.
 */
import { ref, computed, watch } from 'vue';
import { useApiStore } from '@/stores/apiStore';
import { AuditClient } from '@/apiclient/auditClient';
import type { ReportDraftDto, CreateReportDraftRequest, UpdateReportDraftRequest } from '@/apiclient/auditClient';
import type { ReportBlock, BlockLayout, ColumnRowBlock } from '../types/report-block';
import { REPORT_THEMES } from './useReportThemes';

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

    // ── Undo/Redo history ─────────────────────────────────────────────────────
    // History stores JSON snapshots so deep clones are cheap and exact.
    // Cap at 30 entries so memory stays bounded.
    const history = ref<string[]>([]);
    const historyIndex = ref(-1);
    let historyDebounceTimer: ReturnType<typeof setTimeout> | null = null;

    const canUndo = computed(() => historyIndex.value > 0);
    const canRedo = computed(() => historyIndex.value < history.value.length - 1);

    /** Snapshot the current blocks array into history BEFORE a mutation. */
    function pushHistory() {
        const snapshot = JSON.stringify(blocks.value);
        // Truncate any redo future when a new action branches off
        history.value = history.value.slice(0, historyIndex.value + 1);
        history.value.push(snapshot);
        if (history.value.length > 30) history.value.shift();
        historyIndex.value = history.value.length - 1;
    }

    /** Debounced push — used for text edits to avoid a snapshot per keystroke. */
    function pushHistoryDebounced() {
        if (historyDebounceTimer) clearTimeout(historyDebounceTimer);
        historyDebounceTimer = setTimeout(() => pushHistory(), 800);
    }

    function undo() {
        if (!canUndo.value) return;
        historyIndex.value--;
        blocks.value = JSON.parse(history.value[historyIndex.value]);
        isDirty.value = true;
    }

    function redo() {
        if (!canRedo.value) return;
        historyIndex.value++;
        blocks.value = JSON.parse(history.value[historyIndex.value]);
        isDirty.value = true;
    }
    // ─────────────────────────────────────────────────────────────────────────

    let autosaveTimer: ReturnType<typeof setTimeout> | null = null;

    // Mark dirty when blocks change (after initial load)
    watch(blocks, () => { isDirty.value = true; }, { deep: true });

    function getClient() {
        return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
    }

    /**
     * Back-fill layout for any block loaded from an old draft that predates
     * the free-form canvas. Assigns a stacked default layout preserving the
     * original top-to-bottom order.
     */
    /** Back-fill layout for a single block. cumulativeY is passed in from the caller. */
    function ensureLayout(block: ReportBlock, index: number, cumulativeY = 0): ReportBlock {
        if (block.layout) return block;
        return {
            ...block,
            layout: { x: 40, y: cumulativeY, width: 714, height: 0, zIndex: index + 1 },
        };
    }

    /** Deserialize blocksJson from the API into typed ReportBlock[]. */
    function deserialize(json: string): ReportBlock[] {
        try {
            const parsed = JSON.parse(json);
            if (!Array.isArray(parsed)) return [];
            const blocks = parsed as ReportBlock[];
            // Compute cumulative Y so blocks stack correctly (not overlapping)
            let cumulativeY = 0;
            return blocks.map((b, i) => {
                const block = ensureLayout(b, i, cumulativeY);
                // Advance Y by this block's estimated height (or its stored height if set)
                const EST: Partial<Record<ReportBlock['type'], number>> = {
                    cover: 240, 'cover-page': 1123, heading: 56, 'kpi-grid': 320, 'chart-bar': 380,
                    'chart-line': 340, narrative: 220, callout: 120, 'ca-table': 220,
                    image: 300, 'toc-sidebar': 620, 'oval-callout': 250,
                    'findings-category': 220, divider: 32, spacer: 48, 'column-row': 400,
                };
                const h = block.layout.height > 0 ? block.layout.height : (EST[b.type] ?? 160);
                cumulativeY = block.layout.y + h + 16;
                return block;
            });
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

        try {
            const blocksJson = serialize(blocks.value);
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
        if (idx >= 0) {
            pushHistoryDebounced();
            blocks.value[idx] = updated;
        }
    }

    /**
     * Update layout fields for a block — kept separate from updateBlock so
     * drag/resize operations don't flood the text-edit debounce history.
     */
    function updateLayout(id: string, layout: Partial<BlockLayout>) {
        const idx = blocks.value.findIndex(b => b.id === id);
        if (idx < 0) return;
        pushHistoryDebounced();
        blocks.value[idx] = {
            ...blocks.value[idx],
            layout: { ...blocks.value[idx].layout, ...layout },
        };
    }

    function bringForward(id: string) {
        const block = blocks.value.find(b => b.id === id);
        if (!block) return;
        const maxZ = Math.max(...blocks.value.map(b => b.layout?.zIndex ?? 1));
        updateLayout(id, { zIndex: Math.min((block.layout?.zIndex ?? 1) + 1, maxZ + 1) });
    }

    function sendBackward(id: string) {
        const block = blocks.value.find(b => b.id === id);
        if (!block) return;
        updateLayout(id, { zIndex: Math.max((block.layout?.zIndex ?? 1) - 1, 1) });
    }

    function bringToFront(id: string) {
        const maxZ = Math.max(...blocks.value.map(b => b.layout?.zIndex ?? 1));
        updateLayout(id, { zIndex: maxZ + 1 });
    }

    function sendToBack(id: string) {
        updateLayout(id, { zIndex: 1 });
    }

    /** Remove a block by id. */
    function removeBlock(id: string) {
        try { pushHistory(); } catch { /* history skipped — mutation still proceeds */ }
        blocks.value = blocks.value.filter(b => b.id !== id);
    }

    /** Replace all blocks (e.g. after regeneration or reorder). Saves a history snapshot. */
    function setBlocks(newBlocks: ReportBlock[]) {
        try { pushHistory(); } catch { /* history skipped — mutation still proceeds */ }
        blocks.value = newBlocks;
    }

    /**
     * Apply a named theme to all blocks in the draft.
     * Sets BlockStyle on every block (including inner column blocks) and
     * sets cover.primaryColor for cover blocks.
     */
    function applyTheme(themeId: string) {
        const theme = REPORT_THEMES.find(t => t.id === themeId);
        if (!theme) return;
        pushHistory();

        const t = theme; // narrowed non-null after the guard above
        function applyToBlock(block: ReportBlock) {
            // Cover: update primaryColor
            if (block.type === 'cover') {
                block.content.primaryColor = t.coverPrimaryColor;
            }
            // Style update (skip cover — its bg comes from primaryColor, not blockStyle)
            if (block.type !== 'cover') {
                block.style = { ...block.style, ...t.blockStyle };
            }
            // Recurse into column children
            if (block.type === 'column-row') {
                const col = block as ColumnRowBlock;
                col.content.leftBlocks.forEach(applyToBlock);
                col.content.rightBlocks.forEach(applyToBlock);
            }
        }

        blocks.value = blocks.value.map(b => {
            const clone = JSON.parse(JSON.stringify(b)) as ReportBlock;
            applyToBlock(clone);
            return clone;
        });
        isDirty.value = true;
    }

    /** Deep-clone a block and insert the copy directly below the original. */
    function duplicateBlock(id: string) {
        const idx = blocks.value.findIndex(b => b.id === id);
        if (idx === -1) return;
        pushHistory();
        const clone = JSON.parse(JSON.stringify(blocks.value[idx])) as ReportBlock;
        clone.id = crypto.randomUUID();
        blocks.value.splice(idx + 1, 0, clone);
        isDirty.value = true;
    }

    return {
        meta,
        blocks,
        isDirty,
        saving,
        saveError,
        lastSavedAt,
        // Undo/Redo
        canUndo,
        canRedo,
        undo,
        redo,
        // CRUD
        loadDraft,
        save,
        scheduleAutosave,
        deleteDraft,
        updateBlock,
        updateLayout,
        removeBlock,
        setBlocks,
        duplicateBlock,
        applyTheme,
        // Z-order
        bringForward,
        sendBackward,
        bringToFront,
        sendToBack,
    };
}
