/**
 * Development API authentication.
 * Calls the backend's dev-only login endpoint to get a real JWT.
 * Stores the session in the same localStorage format as demo-auth
 * so existing auth stores work without changes to their read logic.
 */
import type { DemoSession } from './demo-auth.js';

const STORAGE_KEY = 'sc_demo_session';

/** Base URL for API calls — must be configured before use */
let _apiBase = 'http://localhost:5000/api';

export function setDevAuthBaseUrl(url: string) {
	_apiBase = url;
}

export interface DevLoginResponse {
	accessToken: string;
	user: {
		id: string;
		name: string;
		phone: string;
		preferredLanguage: string;
		roles: string[];
		flatId: string | null;
		shopId: string | null;
	};
}

/**
 * Login via the backend dev auth endpoint.
 * Accepts phone number or user ID.
 * Returns a DemoSession with a real JWT as the token.
 */
export async function devApiLogin(identifier: string): Promise<DemoSession> {
	const body: Record<string, string> = {};

	// If it looks like a UUID, use userId; otherwise treat as phone
	if (/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(identifier)) {
		body.userId = identifier;
	} else {
		body.phone = identifier;
	}

	const res = await fetch(`${_apiBase}/auth/dev/login`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(body),
		credentials: 'include', // Accept refresh token cookie
	});

	if (!res.ok) {
		const err = await res.json().catch(() => ({ error: 'Dev login failed' }));
		throw new Error(err.error ?? `Dev login failed: ${res.status}`);
	}

	const data: DevLoginResponse = await res.json();

	const session: DemoSession = {
		user: {
			id: data.user.id,
			name: data.user.name,
			phone: data.user.phone,
			preferredLanguage: data.user.preferredLanguage,
			roles: data.user.roles,
			flatId: data.user.flatId ?? undefined,
			shopId: data.user.shopId ?? undefined,
		} as any,
		token: data.accessToken,
		loginAt: new Date().toISOString(),
		authMode: 'api',
	};

	if (typeof window !== 'undefined') {
		localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
	}

	return session;
}

/**
 * List available dev users from the backend.
 */
export async function listDevUsers(): Promise<Array<{ id: string; name: string; phone: string; roles: string[] }>> {
	const res = await fetch(`${_apiBase}/auth/dev/users`, { credentials: 'include' });
	if (!res.ok) throw new Error('Failed to list dev users');
	return res.json();
}

/**
 * Check if the dev auth endpoint is reachable.
 */
export async function isDevAuthAvailable(): Promise<boolean> {
	try {
		const res = await fetch(`${_apiBase}/auth/dev/users`, {
			method: 'GET',
			signal: AbortSignal.timeout(2000),
		});
		return res.ok;
	} catch {
		return false;
	}
}
