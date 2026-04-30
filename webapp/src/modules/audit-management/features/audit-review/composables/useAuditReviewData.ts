import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import { useAuditStore } from '@/modules/audit-management/stores/auditStore';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import { useUserStore } from '@/stores/userStore';
import type { EmailRoutingRuleDto } from '@/apiclient/auditClient';

export function useAuditReviewData() {
    const route     = useRoute();
    const store     = useAuditStore();
    const userStore = useUserStore();
    const service   = useAuditService();

    const review = computed(() => store.review);

    const repeatFindingIdSet = computed(() =>
        new Set(review.value?.repeatFindingQuestionIds ?? [])
    );

    // All routing entries — loaded once for the recipients dialog
    const allRoutingEntries = ref<EmailRoutingRuleDto[]>([]);

    onMounted(async () => {
        const id = Number(route.params.id);
        if (!isNaN(id)) await store.loadReview(id);

        if (userStore.isAuditAdmin) {
            try {
                allRoutingEntries.value = await service.getEmailRouting();
            } catch { /* non-fatal */ }
        }
    });

    return { review, reviewLoading: computed(() => store.reviewLoading), repeatFindingIdSet, allRoutingEntries };
}
