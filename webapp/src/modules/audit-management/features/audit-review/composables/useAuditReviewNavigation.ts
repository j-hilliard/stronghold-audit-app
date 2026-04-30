import { nextTick, ref } from 'vue';
import type { Ref } from 'vue';
import { useRoute } from 'vue-router';
import type { AuditReviewDto } from '@/apiclient/auditClient';

export function useAuditReviewNavigation(review: Ref<AuditReviewDto | null>) {
    const route = useRoute();

    const showFullRecord = ref(false);
    const showAiSummary  = ref(true);

    async function printPage() {
        const wasOpen        = showFullRecord.value;
        showFullRecord.value = true;
        await nextTick();
        sessionStorage.setItem('print-review-data', JSON.stringify(review.value));
        window.open(`/audit-management/print-review/${route.params.id}`, '_blank');
        showFullRecord.value = wasOpen;
    }

    return { showFullRecord, showAiSummary, printPage };
}
