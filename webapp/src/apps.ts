import { useUserStore } from '@/stores/userStore';

export const apps = {
    auditManagement: {
        baseSlug: 'audit-management',
        name: 'Compliance Audit',
        description: 'Create and manage field compliance audits',
        icon: 'pi pi-clipboard',
        menu: {
            user: [
                { label: 'Dashboard',          icon: 'pi pi-fw pi-chart-bar',         to: '/audit-management/reports',
                  visible: () => useUserStore().canViewAudits },
                { label: 'Audits',             icon: 'pi pi-fw pi-list',               to: '/audit-management/audits',
                  visible: () => useUserStore().canViewAudits },
                { label: 'New Audit',          icon: 'pi pi-fw pi-plus',               to: '/audit-management/audits/new',
                  visible: () => useUserStore().canCreateAudit },
                { label: 'Corrective Actions', icon: 'pi pi-fw pi-exclamation-circle', to: '/audit-management/corrective-actions',
                  visible: () => useUserStore().canManageCas },
            ],
            admin: [
                { label: 'Templates',  icon: 'pi pi-fw pi-table',   to: '/audit-management/admin/templates',
                  visible: () => useUserStore().canAccessAdminTemplates },
                { label: 'Settings',   icon: 'pi pi-fw pi-cog',     to: '/audit-management/admin/settings',
                  visible: () => useUserStore().canAccessAdminTemplates },
                { label: 'Users',      icon: 'pi pi-fw pi-users',   to: '/audit-management/admin/users',
                  visible: () => useUserStore().isITAdmin || useUserStore().isAdmin },
                { label: 'Audit Log',  icon: 'pi pi-fw pi-shield',  to: '/audit-management/admin/audit-log',
                  visible: () => useUserStore().isAuditAdmin || useUserStore().isAdmin || useUserStore().isITAdmin },
            ],
        },
    } as App,
};
