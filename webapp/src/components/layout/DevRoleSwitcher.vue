<template>
    <Teleport to="body">
        <!-- Small pill, bottom-right, low opacity until hovered -->
        <div class="dev-pill" @click.stop="open = !open" title="Dev role switcher">
            <span class="dev-pill__dot" />
            <span>DEV: {{ activeLabel }}</span>
        </div>

        <!-- Dropdown panel, opens above the pill -->
        <Transition name="dev-slide">
            <div v-if="open" class="dev-panel" @click.stop>
                <p class="dev-panel__title">Switch Role</p>

                <ul class="dev-panel__list">
                    <li>
                        <label class="dev-panel__item" :class="{ active: selected === null }">
                            <input type="radio" :checked="selected === null" @change="select(null)" />
                            All Access (no restriction)
                        </label>
                    </li>
                    <li v-for="role in ROLES" :key="role">
                        <label class="dev-panel__item" :class="{ active: selected === role }">
                            <input type="radio" :checked="selected === role" @change="select(role)" />
                            {{ role }}
                        </label>
                    </li>
                </ul>

                <p class="dev-panel__hint">Reloads page on change. Stored in localStorage.</p>
            </div>
        </Transition>
    </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';

const STORAGE_KEY = 'stronghold-audit-dev-role';

// Mirrors the 5 official roles in Shared/Enumerations/AuthorizationRole.cs
const ROLES = [
    'ITAdmin',
    'Auditor',
    'AuditAdmin',
    'Executive',
    'NormalUser',
];

const open     = ref(false);
const selected = ref<string | null>(null);

onMounted(() => {
    selected.value = localStorage.getItem(STORAGE_KEY) || null;
});

const activeLabel = computed(() => selected.value ?? 'All Access');

function select(role: string | null) {
    if (role === null) localStorage.removeItem(STORAGE_KEY);
    else localStorage.setItem(STORAGE_KEY, role);
    window.location.reload();
}
</script>

<style scoped>
.dev-pill {
    position: fixed;
    bottom: 10px;
    right: 12px;
    z-index: 9999;
    display: flex;
    align-items: center;
    gap: 5px;
    padding: 3px 9px;
    background: rgba(15,23,42,0.9);
    border: 1px solid #78350f;
    border-radius: 999px;
    cursor: pointer;
    font-size: 10px;
    font-weight: 600;
    color: #d97706;
    white-space: nowrap;
    max-width: 200px;
    overflow: hidden;
    text-overflow: ellipsis;
    user-select: none;
    opacity: 0.55;
    transition: opacity 0.15s, border-color 0.15s;
}
.dev-pill:hover { opacity: 1; border-color: #d97706; }
.dev-pill__dot {
    width: 5px; height: 5px;
    border-radius: 50%;
    background: #d97706;
    flex-shrink: 0;
}

.dev-panel {
    position: fixed;
    bottom: 36px;
    right: 12px;
    z-index: 9999;
    background: #0f172a;
    border: 1px solid #78350f;
    border-radius: 6px;
    width: 200px;
    padding: 10px 0 6px;
    box-shadow: 0 4px 20px rgba(0,0,0,0.6);
}
.dev-panel__title {
    font-size: 10px;
    font-weight: 700;
    letter-spacing: 0.08em;
    color: #d97706;
    text-transform: uppercase;
    padding: 0 12px 6px;
    border-bottom: 1px solid #1e293b;
    margin: 0 0 4px;
}
.dev-panel__list {
    list-style: none;
    margin: 0;
    padding: 0;
    max-height: 260px;
    overflow-y: auto;
}
.dev-panel__item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 5px 12px;
    font-size: 11px;
    color: #94a3b8;
    cursor: pointer;
    transition: background 0.1s, color 0.1s;
}
.dev-panel__item:hover { background: #1e293b; color: #e2e8f0; }
.dev-panel__item.active { color: #f59e0b; }
.dev-panel__item input[type="radio"] {
    accent-color: #d97706;
    flex-shrink: 0;
}
.dev-panel__hint {
    font-size: 10px;
    color: #334155;
    padding: 6px 12px 0;
    margin: 0;
}
.dev-panel__hint--error { color: #ef4444; }

.dev-slide-enter-active, .dev-slide-leave-active { transition: opacity 0.1s, transform 0.1s; }
.dev-slide-enter-from, .dev-slide-leave-to { opacity: 0; transform: translateY(4px); }
</style>
