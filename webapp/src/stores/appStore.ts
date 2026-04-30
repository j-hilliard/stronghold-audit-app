import { computed } from 'vue';
import { apps } from '@/apps.ts';
import { defineStore } from 'pinia';
import { useRoute } from 'vue-router';
import { useUserStore } from '@/stores/userStore.ts';

export const useAppStore = defineStore('app', () => {
    const route = useRoute();
    const userStore = useUserStore();

    const currentApp = computed(() => {
        return apps.auditManagement;
    });

    const menu = computed(() => {
        const routes = [{ label: currentApp.value.name, items: currentApp.value.menu.user }];

        // Show admin section for system Administrators, TemplateAdmins, AuditAdmins,
        // AND ITAdmins (who need access to the Users link inside the admin block).
        const showAdmin = userStore.isAdmin ||
            (currentApp.value.name === 'Compliance Audit' &&
                (userStore.canAccessAdminTemplates || userStore.isITAdmin));

        if (showAdmin && currentApp.value.menu.admin.length) {
            routes.push({ label: 'Administration', items: currentApp.value.menu.admin });
        }

        return routes;
    });

    return {
        menu,
        currentApp,
    };
});
