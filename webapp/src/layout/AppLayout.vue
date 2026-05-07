<template>
    <div :class="containerClass" @click="onDocumentClick">
        <PrimeVueToast />
        <TheTopBar
            :topbarTheme="topbarTheme"
            :activeTopbarItem="activeTopBarItem"
            @topbar-item-click="onTopbarItemClick"
            @menu-button-click="onMenuButtonClick($event)"
        />

        <!-- Desktop sidebar — hidden in narrow mode via CSS -->
        <div class="menu-wrapper" @click="onMenuClick($event)">
            <TheMenu
                :model="appStore.menu"
                :active="menuActive"
                :menuMode="menuMode"
                :mobileMenuActive="staticMenuMobileActive"
                @menu-click="onMenuClick"
                @menuitem-click="onMenuItemClick"
            />
        </div>

        <div class="layout-main bg-gradient-to-b from-slate-900 to-slate-800">
            <div class="layout-content p-4 sm:p-6 lg:p-12">
                <router-view v-slot="{ Component }">
                    <component v-if="Component" :is="Component" :key="route.fullPath" />
                    <div v-else class="text-slate-400" data-testid="route-view-loading">
                        Loading...
                    </div>
                </router-view>
            </div>
        </div>

        <!-- Desktop mobile-overlay mask — suppressed in narrow mode via CSS -->
        <div v-if="staticMenuMobileActive" class="layout-mask modal-in" @click="staticMenuMobileActive = false"></div>

        <!-- Narrow (tablet/phone) nav sheet — hamburger-triggered in-frame overlay -->
        <NarrowNavSheet
            v-if="isNarrowScreen"
            :is-open="narrowNavOpen"
            @close="narrowNavOpen = false"
        />

        <DevRoleSwitcher v-if="isDev" />
        <DevViewportSwitcher v-if="isDev" />
        <ConfirmDialog />
    </div>
</template>

<script setup lang="ts">
import PrimeVueToast from 'primevue/toast';
import ConfirmDialog from 'primevue/confirmdialog';
import { useToast } from 'primevue/usetoast';
import EventBus from '@/layout/event-bus.ts';
import TheMenu from '@/components/layout/TheMenu.vue';
import TheTopBar from '@/components/layout/TheTopBar.vue';
import NarrowNavSheet from '@/components/layout/NarrowNavSheet.vue';
import DevRoleSwitcher from '@/components/layout/DevRoleSwitcher.vue';
import DevViewportSwitcher from '@/components/layout/DevViewportSwitcher.vue';
import { useAppStore } from '@/stores/appStore.ts';
import { ref, computed, onBeforeMount, onUnmounted, watch } from 'vue';
import { useRoute } from 'vue-router';

const isDev = import.meta.env.DEV;

const menuTheme = ref('dim');
const menuClick = ref(false);
const menuActive = ref(false);
const searchClick = ref(false);
const configClick = ref(false);
const menuMode = ref('static');
const searchActive = ref(false);
const topbarTheme = ref('light');
const activeTopBarItem = ref('');
const rightPanelClick = ref(false);
const rightPanelActive = ref(false);
const topbarMenuActive = ref(false);
const overlayMenuActive = ref(false);
const staticMenuMobileActive = ref(false);
const staticMenuDesktopInactive = ref(false);

// Narrow nav sheet open/close state
const narrowNavOpen = ref(false);

const appStore = useAppStore();
const toast = useToast();
const route = useRoute();

// Close narrow nav on every route change (handles programmatic navigation too)
watch(() => route.fullPath, () => {
    narrowNavOpen.value = false;
});

const isMobile = computed(() => window.innerWidth <= 640);
const isDesktop = computed(() => window.innerWidth > 992);
const isOverlay = computed(() => menuMode.value === 'overlay');

// Narrow screen — tracks ONLY real viewport width and dev-viewport-change events.
// NOT coupled to staticMenuDesktopInactive: collapsing the sidebar on desktop
// must NOT trigger narrow layout mode.
const isNarrowScreen = ref(window.innerWidth <= 768);

