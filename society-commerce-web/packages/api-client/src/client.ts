export interface ApiOptions {
	baseUrl?: string;
	getToken?: () => string | null;
	onUnauthorized?: () => void;
}

let _options: ApiOptions = {};

export function configure(options: ApiOptions) {
	_options = { ..._options, ...options };
}

export class ApiError extends Error {
	constructor(
		public status: number,
		public body: unknown,
		message?: string
	) {
		super(message ?? `API error ${status}`);
	}
}

export async function apiFetch<T = unknown>(
	path: string,
	init?: RequestInit & { params?: Record<string, string> }
): Promise<T> {
	const base = _options.baseUrl ?? '/api';
	let url = `${base}${path}`;

	if (init?.params) {
		const search = new URLSearchParams(init.params);
		url += `?${search}`;
		delete init.params;
	}

	const headers = new Headers(init?.headers);
	const token = _options.getToken?.();
	if (token) headers.set('Authorization', `Bearer ${token}`);
	if (init?.body && !headers.has('Content-Type')) {
		headers.set('Content-Type', 'application/json');
	}

	const res = await fetch(url, { ...init, headers, credentials: 'include' });

	if (res.status === 401) {
		_options.onUnauthorized?.();
		throw new ApiError(401, null, 'Unauthorized');
	}

	if (!res.ok) {
		const body = await res.json().catch(() => null);
		throw new ApiError(res.status, body, body?.error ?? `API error ${res.status}`);
	}

	if (res.status === 204) return undefined as T;
	const ct = res.headers.get('content-type') ?? '';
	if (ct.includes('application/json')) return res.json();
	// Non-JSON success — return text wrapped as unknown
	const text = await res.text();
	try { return JSON.parse(text); } catch { return text as T; }
}
