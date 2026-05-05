/**
 * Auth store for Buyer app.
 * Uses dev API auth when backend is available, falls back to demo auth.
 */
import { browser } from '$app/environment';
import {
	getDemoSession, demoLogin, demoLogout, isBuyer,
	devApiLogin, setDevAuthBaseUrl, configure, configureServices, usesRealApiSession,
	type DemoUser
} from '@society-commerce/api-client';

const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000/api';

function syncServiceMode(session: ReturnType<typeof getDemoSession>) {
	configureServices({
		useRealApi: usesRealApiSession(session),
		overrides: { ledger: false },
	});
}

// Configure api-client on load
if (browser) {
	setDevAuthBaseUrl(API_BASE);
	configure({
		baseUrl: API_BASE,
		getToken: () => token,
		onUnauthorized: () => { auth.logout(); },
	});
	syncServiceMode(getDemoSession());
}

// ── Reactive state ──
let user = $state<DemoUser | null>(null);
let token = $state<string | null>(null);
let ready = $state(false);
let loggingIn = $state(false);

// Restore session on load
if (browser) {
	const session = getDemoSession();
	syncServiceMode(session);
	if (session && isBuyer(session.user)) {
		user = session.user;
		token = session.token;
	}
	ready = true;
}

// ── Auth object ──
export const auth = {
	get user() { return user; },
	get token() { return token; },
	get loggedIn() { return user !== null && token !== null; },
	get ready() { return ready; },
	get loggingIn() { return loggingIn; },

	/** Login via dev API (real JWT) with fallback to demo login */
	async login(identifier: string) {
		loggingIn = true;
		try {
			// Try real backend dev auth first
			const session = await devApiLogin(identifier);
			syncServiceMode(session);
			user = session.user as DemoUser;
			token = session.token;
		} catch (err) {
			// Fallback to demo login if backend unreachable
			console.warn('[auth] Dev API login failed, falling back to demo:', err);
			const session = demoLogin(identifier);
			syncServiceMode(session);
			user = session.user;
			token = session.token;
		} finally {
			loggingIn = false;
		}
	},

	logout() {
		demoLogout();
		syncServiceMode(null);
		user = null;
		token = null;
	},
};
