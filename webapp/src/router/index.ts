import { useUserStore } from '@/stores/userStore.ts';
import { auditManagementRoutes } from '@/modules/audit-management/router';
import { createRouter, createWebHistory } from 'vue-router';
import NProgress from 'nprogress';
import 'nprogress/nprogress.css';

const routes = [
    {
        path: '/',
        redirect: '/audit-management',
    },
    {
        path: '/audit-management',
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
        // Audit review print view — standalone, no AppLayout, professional PDF output
        path: '/audit-management/print-review/:auditId',
        name: 'audit-management-print-review',
        component: () => import('@/modules/audit-management/features/audit-review/views/PrintableAuditReviewView.vue'),
    },
    {
        // External CA access — no auth required, token is in the URL
        path: '/ca/:token',
        name: 'external-ca',
        component: () => import('@/modules/audit-management/features/corrective-actions/views/ExternalCaView.vue'),
    },
    {
        path: '/authentication/login-callback',
        name: 'authentication-login-callback',
        component: () => import('@/views/auth/AuthRedirectView.vue'),
    },
    {
        path: '/:pathMatch(.*)*',
        redirect: '/audit-management',
    },
];

const router = createRouter({
    routes,
    history: createWebHistory(),
    scrollBehavior() {
        return { left: 0, top: 0 };
    },
});

router.beforeEach((to, _from, next) => {
    NProgress.start();

    // Trim trailing slashes
    if (to.path !== '/' && to.path.endsWith('/')) {
        next({ path: to.path.slice(0, -1), query: to.query, hash: to.hash });
        return;
    }

    // Audit module route guards
    if (to.path.startsWith('/audit-management')) {
        const userStore = useUserStore();

        if (userStore.isAuthenticated) {
            const meta         = to.meta as Record<string, unknown>;
            const path         = to.path;
            const unauthorized = '/audit-management/unauthorized';

            if (meta.requiresITAdmin && !userStore.isITAdmin && !userStore.isAdmin) {
                next({ path: unauthorized });
                return;
            }

            if (meta.requiresAuditAdmin && !userStore.canAccessAdminTemplates && !userStore.isITAdmin && !userStore.isAdmin) {
                next({ path: unauthorized });
                return;
            }

            if (meta.requiresCreateAudit && !userStore.canCreateAudit) {
                next({ path: unauthorized });
                return;
            }

            const isReviewRoute = /^\/audit-management\/audits\/[^/]+\/review(\/|$)/.test(path);
            if (isReviewRoute && !userStore.isAuditReviewer && !userStore.isAuditAdmin) {
                next({ path: unauthorized });
                return;
            }

            const isAuditRoute = /^\/audit-management\/audits(\/|$)/.test(path);
            if (isAuditRoute && !userStore.canViewAudits) {
                next({ path: unauthorized });
                return;
            }

            if (path.includes('/reports') && !userStore.canViewReports) {
                next({ path: unauthorized });
                return;
            }

            if (path.includes('/corrective-actions') && !userStore.canManageCas) {
                next({ path: unauthorized });
                return;
            }
        }
    }

    next();
});

router.afterEach((to) => {
    NProgress.done();
    document.title = to.meta.title ? `${to.meta.title} - Compliance Audit` : 'Compliance Audit';
});

router.onError(() => {
    NProgress.done();
});

export default router;