function updateNarrowScreen() {
    isNarrowScreen.value = window.innerWidth <= 768;
    if (!isNarrowScreen.value) narrowNavOpen.value = false;
}

// containerClass:
//   - In narrow mode, suppress all sidebar-related classes so theme.css
//     desktop sidebar logic never fires.
//   - Only layout-narrow + base classes are added in narrow mode.
const containerClass = computed(() => [
    'layout-wrapper',
    {
        'layout-static':  menuMode.value === 'static',
        'layout-overlay': menuMode.value === 'overlay',
        // Sidebar-active classes MUST NOT fire in narrow mode (causes content offset)
        'layout-overlay-active':  !isNarrowScreen.value && overlayMenuActive.value,
        'layout-mobile-active':   !isNarrowScreen.value && staticMenuMobileActive.value,
        'layout-static-active':   !isNarrowScreen.value && !staticMenuDesktopInactive.value && menuMode.value === 'static',
        'layout-narrow': isNarrowScreen.value,
    },
    `layout-menu-${menuTheme.value} layout-topbar-${topbarTheme.value}`,
]);

watch(() => appStore.currentApp, (app) => {
    toast.add({ severity: 'info', summary: 'App Change', detail: `You are now working in ${app.name}`, life: 3000 });
});

onBeforeMount(() => {
    document.addEventListener('click', onDocumentClick);
    window.addEventListener('resize', updateNarrowScreen);
    if (isDev) window.addEventListener('dev-viewport-change', onDevViewportChange);
});

onUnmounted(() => {
    document.removeEventListener('click', onDocumentClick);
    window.removeEventListener('resize', updateNarrowScreen);
    if (isDev) window.removeEventListener('dev-viewport-change', onDevViewportChange);
});

function onDocumentClick() {
    if (!searchClick.value) searchActive.value = false;
    if (!rightPanelClick.value) rightPanelActive.value = false;
    if (!menuClick.value) overlayMenuActive.value = false;
    searchClick.value = configClick.value = menuClick.value = rightPanelClick.value = false;
}

function blockBodyScroll() {
    document.body.classList.add('blocked-scroll');
}
function unblockBodyScroll() {
    document.body.classList.remove('blocked-scroll');
}

function onMenuClick(event: Event) {
    menuClick.value = true;
    event.stopPropagation();
}

function onMenuButtonClick(event: Event) {
    menuClick.value = true;
    topbarMenuActive.value = false;

    if (isNarrowScreen.value) {
        // Narrow mode: toggle in-frame nav sheet. Never touch sidebar state.
        narrowNavOpen.value = !narrowNavOpen.value;
    } else if (isOverlay.value && !isMobile.value) {
        overlayMenuActive.value = !overlayMenuActive.value;
    } else if (isDesktop.value) {
        staticMenuDesktopInactive.value = !staticMenuDesktopInactive.value;
    } else {
        staticMenuMobileActive.value = !staticMenuMobileActive.value;
        if (staticMenuMobileActive.value) blockBodyScroll();
        else unblockBodyScroll();
    }

    event.preventDefault();
}

function onMenuItemClick(event) {
    if (!event.item.items) {
        EventBus.emit('reset-active-index');
        overlayMenuActive.value = false;
    }
    if (isMobile.value) {
        onMenuButtonClick(event.originalEvent);
    }
}

function onTopbarItemClick(event) {
    activeTopBarItem.value = activeTopBarItem.value === event.item ? '' : event.item;
}

// Dev-only: sync narrow state when dev viewport switcher changes preset
function onDevViewportChange(e: Event) {
    const { preset } = (e as CustomEvent<{ preset: string }>).detail;
    narrowNavOpen.value = false;
    staticMenuMobileActive.value = false;
    if (preset === 'desktop') {
        staticMenuDesktopInactive.value = false;
        isNarrowScreen.value = window.innerWidth <= 768;
    } else {
        // phone or tablet: switch to narrow mode
        isNarrowScreen.value = true;
    }
}
</script>
