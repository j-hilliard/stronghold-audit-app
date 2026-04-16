import { apps } from '@/apps.ts';
import { useAppStore } from '@/stores/appStore.ts';
import { useUserStore } from '@/stores/userStore.ts';
import { createRouter, createWebHistory } from 'vue-router';
import { billingPacketRoutes } from '@/modules/billing-packet-request-system/router';
import { strongholdBizAppsSuiteRoutes } from '@/modules/stronghold-biz-apps-suite/router';
import { projectManagementSystemRoutes } from '@/modules/project-management-system/router';
import { incidentManagementRoutes } from '@/modules/incident-management/router';
import { auditManagementRoutes } from '@/modules/audit-management/router';
import NProgress from 'nprogress';
import 'nprogress/nprogress.css';

const routes = [
    {
        path: `/${apps.billingPacketRequestSystem.baseSlug}`,
        component: () => import('@/layout/AppLayout.vue'),
        children: billingPacketRoutes,
    },
    {
        path: `/${apps.strongholdBizAppsSuite.baseSlug}`,
        component: () => import('@/layout/AppLayout.vue'),
        children: strongholdBizAppsSuiteRoutes,
    },
    {
        path: `/${apps.projectManagementSystem.baseSlug}`,
        component: () => import('@/layout/AppLayout.vue'),
        children: projectManagementSystemRoutes,
    },
    {
        path: `/${apps.incidentManagement.baseSlug}`,
        component: () => import('@/layout/AppLayout.vue'),
        children: incidentManagementRoutes,
    },
    {
        path: `/${apps.auditManagement.baseSlug}`,
        component: () => import('@/layout/AppLayout.vue'),
        children: auditManagementRoutes,
    },
    {
        // Newsletter - standalone print/report view
        path: '/audit-management/newsletter',
        name: 'audit-management-newsletter',
        component: () => import('@/modules/audit-management/features/reports/views/NewsletterView.vue'),
    },
    {
        // Quarterly summary — standalone print view
        path: '/audit-management/reports/quarterly-summary',
        name: 'audit-management-quarterly-summary',
        component: () => import('@/modules/audit-management/features/reports/views/QuarterlySummaryView.vue'),
    },
    {
        // Print view — no AppLayout wrapper, renders standalone for window.print()
        path: '/audit-management/print/:divisionId',
        name: 'audit-management-print',
        component: () => import('@/modules/audit-management/features/audit-form/views/PrintableAuditFormView.vue'),
    },
    {
        path: '/authentication/login-callback',
        name: 'authentication-login-callback',
        component: () => import('@/views/auth/AuthRedirectView.vue'),
    },
];

const router = createRouter({
    routes,
    history: createWebHistory(),
    scrollBehavior() {
        return { left: 0, top: 0 };
    },
});

router.beforeEach((to, from, next) => {
    NProgress.start();

    // Trim trailing slashes
    if (to.path !== '/' && to.path.endsWith('/')) {
        next({ path: to.path.slice(0, -1), query: to.query, hash: to.hash });
        return;
    }

    // Audit module route guards
    if (to.path.startsWith('/audit-management')) {
        const userStore = useUserStore();

        // Only check if user data is loaded (skip during initial auth hydration)
        if (userStore.isAuthenticated) {
            const isAdminRoute = to.path.includes('/admin/');

            if (isAdminRoute && !userStore.canAccessAdminTemplates) {
                next({ path: '/audit-management/audits' });
                return;
            }

            if (to.path.includes('/audits/new') && !userStore.canCreateAudit) {
                next({ path: '/audit-management/audits' });
                return;
            }
        }
    }

    next();
});

router.afterEach((to) => {
    NProgress.done();
    const appStore = useAppStore();
    const routeTitle = to.meta.title ? `${to.meta.title} - ` : '';
    const appName = appStore.currentApp.name || 'The Stronghold Companies';
    document.title = routeTitle + appName;
});

router.onError(() => {
    NProgress.done();
});

export default router;
