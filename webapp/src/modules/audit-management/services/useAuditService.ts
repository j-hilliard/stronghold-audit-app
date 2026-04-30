import { AuditClient } from '@/apiclient/auditClient';
import { useApiStore } from '@/stores/apiStore';

export function useAuditService(): AuditClient {
    const apiStore = useApiStore();
    return new AuditClient(apiStore.api.defaults.baseURL, apiStore.api);
}
