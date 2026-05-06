import { computed } from 'vue';
import { useAuditStore } from '../../../stores/auditStore';

export function useAuditAutosave() {
    const store = useAuditStore();

    const autosaveLabel = computed(() => {
        switch (store.autosaveStatus) {
            case 'saving': return 'Saving…';
            case 'saved':  return 'Saved';
            case 'failed': return 'Save failed';
            default:       return '';
        }
    });

    const autosaveIcon = computed(() => {
        switch (store.autosaveStatus) {
            case 'saving': return 'pi pi-spin pi-spinner';
            case 'saved':  return 'pi pi-check-circle';
            case 'failed': return 'pi pi-exclamation-circle';
            default:       return '';
        }
    });

    const autosaveColor = computed(() => {
        switch (store.autosaveStatus) {
            case 'saving': return 'text-slate-400';
            case 'saved':  return 'text-emerald-400';
            case 'failed': return 'text-red-400';
            default:       return '';
        }
    });

    return {
        autosaveStatus: computed(() => store.autosaveStatus),
        autosaveLabel,
        autosaveIcon,
        autosaveColor,
    };
}
