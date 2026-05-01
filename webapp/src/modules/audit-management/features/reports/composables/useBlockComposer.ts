import { ref } from 'vue';
import type { ReportBlock, BlockType, BlockStyle, BlockLayout } from '../types/report-block';

// ── Block metadata for the Add-block UI ──────────────────────────────────────

export interface BlockMeta {
    type: BlockType;
    label: string;
    icon: string;
    group: 'Text' | 'Data' | 'Media' | 'Layout';
}

export const ADDABLE_BLOCKS: BlockMeta[] = [
    { type: 'heading',    label: 'Heading',      icon: 'pi pi-heading',          group: 'Text'   },
    { type: 'narrative',  label: 'Rich Text',    icon: 'pi pi-align-left',       group: 'Text'   },
    { type: 'callout',    label: 'Callout',      icon: 'pi pi-info-circle',      group: 'Text'   },
    { type: 'kpi-grid',   label: 'KPI Grid',     icon: 'pi pi-chart-bar',        group: 'Data'   },
    { type: 'chart-bar',  label: 'Bar Chart',    icon: 'pi pi-chart-bar',        group: 'Data'   },
    { type: 'chart-line', label: 'Line Chart',   icon: 'pi pi-chart-line',       group: 'Data'   },
    { type: 'ca-table',   label: 'CA Table',     icon: 'pi pi-table',            group: 'Data'   },
    { type: 'image',      label: 'Image',        icon: 'pi pi-image',            group: 'Media'  },
    { type: 'column-row', label: 'Two Columns',  icon: 'pi pi-objects-column',   group: 'Layout' },
    { type: 'cover',      label: 'Cover',        icon: 'pi pi-file',             group: 'Layout' },
    { type: 'divider',    label: 'Divider',      icon: 'pi pi-minus',            group: 'Layout' },
    { type: 'spacer',     label: 'Spacer',       icon: 'pi pi-arrows-v',         group: 'Layout' },
];

// ── Default layout (zero position — FlowCanvas ignores x/y/zIndex) ───────────

const DEFAULT_LAYOUT: BlockLayout = { x: 0, y: 0, width: 714, height: 0, zIndex: 1 };
const DEFAULT_STYLE: BlockStyle = {};

// ── Default-content factories ─────────────────────────────────────────────────

function makeBlock(type: BlockType): ReportBlock {
    const base = {
        id:       crypto.randomUUID(),
        isEdited: false,
        style:    { ...DEFAULT_STYLE },
        layout:   { ...DEFAULT_LAYOUT },
    };

    switch (type) {
        case 'heading':
            return { ...base, type, content: { text: 'New Section', level: 2 } };
        case 'kpi-grid':
            return { ...base, type, content: { cards: [], showComparison: false } };
        case 'narrative':
            return { ...base, type, content: { text: '' } };
        case 'callout':
            return { ...base, type, content: { title: 'Note', body: '', variant: 'info' } };
        case 'chart-bar':
            return { ...base, type, content: { title: 'Category Breakdown', labels: [], datasets: [], caption: '' } };
        case 'chart-line':
            return { ...base, type, content: { sectionName: '', title: 'Trend', labels: [], datasets: [], caption: '', examples: '' } };
        case 'image':
            return { ...base, type, content: { url: '', alt: '', caption: '', width: 'full' } };
        case 'column-row':
            return { ...base, type, content: { ratio: '50/50', gap: 'md', leftBlocks: [], rightBlocks: [] } };
        case 'divider':
            return { ...base, type, content: { thickness: 1, variant: 'solid', color: '#475569', marginY: 'md' } };
        case 'spacer':
            return { ...base, type, content: { height: 'md' } };
        case 'ca-table':
            return { ...base, type, content: { rows: [] } };
        case 'cover':
            return { ...base, type, content: {
                divisionName: 'Division Name', divisionCode: '',
                period: '', preparedBy: '',
                primaryColor: '#1e3a5f', backgroundImageUrl: '',
                coverHeight: 'sm',
            } };
        case 'toc-sidebar':
            return { ...base, type, content: { title: 'INSIDE', items: [], darkBackground: true } };
        case 'oval-callout':
            return { ...base, type, content: { title: 'strong-hold', body: '', backgroundColor: '#1e3a5f', textColor: '#ffffff' } };
        case 'findings-category':
            return { ...base, type, content: { sectionName: '', findings: '', showExamplesLabel: true } };
        case 'cover-page':
            return { ...base, type, content: {
                schemaVersion: 1, templateId: 'stronghold-dark',
                divisionName: '', year: String(new Date().getFullYear()),
                showStats: false, showCallout: false, showMap: false,
                showLocations: false, showAward: false, showHighlights: false,
                stats: [], highlights: [],
            } };
        default:
            return { ...base, type: 'narrative', content: { text: '' } } as ReportBlock;
    }
}

// ── Composable ────────────────────────────────────────────────────────────────

export function useBlockComposer() {
    const blocks   = ref<ReportBlock[]>([]);
    const editMode = ref(true);

    function addBlock(type: BlockType, afterId?: string) {
        const newBlock = makeBlock(type);
        if (afterId) {
            const idx = blocks.value.findIndex(b => b.id === afterId);
            if (idx >= 0) { blocks.value.splice(idx + 1, 0, newBlock); return; }
        }
        blocks.value.push(newBlock);
    }

    function removeBlock(id: string) {
        blocks.value = blocks.value.filter(b => b.id !== id);
    }

    function reorderBlocks(newList: ReportBlock[]) {
        blocks.value = newList;
    }

    function updateBlockConfig(id: string, content: unknown) {
        const block = blocks.value.find(b => b.id === id);
        if (block) (block as Record<string, unknown>).content = content;
    }

    function updateBlockStyle(id: string, style: Partial<BlockStyle>) {
        const block = blocks.value.find(b => b.id === id);
        if (block) block.style = { ...block.style, ...style };
    }

    function duplicateBlock(id: string) {
        const idx = blocks.value.findIndex(b => b.id === id);
        if (idx < 0) return;
        const clone: ReportBlock = JSON.parse(JSON.stringify(blocks.value[idx]));
        clone.id = crypto.randomUUID();
        blocks.value.splice(idx + 1, 0, clone);
    }

    function setBlocks(newBlocks: ReportBlock[]) {
        blocks.value = newBlocks;
    }

    function markEdited(id: string, isEdited: boolean) {
        const block = blocks.value.find(b => b.id === id);
        if (block) block.isEdited = isEdited;
    }

    function toggleEditMode() {
        editMode.value = !editMode.value;
    }

    return {
        blocks,
        editMode,
        addBlock,
        removeBlock,
        reorderBlocks,
        updateBlockConfig,
        updateBlockStyle,
        duplicateBlock,
        setBlocks,
        markEdited,
        toggleEditMode,
    };
}
