import { computed } from 'vue';
import { useUserStore } from '@/stores/userStore';

export type PermissionPath =
    | 'audit.create'
    | 'audit.submit'
    | 'audit.reopen'
    | 'audit.close'
    | 'audit.review'
    | 'correctiveActions.assign'
    | 'correctiveActions.close'
    | 'reports.generate'
    | 'reports.schedule'
    | 'admin.access'
    | 'admin.manageUsers'
    | 'admin.manageSettings';

export function usePermissions() {
    const store = useUserStore();

    const permissions = computed(() => ({
        audit: {
            create: store.canCreateAudit,
            submit: store.canCreateAudit,
            reopen: store.isAuditReviewer || store.isAuditAdmin,
            close:  store.isAuditReviewer || store.isAuditAdmin,
            review: store.isAuditReviewer || store.isAuditAdmin,
        },
        correctiveActions: {
            assign: store.isAuditManager || store.isAuditReviewer || store.isAuditor || store.isAuditAdmin,
            close:  store.canManageCas,
        },
        reports: {
            generate: store.canViewReports,
            schedule: store.isAuditAdmin || store.isAdmin,
        },
        admin: {
            access:         store.canAccessAdminTemplates || store.isITAdmin || store.isAdmin,
            manageUsers:    store.isITAdmin || store.isAdmin,
            manageSettings: store.canAccessAdminTemplates,
        },
    }));

    function hasPermission(path: PermissionPath): boolean {
        const [group, key] = path.split('.') as [string, string];
        return Boolean((permissions.value as any)[group]?.[key]);
    }

    return { permissions, hasPermission };
}
