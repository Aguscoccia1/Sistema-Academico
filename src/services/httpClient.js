
import { expirarSesion, obtenerToken } from '../core/auth/session';

export class ApiError extends Error {
    constructor(message, status) {
        super(message);
        this.status = status;
    }
}

async function obtenerMensajeError(response) {
    const body = await response.text();
    if (!body) {
        return `La API respondió con ${response.status}.`;
    }

    try {
        const parsed = JSON.parse(body);
        if (typeof parsed === 'string') {
            return parsed;
        }

        if (parsed && typeof parsed === 'object') {
            return parsed.detail || parsed.message || parsed.title ||
                `La API respondió con ${response.status}.`;
        }
    } catch {
        return body.slice(0, 300);
    }

    return `La API respondió con ${response.status}.`;
}

export async function request(path, options = {}) {
    const token = obtenerToken();

    const response = await fetch(path, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...(token && { Authorization: `Bearer ${token}` }),
            ...options.headers,
        },
    });

    if (!response.ok) {
        if (response.status === 401 && token && path !== '/api/auth/login') {
            expirarSesion();
        }

        throw new ApiError(await obtenerMensajeError(response), response.status);
    }

    return response.status === 204
        ? undefined
        : response.json();
}
