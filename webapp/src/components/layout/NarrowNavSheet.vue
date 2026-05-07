<template>
    <transition name="narrow-nav">
        <div
            v-if="isOpen"
            class="narrow-nav-backdrop"
            :style="backdropStyle"
            @click.self="$emit('close')"
        >
            <div class="narrow-nav-panel" @click.stop>
                <!-- Header: logo + close -->
                <div class="narrow-nav-header">
                    <img :src="logo" alt="Stronghold" class="narrow-nav-logo" />
                    <button class="narrow-nav-close-btn" @click="$emit('close')">
                        <i class="pi pi-times" />
                    </button>
                </div>

                <!-- Navigation -->
                <nav class="narrow-nav-body" role="navigation">
                    <template v-for="section in menu" :key="section.label">
                        <div class="narrow-nav-section-label">{{ section.label }}</div>
                        <template v-for="item in section.items" :key="item.label">
                            <router-link
                                v-if="item.to && isVisible(item)"
                                :to="item.to"
                                class="narrow-nav-link"
                                active-class="narrow-nav-link--active"
                                @click="$emit('close')"
                            >
                                <i :class="item.icon" />
                                <span>{{ item.label }}</span>
                            </router-link>
                        </template>
                    </template>
                </nav>

                <!-- Footer: identity + logout -->
                <div class="narrow-nav-footer">
                    <div class="narrow-nav-user-row">
                        <div class="narrow-nav-avatar">
                            <img
                                v-if="userStore.userPhoto"
                                :src="userStore.userPhoto"
                                class="narrow-nav-photo"
                            />
                            <div v-else class="narrow-nav-avatar-placeholder">
                                <i class="pi pi-user" />
                            </div>
                        </div>
                        <div class="narrow-nav-user-info">
                            <span class="narrow-nav-user-name">{{ userStore.userFullName }}</span>
                            <span v-if="userStore.userTitle" class="narrow-nav-user-title">
                                {{ userStore.userTitle }}
                            </span>
                        </div>
                    </div>
                    <button class="narrow-nav-logout-btn" @click="logout">
                        <i class="pi pi-power-off" />
                        <span>Logout</span>
                    </button>
                </div>
            </div>
        </div>
    </transition>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { storeToRefs } from 'pinia';
import { useAppStore } from '@/stores/appStore';
import { useUserStore } from '@/stores/userStore';
import { useNarrowScreen } from '@/composables/useNarrowScreen';
import logo from '@/assets/images/header-logo.svg';

defineProps<{ isOpen: boolean }>();
defineEmits<{ close: [] }>();

const appStore = useAppStore();
const userStore = useUserStore();
const { menu } = storeToRefs(appStore);
const { previewFrameWidth } = useNarrowScreen();

// When the dev viewport switcher constrains #app to a centred preview frame,
// position the backdrop inside that frame rather than against the browser viewport.
const backdropStyle = computed(() => {
    const fw = previewFrameWidth.value;
    if (!fw || window.innerWidth <= fw) return {};
    const left = Math.max(0, Math.round((window.innerWidth - fw) / 2));
    return { left: `${left}px`, width: `${fw}px` };
});

function isVisible(item: any): boolean {
    if (item.isHidden) return false;
    if (typeof item.visible === 'function') return item.visible();
    if (typeof item.visible === 'boolean') return item.visible;
    return true;
}

function logout() {
    userStore.logoutUser();
}
</script>

<style scoped>
/* ── Backdrop ────────────────────────────────────────────────────────────────── */
.narrow-nav-backdrop {
    position: fixed;
    inset: 0;
    z-index: 9000;
    background: rgba(0, 0, 0, 0.55);
    backdrop-filter: blur(2px);
    -webkit-backdrop-filter: blur(2px);
}

/* ── Slide-in panel ──────────────────────────────────────────────────────────── */
.narrow-nav-panel {
    position: absolute;
    top: 0;
    left: 0;
    height: 100%;
    width: 280px;
    max-width: 85vw;
    background: #0f172a;
    border-right: 1px solid rgba(148, 163, 184, 0.15);
    display: flex;
    flex-direction: column;
    box-shadow: 4px 0 28px rgba(0, 0, 0, 0.55);
    overflow: hidden;
}

