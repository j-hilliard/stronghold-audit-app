/**
 * Unauthenticated API client for public-access endpoints.
 * Used by ExternalCaView — the user may not be logged in.
 * Does NOT use apiStore or inject auth headers.
 */

import axios from 'axios';
import type { CaPublicAccessDto, CorrectiveActionPhotoDto } from './auditClient';

export interface PublicCaUpdateRequest {
    newStatus:      string;
    notes?:         string | null;
    updatedByName?: string | null;
}

function getBaseUrl(): string {
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

    uploadCaPhoto(token: string, file: File, caption?: string): Promise<CorrectiveActionPhotoDto> {
        const form = new FormData();
        form.append('file', file);
        if (caption) form.append('caption', caption);
        return axios
            .post<CorrectiveActionPhotoDto>(`${this.baseUrl}/v1/public/ca/${token}/photos`, form, {
                headers: { 'Content-Type': 'multipart/form-data' },
            })
            .then(r => r.data);
    }
}
