import { computed } from 'vue';
import type { Ref } from 'vue';
import type { AuditReviewDto } from '@/apiclient/auditClient';
import { usePermissions } from '@/modules/audit-management/composables/usePermissions';

interface WorkflowMetaOptions {
    review: Ref<AuditReviewDto | null>;
    reviewEditMode: Ref<boolean>;
}

const REOPENABLE_STATUSES = ['Submitted', 'UnderReview', 'Approved', 'Distributed', 'Closed'];
const CLOSABLE_STATUSES   = ['Approved', 'Distributed'];

export function useAuditWorkflowMeta({ review, reviewEditMode }: WorkflowMetaOptions) {
    const { hasPermission } = usePermissions();

    const canEnterReview = computed(() =>
        !!review.value &&
        hasPermission('audit.review') &&
        (review.value.status === 'Submitted' ||
            (review.value.status === 'UnderReview' && !reviewEditMode.value))
    );

    const canSaveChanges = computed(() =>
        !!review.value && hasPermission('audit.review') && reviewEditMode.value
    );

    const showExitReviewMode = computed(() => reviewEditMode.value);

    const canApprove = computed(() =>
        !!review.value && hasPermission('audit.review') && review.value.status === 'UnderReview'
    );

    const canDistribute = computed(() =>
        !!review.value &&
        hasPermission('audit.review') &&
        (review.value.status === 'Approved' || review.value.status === 'Distributed')
    );

    const isResend = computed(() => review.value?.status === 'Distributed');

    const canReopen = computed(() =>
        !!review.value &&
        hasPermission('audit.reopen') &&
        REOPENABLE_STATUSES.includes(review.value.status)
    );

    const canSubmitForReview = computed(() =>
        !!review.value &&
        review.value.status === 'Reopened' &&
        !hasPermission('audit.review')
    );

    const canClose = computed(() =>
        !!review.value &&
        hasPermission('audit.close') &&
        CLOSABLE_STATUSES.includes(review.value.status)
    );

    const showReviewBanner = computed(() =>
        reviewEditMode.value || review.value?.status === 'UnderReview'
    );

    const showEditResponsesButton = computed(() =>
        !!review.value &&
        review.value.status === 'UnderReview' &&
        hasPermission('audit.review')
    );

    return {
        canEnterReview, canSaveChanges, showExitReviewMode,
        canApprove, canDistribute, isResend,
        canReopen, canSubmitForReview, canClose,
        showReviewBanner, showEditResponsesButton,
    };
}