/* ── Header ──────────────────────────────────────────────────────────────────── */
.narrow-nav-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 14px 16px;
    border-bottom: 1px solid rgba(148, 163, 184, 0.12);
    flex-shrink: 0;
}
.narrow-nav-logo {
    height: 28px;
    width: auto;
}
.narrow-nav-close-btn {
    width: 32px;
    height: 32px;
    border: none;
    background: rgba(148, 163, 184, 0.08);
    border-radius: 6px;
    color: #94a3b8;
    font-size: 0.875rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background 0.15s ease, color 0.15s ease;
}
.narrow-nav-close-btn:hover {
    background: rgba(148, 163, 184, 0.18);
    color: #f1f5f9;
}

/* ── Nav body ────────────────────────────────────────────────────────────────── */
.narrow-nav-body {
    flex: 1;
    overflow-y: auto;
    padding: 8px 0;
}
.narrow-nav-section-label {
    padding: 16px 20px 5px;
    font-size: 0.63rem;
    font-weight: 700;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: #475569;
}
.narrow-nav-link {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 11px 20px 11px 17px;
    color: #94a3b8;
    text-decoration: none;
    font-size: 0.875rem;
    font-weight: 500;
    border-left: 3px solid transparent;
    transition: background 0.12s ease, color 0.12s ease, border-color 0.12s ease;
}
.narrow-nav-link i {
    font-size: 0.9rem;
    flex-shrink: 0;
}
.narrow-nav-link:hover {
    background: rgba(99, 179, 237, 0.08);
    color: #e2e8f0;
    border-left-color: rgba(99, 179, 237, 0.4);
}
.narrow-nav-link--active {
    background: rgba(99, 179, 237, 0.14) !important;
    color: #fff !important;
    border-left-color: #63b3ed !important;
    font-weight: 600;
}

/* ── Footer ──────────────────────────────────────────────────────────────────── */
.narrow-nav-footer {
    border-top: 1px solid rgba(148, 163, 184, 0.12);
    padding: 12px 16px 16px;
    flex-shrink: 0;
}
.narrow-nav-user-row {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 6px 4px 10px;
}
.narrow-nav-avatar {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    flex-shrink: 0;
    overflow: hidden;
}
.narrow-nav-photo {
    width: 100%;
    height: 100%;
    object-fit: cover;
}
.narrow-nav-avatar-placeholder {
    width: 100%;
    height: 100%;
    background: #1e293b;
    border: 1px solid rgba(148, 163, 184, 0.2);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}
.narrow-nav-avatar-placeholder i {
    color: #64748b;
    font-size: 0.875rem;
}
.narrow-nav-user-info {
    display: flex;
    flex-direction: column;
    min-width: 0;
}
.narrow-nav-user-name {
    color: #e2e8f0;
    font-size: 0.8125rem;
    font-weight: 600;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}
.narrow-nav-user-title {
    color: #64748b;
    font-size: 0.7rem;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
}
.narrow-nav-logout-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    width: 100%;
    padding: 9px 12px;
    border: 1px solid rgba(148, 163, 184, 0.15);
    border-radius: 8px;
    background: transparent;
    color: #94a3b8;
    font-size: 0.825rem;
    cursor: pointer;
    transition: background 0.12s ease, color 0.12s ease, border-color 0.12s ease;
}
.narrow-nav-logout-btn:hover {
    background: rgba(239, 68, 68, 0.08);
    color: #f87171;
    border-color: rgba(239, 68, 68, 0.25);
}

/* ── Slide transition ────────────────────────────────────────────────────────── */
.narrow-nav-enter-active {
    transition: opacity 0.2s ease;
}
.narrow-nav-leave-active {
    transition: opacity 0.18s ease;
}
.narrow-nav-enter-active .narrow-nav-panel,
.narrow-nav-leave-active .narrow-nav-panel {
    transition: transform 0.22s cubic-bezier(0.4, 0, 0.2, 1);
}
.narrow-nav-enter-from,
.narrow-nav-leave-to {
    opacity: 0;
}
.narrow-nav-enter-from .narrow-nav-panel,
.narrow-nav-leave-to .narrow-nav-panel {
    transform: translateX(-100%);
}
</style>
