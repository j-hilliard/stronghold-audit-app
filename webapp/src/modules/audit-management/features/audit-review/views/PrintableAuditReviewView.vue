<template>
    <div class="print-doc" ref="docEl">
        <div v-if="loading" class="loading-msg">Loading audit data…</div>
        <AuditTableContent v-else-if="review" :review="review" />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import { useAuditService } from '@/modules/audit-management/services/useAuditService';
import AuditTableContent from '../components/AuditTableContent.vue';

const route = useRoute();
const docEl   = ref<HTMLElement | null>(null);
const loading = ref(true);
const review  = ref<any>(null);

onMounted(async () => {
    try {
        const cached = sessionStorage.getItem('print-review-data');
        if (cached) {
            review.value = JSON.parse(cached);
        } else {
            const auditId = Number(route.params.auditId);
            review.value = await useAuditService().getAuditReview(auditId);
        }
    } catch (e) {
        console.error('Failed to load audit review for print:', e);
    } finally {
        loading.value = false;
    }

    await nextTick();

    // Wait for all logo images to finish loading before printing
    const imgs = Array.from(document.querySelectorAll<HTMLImageElement>('img'));
    await Promise.all(
        imgs.map(img =>
            img.complete
                ? Promise.resolve()
                : new Promise<void>(resolve => { img.onload = () => resolve(); img.onerror = () => resolve(); })
        )
    );

    // Move into #print-root so PrimeVue's print visibility rules apply correctly
    const root = document.getElementById('print-root');
    if (root && docEl.value) {
        root.appendChild(docEl.value);
    }

    window.print();
});
</script>

<style scoped>
* {
    font-family: Arial, Helvetica, sans-serif;
    font-size: 9pt;
    box-sizing: border-box;
}

.print-doc {
    background: #fff;
    color: #000;
    padding: 0;
    margin: 0;
}

.loading-msg {
    padding: 20px;
    font-size: 12pt;
    color: #666;
}

@media screen {
    .print-doc {
        max-width: 1100px;
        margin: 0 auto;
        padding: 20px;
        background: #fff;
        box-shadow: 0 0 20px rgba(0,0,0,0.15);
    }
}

@media print {
    .print-doc {
        width: 100%;
    }
}
</style>
