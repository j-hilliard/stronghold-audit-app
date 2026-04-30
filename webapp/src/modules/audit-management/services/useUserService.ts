import { useApiStore } from '@/stores/apiStore';
import { UserClient, RoleClient, UserRoleClient } from '@/apiclient/client';

export function useUserService() {
    const apiStore = useApiStore();
    const baseUrl = apiStore.api.defaults.baseURL ?? '';
    const axiosInstance = apiStore.api;

    const getUserClient     = () => new UserClient(baseUrl, axiosInstance);
    const getRoleClient     = () => new RoleClient(baseUrl, axiosInstance);
    const getUserRoleClient = () => new UserRoleClient(baseUrl, axiosInstance);

    return { getUserClient, getRoleClient, getUserRoleClient };
}
