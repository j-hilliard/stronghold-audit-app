<template>
    <div class="rte-wrapper" :class="{ 'rte--focused': isFocused, 'rte--readonly': readonly }">
        <!-- Toolbar — visible when focused and not readonly -->
        <div v-if="!readonly && editor && isFocused" class="rte-toolbar">
            <!-- Bold / Italic / Underline -->
            <button type="button" @mousedown.prevent="editor.chain().focus().toggleBold().run()"
                :class="['rte-btn', { active: editor.isActive('bold') }]" title="Bold">
                <strong>B</strong>
            </button>
            <button type="button" @mousedown.prevent="editor.chain().focus().toggleItalic().run()"
                :class="['rte-btn', { active: editor.isActive('italic') }]" title="Italic">
                <em>I</em>
            </button>
            <button type="button" @mousedown.prevent="editor.chain().focus().toggleUnderline().run()"
                :class="['rte-btn', { active: editor.isActive('underline') }]" title="Underline">
                <span style="text-decoration:underline">U</span>
            </button>

            <span class="rte-sep" />

            <!-- Lists -->
            <button type="button" @mousedown.prevent="editor.chain().focus().toggleBulletList().run()"
                :class="['rte-btn', { active: editor.isActive('bulletList') }]" title="Bullet list">
                <i class="pi pi-list" />
            </button>
            <button type="button" @mousedown.prevent="editor.chain().focus().toggleOrderedList().run()"
                :class="['rte-btn', { active: editor.isActive('orderedList') }]" title="Numbered list">
                <i class="pi pi-sort-amount-down" />
            </button>

            <span class="rte-sep" />

            <!-- Alignment -->
            <button type="button" @mousedown.prevent="editor.chain().focus().setTextAlign('left').run()"
                :class="['rte-btn', { active: editor.isActive({ textAlign: 'left' }) }]" title="Align left">
                <i class="pi pi-align-left" />
            </button>
            <button type="button" @mousedown.prevent="editor.chain().focus().setTextAlign('center').run()"
                :class="['rte-btn', { active: editor.isActive({ textAlign: 'center' }) }]" title="Align center">
                <i class="pi pi-align-center" />
            </button>
            <button type="button" @mousedown.prevent="editor.chain().focus().setTextAlign('right').run()"
                :class="['rte-btn', { active: editor.isActive({ textAlign: 'right' }) }]" title="Align right">
                <i class="pi pi-align-right" />
            </button>

            <span class="rte-sep" />

            <!-- Heading / size shortcuts -->
            <button type="button"
                @mousedown.prevent="editor.chain().focus().setParagraph().run()"
                :class="['rte-btn rte-size-btn', { active: editor.isActive('paragraph') }]" title="Normal">
                P
            </button>
            <button type="button"
                @mousedown.prevent="editor.chain().focus().toggleHeading({ level: 2 }).run()"
                :class="['rte-btn rte-size-btn', { active: editor.isActive('heading', { level: 2 }) }]" title="Heading">
                H2
            </button>
            <button type="button"
                @mousedown.prevent="editor.chain().focus().toggleHeading({ level: 3 }).run()"
                :class="['rte-btn rte-size-btn', { active: editor.isActive('heading', { level: 3 }) }]" title="Subheading">
                H3
            </button>

            <span class="rte-sep" />

            <!-- Text color -->
            <label class="rte-btn rte-color-label" title="Text color">
                <span class="rte-color-icon" :style="{ borderBottomColor: currentColor }">A</span>
                <input type="color" class="rte-color-input"
                    :value="currentColor"
                    @input="onColorInput" />
            </label>

            <!-- Clear formatting -->
            <button type="button" @mousedown.prevent="editor.chain().focus().clearNodes().unsetAllMarks().run()"
                class="rte-btn rte-clear-btn" title="Clear formatting">
                <i class="pi pi-times" />
            </button>
        </div>

        <!-- Tiptap editor surface (edit mode) -->
        <EditorContent v-if="!readonly" :editor="editor" class="rte-content" />

        <!-- Read-only render (print / preview) -->
        <div v-else class="rte-output" v-html="safeHtml" />

        <!-- Placeholder -->
        <div
            v-if="!readonly && !hasContent"
            class="rte-placeholder"
            @click="editor?.commands.focus()"
        >
            {{ placeholder }}
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onBeforeUnmount } from 'vue';
import { useEditor, EditorContent } from '@tiptap/vue-3';
import StarterKit from '@tiptap/starter-kit';
import Underline from '@tiptap/extension-underline';
import { TextStyle } from '@tiptap/extension-text-style';
import { Color } from '@tiptap/extension-color';
import TextAlign from '@tiptap/extension-text-align';

