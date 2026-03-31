import { apps } from '@/apps.ts';
import { getErrorRoutes } from '@/router/errorRoutes.ts';

export const auditManagementRoutes = [
    { path: '', redirect: `/${apps.auditManagement.baseSlug}/audits` },
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
            title: 'Reports',
            breadcrumbItems: () => [
                { label: 'Compliance Audit' },
                { label: 'Reports' },
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
    ...getErrorRoutes(apps.auditManagement),
];
