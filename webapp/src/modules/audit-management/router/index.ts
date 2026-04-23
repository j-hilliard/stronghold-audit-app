import { apps } from '@/apps.ts';
import { getErrorRoutes } from '@/router/errorRoutes.ts';

export const auditManagementRoutes = [
    { path: '', redirect: `/${apps.auditManagement.baseSlug}/reports` },
    {
        path: 'audits',
        name: 'audit-management-dashboard',
        component: () => import('@/modules/audit-management/features/audit-dashboard/views/AuditDashboardView.vue'),
        meta: {
            title: 'Audits',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Audits' },
            ],
        },
    },
    {
        path: 'audits/new',
        name: 'audit-management-new',
        component: () => import('@/modules/audit-management/features/new-audit/views/NewAuditView.vue'),
        meta: {
            title: 'New Audit',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Audits', to: '/audit-management/audits' },
                { label: 'New Audit' },
            ],
        },
    },
    {
        path: 'audits/:id',
        name: 'audit-management-form',
        component: () => import('@/modules/audit-management/features/audit-form/views/AuditFormView.vue'),
        meta: {
            title: 'Audit Form',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Audits', to: '/audit-management/audits' },
                { label: 'Audit Form' },
            ],
        },
    },
    {
        path: 'audits/:id/review',
        name: 'audit-management-review',
        component: () => import('@/modules/audit-management/features/audit-review/views/AuditReviewView.vue'),
        meta: {
            title: 'Audit Review',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Audits', to: '/audit-management/audits' },
                { label: 'Review' },
            ],
        },
    },
    {
        path: 'reports',
        name: 'audit-management-reports',
        component: () => import('@/modules/audit-management/features/reports/views/ReportsView.vue'),
        meta: {
            title: 'Dashboard',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Dashboard' },
            ],
        },
    },
    {
        path: 'corrective-actions',
        name: 'audit-management-corrective-actions',
        component: () => import('@/modules/audit-management/features/corrective-actions/views/CorrectiveActionsView.vue'),
        meta: {
            title: 'Corrective Actions',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Corrective Actions' },
            ],
        },
    },
    {
        path: 'admin/templates',
        name: 'audit-management-templates',
        component: () => import('@/modules/audit-management/features/template-manager/views/TemplateManagerView.vue'),
        meta: {
            title: 'Template Manager',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Admin' },
                { label: 'Templates' },
            ],
        },
    },
    {
        path: 'admin/settings',
        name: 'audit-management-settings',
        component: () => import('@/modules/audit-management/features/admin-settings/views/AuditSettingsView.vue'),
        meta: {
            title: 'Email Routing',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Admin' },
                { label: 'Email Routing' },
            ],
        },
    },
    {
        path: 'admin/users',
        name: 'audit-management-admin-users',
        component: () => import('@/modules/audit-management/features/admin-users/views/AdminUsersView.vue'),
        meta: {
            title: 'User Management',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Admin' },
                { label: 'User Management' },
            ],
        },
    },
    {
        path: 'admin/audit-log',
        name: 'audit-management-admin-audit-log',
        component: () => import('@/modules/audit-management/features/admin-audit-log/views/AdminAuditLogView.vue'),
        meta: {
            title: 'Audit Log',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Admin' },
                { label: 'Audit Log' },
            ],
        },
    },
    {
        path: 'newsletter/template-editor',
        name: 'audit-management-newsletter-template-editor',
        component: () => import('@/modules/audit-management/features/reports/views/NewsletterTemplateEditorView.vue'),
        meta: {
            title: 'Newsletter Template Editor',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Dashboard', to: '/audit-management/reports' },
                { label: 'Newsletter Template Editor' },
            ],
        },
    },
    {
        path: 'reports/composer',
        name: 'audit-management-report-composer',
        component: () => import('@/modules/audit-management/features/reports/views/ReportComposerView.vue'),
        meta: {
            title: 'Report Composer',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Dashboard', to: '/audit-management/reports' },
                { label: 'Report Composer' },
            ],
        },
    },
    {
        path: 'reports/gallery',
        name: 'audit-management-report-gallery',
        component: () => import('@/modules/audit-management/features/reports/views/ReportGalleryView.vue'),
        meta: {
            title: 'Generate Report',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Dashboard', to: '/audit-management/reports' },
                { label: 'Generate Report' },
            ],
        },
    },
    {
        path: 'reports/scheduled',
        name: 'audit-management-report-scheduled',
        component: () => import('@/modules/audit-management/features/reports/views/ScheduledReportsView.vue'),
        meta: {
            title: 'Scheduled Reports',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Dashboard', to: '/audit-management/reports' },
                { label: 'Scheduled Reports' },
            ],
        },
    },
    {
        path: 'reports/by-employee',
        name: 'audit-management-report-by-employee',
        component: () => import('@/modules/audit-management/features/reports/views/AuditsByEmployeeView.vue'),
        meta: {
            title: 'Audits by Employee',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Dashboard', to: '/audit-management/reports' },
                { label: 'Audits by Employee' },
            ],
        },
    },
    ...getErrorRoutes(apps.auditManagement),
];
