<template>
    <div class="notif-bell" ref="containerRef">
        <!-- Bell trigger -->
        <button
            class="notif-trigger"
            :class="{ 'notif-trigger--active': open }"
            :title="unreadCount > 0 ? `${unreadCount} unread notification${unreadCount > 1 ? 's' : ''}` : 'Notifications'"
            @click="toggle"
        >
            <i class="pi pi-bell text-sm" />
            <span v-if="unreadCount > 0" class="notif-badge">{{ unreadCount > 99 ? '99+' : unreadCount }}</span>
        </button>

        <!-- Dropdown panel -->
        <Transition name="notif-fade">
            <div v-if="open" class="notif-panel">
                <!-- Header -->
                <div class="notif-header">
                    <span class="text-sm font-semibold text-slate-200">Notifications</span>
                    <button
                        v-if="unreadCount > 0"
                        class="text-xs text-blue-400 hover:text-blue-300 transition-colors"
                        @click="markAllRead"
                    >
                        Mark all read
                    </button>
                </div>

                <!-- Loading -->
                <div v-if="loading" class="notif-empty">
                    <i class="pi pi-spin pi-spinner text-slate-500" />
                </div>

                <!-- Empty state -->
                <div v-else-if="notifications.length === 0" class="notif-empty">
                    <i class="pi pi-check-circle text-slate-600 text-xl mb-1" />
                    <span class="text-xs text-slate-500">All caught up</span>
                </div>

                <!-- List -->
                <ul v-else class="notif-list">
                    <li
                        v-for="n in notifications"
                        :key="n.id"
                        class="notif-item"
                        :class="{ 'notif-item--unread': !n.isRead }"
                        @click="handleClick(n)"
                    >
                        <div class="notif-icon" :class="iconClass(n.type)">
                            <i :class="iconName(n.type)" class="text-xs" />
                        </div>
                        <div class="flex-1 min-w-0">
                            <p class="text-xs font-semibold text-slate-200 truncate">{{ n.title }}</p>
                            <p class="text-xs text-slate-400 leading-snug mt-0.5 line-clamp-2">{{ n.body }}</p>
                            <p class="text-[10px] text-slate-600 mt-1">{{ timeAgo(n.createdAt) }}</p>
                        </div>
                        <div v-if="!n.isRead" class="w-1.5 h-1.5 rounded-full bg-blue-500 flex-shrink-0 mt-1" />
                    </li>
                </ul>
            </div>
        </Transition>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import type { NotificationDto } from '@/apiclient/auditClient';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';

const service    = useAuditService();
const router     = useRouter();
const open       = ref(false);
const loading    = ref(false);
const containerRef = ref<HTMLElement | null>(null);

const notifications = ref<NotificationDto[]>([]);
const unreadCount   = ref(0);

async function load() {
    loading.value = true;
    try {
        const result = await service.getNotifications(false, 20);
        notifications.value = result.items;
        unreadCount.value   = result.unreadCount;
    } catch {
        // non-fatal
    } finally {
        loading.value = false;
    }
}

function toggle() {
    open.value = !open.value;
    if (open.value) load();
}

async function markAllRead() {
    await service.markAllNotificationsRead();
    notifications.value.forEach(n => { n.isRead = true; });
    unreadCount.value = 0;
}

function handleClick(n: NotificationDto) {
    if (!n.isRead) {
        service.markNotificationRead(n.id);
        n.isRead = true;
        unreadCount.value = Math.max(0, unreadCount.value - 1);
    }
    open.value = false;
    if (n.linkUrl) router.push(n.linkUrl);
}

function iconName(type: NotificationDto['type']) {
    switch (type) {
        case 'AuditSubmitted':   return 'pi pi-file-edit';
        case 'AuditApproved':    return 'pi pi-check-circle';
        case 'AuditDistributed': return 'pi pi-send';
        case 'CaAssigned':       return 'pi pi-user-edit';
        case 'CaCompleted':      return 'pi pi-check';
        default:                 return 'pi pi-bell';
    }
}

function iconClass(type: NotificationDto['type']) {
    switch (type) {
        case 'AuditSubmitted':   return 'notif-icon--blue';
        case 'AuditApproved':    return 'notif-icon--green';
        case 'AuditDistributed': return 'notif-icon--purple';
        case 'CaAssigned':       return 'notif-icon--amber';
        case 'CaCompleted':      return 'notif-icon--green';
        default:                 return 'notif-icon--blue';
    }
}

function timeAgo(iso: string): string {
    const diff = Date.now() - new Date(iso).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1)  return 'just now';
    if (mins < 60) return `${mins}m ago`;
    const hrs = Math.floor(mins / 60);
    if (hrs < 24)  return `${hrs}h ago`;
    return `${Math.floor(hrs / 24)}d ago`;
}

function onDocClick(e: MouseEvent) {
    if (!containerRef.value?.contains(e.target as Node)) open.value = false;
}

onMounted(() => {
    document.addEventListener('click', onDocClick, true);
    load(); // preload count on mount
});
onUnmounted(() => document.removeEventListener('click', onDocClick, true));
</script>

<style scoped>
.notif-bell { position: relative; display: flex; align-items: center; }

.notif-trigger {
    position: relative;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 36px;
    height: 36px;
    border-radius: 8px;
    border: none;
    background: transparent;
    color: #94a3b8;
    cursor: pointer;
    transition: background 0.12s, color 0.12s;
}
.notif-trigger:hover,
.notif-trigger--active { background: rgba(30,41,59,0.8); color: #e2e8f0; }

.notif-badge {
    position: absolute;
    top: 3px;
    right: 3px;
    min-width: 16px;
    height: 16px;
    padding: 0 3px;
    border-radius: 8px;
    background: #ef4444;
    color: #fff;
    font-size: 9px;
    font-weight: 700;
    line-height: 16px;
    text-align: center;
}

.notif-panel {
    position: absolute;
    top: calc(100% + 8px);
    right: 0;
    width: 340px;
    background: #1e293b;
    border: 1px solid #334155;
    border-radius: 10px;
    box-shadow: 0 8px 24px rgba(0,0,0,0.5);
    z-index: 1000;
    overflow: hidden;
}

.notif-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 12px 14px 10px;
    border-bottom: 1px solid #334155;
}

.notif-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 4px;
    padding: 28px 16px;
    color: #64748b;
}

.notif-list {
    list-style: none;
    margin: 0;
    padding: 0;
    max-height: 360px;
    overflow-y: auto;
}

.notif-item {
    display: flex;
    align-items: flex-start;
    gap: 10px;
    padding: 10px 14px;
    cursor: pointer;
    border-bottom: 1px solid rgba(51,65,85,0.4);
    transition: background 0.1s;
}
.notif-item:last-child { border-bottom: none; }
.notif-item:hover { background: rgba(30,58,95,0.25); }
.notif-item--unread { background: rgba(30,58,95,0.15); }

.notif-icon {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 28px;
    height: 28px;
    border-radius: 6px;
    flex-shrink: 0;
}
.notif-icon--blue   { background: rgba(59,130,246,0.15); color: #60a5fa; }
.notif-icon--green  { background: rgba(34,197,94,0.15);  color: #4ade80; }
.notif-icon--purple { background: rgba(168,85,247,0.15); color: #c084fc; }
.notif-icon--amber  { background: rgba(251,191,36,0.15); color: #fbbf24; }

.notif-fade-enter-active, .notif-fade-leave-active { transition: opacity 0.12s, transform 0.12s; }
.notif-fade-enter-from, .notif-fade-leave-to { opacity: 0; transform: translateY(-4px); }
</style>