const props = withDefaults(defineProps<{
    modelValue: string;
    placeholder?: string;
    readonly?: boolean;
}>(), {
    placeholder: 'Click to add text…',
    readonly: false,
});

const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
    (e: 'focus'): void;
    (e: 'blur'): void;
}>();

const isFocused = ref(false);

// Sanitise for v-html (basic — no XSS vectors expected in internal use)
const safeHtml = computed(() => props.modelValue || '');

const hasContent = computed(() => {
    if (!props.modelValue) return false;
    // Strip all tags and check for non-whitespace characters
    return props.modelValue.replace(/<[^>]*>/g, '').trim().length > 0;
});

const editor = useEditor({
    content: props.modelValue || '',
    extensions: [
        StarterKit.configure({
            // BulletList/OrderedList/ListItem already bundled in StarterKit
        }),
        Underline,
        TextStyle,
        Color,
        TextAlign.configure({ types: ['heading', 'paragraph'] }),
    ],
    editorProps: {
        attributes: { class: 'rte-editor-inner', spellcheck: 'false' },
    },
    onUpdate: ({ editor: e }) => {
        const html = e.getHTML();
        // Avoid emitting empty paragraph as meaningful content
        const normalized = html === '<p></p>' ? '' : html;
        emit('update:modelValue', normalized);
    },
    onFocus: () => {
        isFocused.value = true;
        emit('focus');
    },
    onBlur: () => {
        isFocused.value = false;
        emit('blur');
    },
});

// Sync external updates (e.g., regeneration) into editor without triggering onUpdate loop
watch(() => props.modelValue, (newVal) => {
    if (!editor.value) return;
    const current = editor.value.getHTML();
    const incoming = newVal || '';
    if (current !== incoming) {
        editor.value.commands.setContent(incoming, false);
    }
});

const currentColor = computed(() => {
    return editor.value?.getAttributes('textStyle').color || '#f1f5f9';
});

function onColorInput(e: Event) {
    const color = (e.target as HTMLInputElement).value;
    editor.value?.chain().focus().setColor(color).run();
}

onBeforeUnmount(() => {
    editor.value?.destroy();
});
</script>

<style scoped>
.rte-wrapper {
    position: relative;
    min-height: 2rem;
}

/* ── Toolbar ───────────────────────────────────────────────────────────── */
.rte-toolbar {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 2px;
    padding: 4px 6px;
    margin-bottom: 6px;
    background: #1e293b;
    border: 1px solid #334155;
    border-radius: 5px;
}

.rte-btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 26px;
    height: 24px;
    padding: 0 4px;
    border-radius: 3px;
    border: none;
    background: transparent;
    color: #94a3b8;
    font-size: 12px;
    cursor: pointer;
    transition: background 0.1s, color 0.1s;
    line-height: 1;
    font-family: inherit;
}

.rte-btn:hover {
    background: #334155;
    color: #e2e8f0;
}

.rte-btn.active {
    background: #1e40af;
    color: #ffffff;
}

.rte-size-btn {
    font-size: 10px;
    font-weight: 700;
    letter-spacing: 0.01em;
    min-width: 24px;
}

.rte-clear-btn {
    margin-left: auto;
    font-size: 10px;
    color: #64748b;
}

