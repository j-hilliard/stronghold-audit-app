/**
 * Unauthenticated API client for public-access endpoints.
 * Used by ExternalCaView — the user may not be logged in.
 * Does NOT use apiStore or inject auth headers.
 */

import axios from 'axios';
import type { CaPublicAccessDto } from './auditClient';

export interface PublicCaUpdateRequest {
    newStatus:      string;
    notes?:         string | null;
    updatedByName?: string | null;
}

function getBaseUrl(): string {
    // Falls back to same-origin (works for both dev proxy and production)
    return (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? '';
}

export class PublicCaClient {
    private baseUrl: string;

    constructor(baseUrl?: string) {
        this.baseUrl = baseUrl ?? getBaseUrl();
    }

    getCaByToken(token: string): Promise<CaPublicAccessDto> {
        return axios
            .get<CaPublicAccessDto>(`${this.baseUrl}/v1/public/ca/${token}`)
            .then(r => r.data);
    }

    updateCaByToken(token: string, payload: PublicCaUpdateRequest): Promise<void> {
        return axios
            .put(`${this.baseUrl}/v1/public/ca/${token}`, payload)
            .then(() => undefined);
    }
}
