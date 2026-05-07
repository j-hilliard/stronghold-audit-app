<template>
    <div class="layout-topbar bg-slate-900 border-b border-slate-700/50">
        <div class="layout-topbar-wrapper">
            <div class="layout-topbar-left">
                <router-link to="/">
                    <div class="layout-topbar-logo">
                        <img id="app-logo" :src="logo" alt="poseidon-layout" />
                    </div>
                </router-link>
            </div>
            <div class="layout-topbar-right">
                <a href="#" @click="$emit('menu-button-click', $event)" class="menu-button bg-slate-800">
                    <i class="pi pi-bars" />
                </a>
                <!-- Breadcrumb hidden on narrow screens to prevent overlap -->
                <Breadcrumb :model="breadcrumbItems" class="topbar-breadcrumb ml-4" />
                <ul class="layout-topbar-actions">
                    <li class="topbar-item">
                        <NotificationBell />
                    </li>
                    <li class="topbar-item theme-toggle-item">
                        <button
                            class="theme-toggle-btn"
                            :title="isDark ? 'Switch to Light Mode' : 'Switch to Dark Mode'"
                            @click="toggle"
                        >
                            <i :class="isDark ? 'pi pi-sun' : 'pi pi-moon'" />
                        </button>
                    </li>
                    <li class="topbar-item user-profile"
                        :class="{ 'active-topmenuitem': activeTopbarItem === 'profile' }">
                        <a
                            href="#"
                            class="text-white flex items-center justify-center"
                            @click="onTopbarItemClick($event, 'profile')"
                        >
                            <img
                                v-if="userStore.userPhoto"
                                alt="demo"
                                :src="userStore.userPhoto"
                                class="profile-image round-image"
                            />
                            <div v-else class="w-10 h-10 rounded-full bg-slate-700 flex items-center justify-center">
                                <i class="pi pi-user text-slate-300" />
                            </div>
                            <!-- Profile name/title hidden on narrow screens -->
                            <div class="profile-info profile-info--text text-slate-300">
                                <h6>{{ userStore.userFullName }}</h6>
                                <span v-if="userStore.userTitle">{{ userStore.userTitle }}</span>
                            </div>
                        </a>
                        <ul class="fadeInDown">
                            <li class="layout-submenu-header">
                                <img
                                    v-if="userStore.userPhoto"
                                    alt="demo"
                                    :src="userStore.userPhoto"
                                    class="profile-image, round-image"
                                />
                                <div v-else
                                     class="w-10 h-10 rounded-full bg-slate-700 flex items-center justify-center">
                                    <i class="pi pi-user text-slate-300" />
                                </div>
                                <div class="profile-info">
                                    <h6>{{ userStore.userFullName }}</h6>
                                    <span v-if="userStore.userTitle">{{ userStore.userTitle }}</span>
                                </div>
                            </li>
                            <li role="menuitem">
                                <a href="#" @click="userStore.logoutUser">
                                    <i class="pi pi-power-off" />
                                    <h6>Logout</h6>
                                </a>
                            </li>
                        </ul>
                    </li>
                </ul>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import logo from '@/assets/images/header-logo.svg';
import { useUserStore } from '@/stores/userStore.ts';
import { useTheme } from '@/composables/useTheme';
import NotificationBell from '@/components/notifications/NotificationBell.vue';

const { isDark, toggle } = useTheme();

defineProps<{ activeTopbarItem: string; }>();

const emit = defineEmits([
    'menu-button-click',
    'topbar-item-click',
]);

const route = useRoute();
const userStore = useUserStore();

const breadcrumbItems = computed(() => {
    const routeBreadcrumbItems = route.meta?.breadcrumbItems;

    if (routeBreadcrumbItems instanceof Function) {
        return routeBreadcrumbItems() || [];
    }

    return [];
});

function onTopbarItemClick(event: Event, item: string) {
    emit('topbar-item-click', { originalEvent: event, item });
    event.preventDefault();
}
</script>

<style scoped>
.round-image {
    width: 100px;
    height: 100px;
    border-radius: 50%;
}

.theme-toggle-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border-radius: 50%;
    border: none;
    background: transparent;
    color: #94a3b8;
    font-size: 1.1rem;
    cursor: pointer;
    transition: background-color 0.15s ease, color 0.15s ease, transform 0.15s ease;
}
.theme-toggle-btn:hover {
    background: rgba(99, 179, 237, 0.15);
    color: #63b3ed;
    transform: rotate(15deg) scale(1.1);
}

/* Narrow-screen topbar overrides live in style.css to avoid the Vue scoped
   :global() compiler bug that strips child selectors and nukes the layout. */
</style>