.rte-sep {
    width: 1px;
    height: 16px;
    background: #334155;
    margin: 0 2px;
    flex-shrink: 0;
}

/* ── Color input ──────────────────────────────────────────────────────── */
.rte-color-label {
    cursor: pointer;
    position: relative;
}

.rte-color-icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-weight: 700;
    font-size: 13px;
    width: 20px;
    border-bottom: 3px solid currentColor;
    padding-bottom: 1px;
    line-height: 1;
}

.rte-color-input {
    position: absolute;
    opacity: 0;
    width: 0;
    height: 0;
    pointer-events: none;
}

.rte-color-label:hover .rte-color-input {
    pointer-events: all;
    opacity: 1;
    width: 100%;
    height: 100%;
    top: 0;
    left: 0;
}

/* ── Editor surface ───────────────────────────────────────────────────── */
.rte-content {
    position: relative;
}

/* ProseMirror resets for our dark theme */
.rte-content :deep(.rte-editor-inner) {
    outline: none;
    min-height: 2.5rem;
    font-size: 0.875rem;
    line-height: 1.6;
    color: inherit;
    caret-color: #60a5fa;
}

.rte-content :deep(.rte-editor-inner p) {
    margin: 0 0 0.5em;
}

.rte-content :deep(.rte-editor-inner p:last-child) {
    margin-bottom: 0;
}

.rte-content :deep(.rte-editor-inner h2) {
    font-size: 1.1rem;
    font-weight: 700;
    margin: 0.5em 0 0.3em;
    line-height: 1.3;
}

.rte-content :deep(.rte-editor-inner h3) {
    font-size: 0.95rem;
    font-weight: 600;
    margin: 0.4em 0 0.25em;
    line-height: 1.3;
}

.rte-content :deep(.rte-editor-inner ul) {
    list-style: disc;
    padding-left: 1.4em;
    margin: 0.25em 0 0.5em;
}

.rte-content :deep(.rte-editor-inner ol) {
    list-style: decimal;
    padding-left: 1.4em;
    margin: 0.25em 0 0.5em;
}

.rte-content :deep(.rte-editor-inner li) {
    margin: 0.1em 0;
}

.rte-content :deep(.rte-editor-inner li p) {
    margin: 0;
}

.rte-content :deep(.rte-editor-inner strong) {
    font-weight: 700;
}

.rte-content :deep(.rte-editor-inner em) {
    font-style: italic;
}

.rte-content :deep(.rte-editor-inner u) {
    text-decoration: underline;
}

/* ── Read-only output ─────────────────────────────────────────────────── */
.rte-output {
    font-size: 0.875rem;
    line-height: 1.6;
    color: inherit;
}

.rte-output :deep(p) { margin: 0 0 0.5em; }
.rte-output :deep(p:last-child) { margin-bottom: 0; }
.rte-output :deep(h2) { font-size: 1.1rem; font-weight: 700; margin: 0.5em 0 0.3em; }
.rte-output :deep(h3) { font-size: 0.95rem; font-weight: 600; margin: 0.4em 0 0.25em; }
.rte-output :deep(ul) { list-style: disc; padding-left: 1.4em; margin: 0.25em 0 0.5em; }
.rte-output :deep(ol) { list-style: decimal; padding-left: 1.4em; margin: 0.25em 0 0.5em; }
.rte-output :deep(li) { margin: 0.1em 0; }
.rte-output :deep(li p) { margin: 0; }
.rte-output :deep(strong) { font-weight: 700; }
.rte-output :deep(em) { font-style: italic; }
.rte-output :deep(u) { text-decoration: underline; }

/* ── Placeholder ──────────────────────────────────────────────────────── */
.rte-placeholder {
    position: absolute;
    top: 0;
    left: 0;
    pointer-events: none;
    font-size: 0.875rem;
    color: #475569;
    font-style: italic;
    cursor: text;
}

/* ── Print ────────────────────────────────────────────────────────────── */
@media print {
    .rte-toolbar { display: none !important; }
    .rte-placeholder { display: none !important; }
}
</style>
