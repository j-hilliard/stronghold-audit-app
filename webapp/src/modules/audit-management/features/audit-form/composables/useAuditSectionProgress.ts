import { computed } from 'vue';
import { useAuditStore, calculateScore } from '../../../stores/auditStore';
import type { TemplateSectionDto } from '@/apiclient/auditClient';

export interface SectionProgressItem {
    sectionId: number;
    name: string;
    isNa: boolean;
    totalQuestions: number;
    answered: number;
    conforming: number;
    nonConforming: number;
    warning: number;
    na: number;
    unanswered: number;
    scorePercent: number | null;
}

export function useAuditSectionProgress() {
    const store = useAuditStore();

    const sectionProgress = computed((): SectionProgressItem[] =>
        store.visibleSections.map((section: TemplateSectionDto) => {
            const isNa = store.sectionNaOverrides.has(section.id);

            if (isNa) {
                return {
                    sectionId: section.id,
                    name: section.name,
                    isNa: true,
                    totalQuestions: section.questions.length,
                    answered: 0,
                    conforming: 0,
                    nonConforming: 0,
                    warning: 0,
                    na: 0,
                    unanswered: 0,
                    scorePercent: null,
                };
            }

            const sectionResponses = section.questions
                .map(q => store.responses.get(q.questionId))
                .filter((r) => r != null);

            const { counts, scorePercent } = calculateScore(sectionResponses);
            const answered = sectionResponses.filter(r => r.status !== null).length;

            return {
                sectionId: section.id,
                name: section.name,
                isNa: false,
                totalQuestions: section.questions.length,
                answered,
                conforming: counts.conforming,
                nonConforming: counts.nonConforming,
                warning: counts.warning,
                na: counts.na,
                unanswered: counts.unanswered,
                scorePercent,
            };
        }),
    );

    return { sectionProgress };
}
