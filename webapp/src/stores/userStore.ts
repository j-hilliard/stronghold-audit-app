import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { blobToBase64 } from '@/utils.ts';
import { useApiStore } from '@/stores/apiStore';
import { AccountInfo } from '@azure/msal-browser';
import { User, UserClient } from '@/apiclient/client';
import { getUserProfilePhoto } from '@/services/graphService';
import { logout, msalInstance } from '@/services/msalService';

const DEV_ROLE_KEY = 'stronghold-audit-dev-role';

export const useUserStore = defineStore('user', () => {
    const user = ref<User | null>(null);
    const userPhoto = ref<string | null>(null);
    const userAccountInfo = ref<AccountInfo | null>(null);

    // Captured once at store init; page reloads on every role change so this is always fresh.
    const devRoleOverride: string | null = localStorage.getItem(DEV_ROLE_KEY);

    const apiStore = useApiStore();

    const userTitle = computed(() => {
        return user.value?.title || '';
    });

    const isAuthenticated = computed(() => {
        return !!user.value;
    });

    const apiClient = computed(() => {
        return new UserClient(apiStore.api.defaults.baseURL, apiStore.api);
    });

    const userFullName = computed(() => {
        return `${user.value?.firstName || ''} ${user.value?.lastName || ''}`;
    });

    // Suppressed when a dev role override is active — override is never 'Administrator'.
    const isAdmin = computed(() => {
        if (devRoleOverride) return false;
        return !!(user.value?.roles?.map(role => role.role?.name).includes('Administrator'));
    });

    // ── Audit-specific role helpers ───────────────────────────────────────────
    // When dev role override is active, restrict to only that role for nav/UI gating.
    const auditRoleNames = computed(() => {
        if (devRoleOverride) return [devRoleOverride];
        return user.value?.roles?.map(r => r.role?.name ?? '').filter(Boolean) ?? [];
    });

    const hasAuditRole = (role: string) =>
        auditRoleNames.value.includes(role) || isAdmin.value;

    /** User is a TemplateAdmin (or system Administrator). */
    const isTemplateAdmin = computed(() =>
        hasAuditRole('TemplateAdmin')
    );

    /** User can create, save, and submit audits. */
    const isAuditManager = computed(() =>
        hasAuditRole('AuditManager') || isTemplateAdmin.value
    );

    /** User can review and close audits + manage CAs. */
    const isAuditReviewer = computed(() =>
        hasAuditRole('AuditReviewer') || isTemplateAdmin.value
    );

    // ── Official user-facing roles ────────────────────────────────────────────
    const isITAdmin    = computed(() => hasAuditRole('ITAdmin'));
    const isAuditor    = computed(() => hasAuditRole('Auditor'));
    const isAuditAdmin = computed(() => hasAuditRole('AuditAdmin') || isTemplateAdmin.value || isAdmin.value);
    const isExecutive  = computed(() => hasAuditRole('Executive') || hasAuditRole('ExecutiveViewer'));
    const isNormalUser = computed(() => hasAuditRole('NormalUser') || hasAuditRole('CorrectiveActionOwner'));

    /** User has any audit-module role (controls route access). */
    const canViewAudits = computed(() =>
        isAdmin.value || isAuditAdmin.value || isAuditor.value || isExecutive.value ||
        ['TemplateAdmin', 'AuditManager', 'AuditReviewer', 'ReadOnlyViewer']
            .some(r => auditRoleNames.value.includes(r))
    );

    /** User can create new audits. */
    const canCreateAudit = computed(() => isAuditManager.value || isAuditor.value || isAuditAdmin.value);

    /** User can view/manage corrective actions (Executive is read-only; backend enforces write access). */
    const canManageCas = computed(() =>
        isAuditManager.value || isAuditReviewer.value || isAuditor.value || isAuditAdmin.value || isNormalUser.value || isExecutive.value
    );

    /** User can access the template/settings admin section. */
    const canAccessAdminTemplates = computed(() => isTemplateAdmin.value || isAuditAdmin.value);

    /** User can access the reports and dashboard section. */
    const canViewReports = computed(() =>
        isAuditAdmin.value || isExecutive.value || isAuditManager.value || isAuditReviewer.value || isAdmin.value
    );

    async function logoutUser() {
        await logout();
        await setUser(null);
    }

    async function setUser(accountInfo: AccountInfo | null) {
        userAccountInfo.value = accountInfo;

        await getUserPhoto(accountInfo);
        await getUser(accountInfo?.idTokenClaims?.oid || '');
    }

    async function getUser(azureAdObjectId: string) {
        if (!azureAdObjectId) {
            return;
        }

        user.value = await apiClient.value.getUserByAzureAdObjectId(azureAdObjectId).catch(() => null);
    }

    function setMockUser() {
        user.value = {
            firstName: 'Joseph',
            lastName: 'Hilliard',
            email: 'joseph.hilliard@quantaservices.com',
            roles: [{ role: { name: 'Administrator' } }],
        } as User;
    }

    async function getUserPhoto(account: AccountInfo | null) {
        if (!account) {
            return;
        }

        try {
            const tokenResponse = await msalInstance.acquireTokenSilent({ account, scopes: ['User.Read'] });
            const accessToken = tokenResponse?.accessToken || '';
            const response = await getUserProfilePhoto(accessToken);

            userPhoto.value = (await blobToBase64(response))?.toString() || null;
        } catch {
            userPhoto.value = null;
        }
    }

    return {
        user,
        isAdmin,
        userPhoto,
        apiClient,
        userTitle,
        userFullName,
        isAuthenticated,
        userAccountInfo,
        setUser,
        setMockUser,
        logoutUser,
        // Audit roles — legacy
        isTemplateAdmin,
        isAuditManager,
        isAuditReviewer,
        // Audit roles — official
        isITAdmin,
        isAuditor,
        isAuditAdmin,
        isExecutive,
        isNormalUser,
        // Feature flags
        canViewAudits,
        canCreateAudit,
        canManageCas,
        canAccessAdminTemplates,
        canViewReports,
    };
});
