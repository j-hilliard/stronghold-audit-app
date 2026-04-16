import { computed, ref } from 'vue';
import { defineStore } from 'pinia';
import { blobToBase64 } from '@/utils.ts';
import { useApiStore } from '@/stores/apiStore';
import { AccountInfo } from '@azure/msal-browser';
import { User, UserClient } from '@/apiclient/client';
import { getUserProfilePhoto } from '@/services/graphService';
import { logout, msalInstance } from '@/services/msalService';

export const useUserStore = defineStore('user', () => {
    const user = ref<User | null>(null);
    const userPhoto = ref<string | null>(null);
    const userAccountInfo = ref<AccountInfo | null>(null);

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

    const isAdmin = computed(() => {
        return !!(user.value?.roles?.map(role => role.role?.name).includes('Administrator'));
    });

    // ── Audit-specific role helpers ───────────────────────────────────────────
    const auditRoleNames = computed(() =>
        user.value?.roles?.map(r => r.role?.name ?? '').filter(Boolean) ?? []
    );

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

    /** User has any audit-module role (controls route access). */
    const canViewAudits = computed(() =>
        isAdmin.value ||
        ['TemplateAdmin', 'AuditManager', 'AuditReviewer',
         'CorrectiveActionOwner', 'ReadOnlyViewer', 'ExecutiveViewer']
            .some(r => auditRoleNames.value.includes(r))
    );

    /** User can create new audits. */
    const canCreateAudit = computed(() => isAuditManager.value);

    /** User can manage corrective actions. */
    const canManageCas = computed(() => isAuditManager.value || isAuditReviewer.value);

    /** User can access the template admin section. */
    const canAccessAdminTemplates = computed(() => isTemplateAdmin.value);

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
        // Audit roles
        isTemplateAdmin,
        isAuditManager,
        isAuditReviewer,
        canViewAudits,
        canCreateAudit,
        canManageCas,
        canAccessAdminTemplates,
    };
});
