/**
 * Demo authentication module.
 * Provides a role-based demo login system that bypasses OTP.
 * Easy to replace later with real auth.
 */
import type { UserInfo } from './types.js';
import { DEMO_USERS, type DemoUser } from './demo-data.js';

const STORAGE_KEY = 'sc_demo_session';

export interface DemoSession {
	user: DemoUser;
	token: string;
	loginAt: string;
	authMode?: 'demo' | 'api';
}

export function usesRealApiSession(session: DemoSession | null): boolean {
	if (!session) return false;
	if (session.authMode) return session.authMode === 'api';
	return !session.token.startsWith('demo-token-');
}

/** Get current demo session from localStorage */
export function getDemoSession(): DemoSession | null {
	if (typeof window === 'undefined') return null;
	try {
		const raw = localStorage.getItem(STORAGE_KEY);
		if (!raw) return null;
		return JSON.parse(raw);
	} catch {
		return null;
	}
}

/** Login as a demo user by ID */
export function demoLogin(userId: string): DemoSession {
	const user = DEMO_USERS.find(u => u.id === userId);
	if (!user) throw new Error(`Demo user not found: ${userId}`);
	const session: DemoSession = {
		user,
		token: `demo-token-${user.id}-${Date.now()}`,
		loginAt: new Date().toISOString(),
		authMode: 'demo',
	};
	if (typeof window !== 'undefined') {
		localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
	}
	return session;
}

/** Login as a demo user by role (picks first matching user) */
export function demoLoginByRole(role: string): DemoSession {
	const user = DEMO_USERS.find(u => u.roles.includes(role));
	if (!user) throw new Error(`No demo user with role: ${role}`);
	return demoLogin(user.id);
}

/** Logout / clear demo session */
export function demoLogout(): void {
	if (typeof window !== 'undefined') {
		localStorage.removeItem(STORAGE_KEY);
	}
}

/** Check if user has a specific role */
export function hasRole(user: UserInfo | DemoUser | null, role: string): boolean {
	return user?.roles?.includes(role) ?? false;
}

/** Check if user is any kind of buyer */
export function isBuyer(user: UserInfo | DemoUser | null): boolean {
	return hasRole(user, 'flat_owner') || hasRole(user, 'household_member');
}

/** Check if user is any kind of seller */
export function isSeller(user: UserInfo | DemoUser | null): boolean {
	return hasRole(user, 'seller_owner') || hasRole(user, 'seller_manager');
}

/** Check if user is admin */
export function isAdmin(user: UserInfo | DemoUser | null): boolean {
	return hasRole(user, 'admin');
}

/** Check if user is delivery agent */
export function isAgent(user: UserInfo | DemoUser | null): boolean {
	return hasRole(user, 'delivery_agent');
}

/** Get demo users by role for the login selector */
export function getDemoUsersByRole(role: string): DemoUser[] {
	return DEMO_USERS.filter(u => u.roles.includes(role));
}

/** Get all available demo accounts grouped by role */
export function getDemoAccountGroups(): { role: string; label: string; users: DemoUser[] }[] {
	return [
		{ role: 'flat_owner', label: 'Buyer (Flat Owner)', users: getDemoUsersByRole('flat_owner') },
		{ role: 'household_member', label: 'Buyer (Household Member)', users: getDemoUsersByRole('household_member') },
		{ role: 'seller_owner', label: 'Seller (Shop Owner)', users: getDemoUsersByRole('seller_owner') },
		{ role: 'seller_manager', label: 'Seller (Manager)', users: getDemoUsersByRole('seller_manager') },
		{ role: 'delivery_agent', label: 'Delivery Agent', users: getDemoUsersByRole('delivery_agent') },
		{ role: 'admin', label: 'Admin', users: getDemoUsersByRole('admin') },
	];
}
