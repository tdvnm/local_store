/**
 * Service layer switcher.
 * 
 * Exports the appropriate service implementation based on configuration.
 * When USE_REAL_API is true and a backend is reachable, uses real HTTP services.
 * Otherwise falls back to mock services.
 * 
 * Individual domains can be toggled independently via the config object.
 */
import * as mock from './mock-services.js';
import * as api from './api-services.js';
import { ApiError } from './client.js';

export interface ServiceConfig {
	/** Master switch: set to true to enable real API calls */
	useRealApi: boolean;
	/** Per-domain overrides: true = use real API, false = use mock */
	overrides?: {
		shops?: boolean;
		products?: boolean;
		cart?: boolean;
		orders?: boolean;
		routines?: boolean;
		drafts?: boolean;
		notifications?: boolean;
		agents?: boolean;
		ledger?: boolean;
		support?: boolean;
		household?: boolean;
		admin?: boolean;
	};
}

let _config: ServiceConfig = { useRealApi: false };

export function configureServices(config: ServiceConfig) {
	_config = { ..._config, ...config };
}

function shouldUseReal(domain: keyof NonNullable<ServiceConfig['overrides']>): boolean {
	return _config.overrides?.[domain] ?? _config.useRealApi;
}

function shouldFallbackToMock(err: unknown): boolean {
	if (err instanceof TypeError && err.message.includes('fetch')) return true;
	if (err instanceof ApiError) {
		return err.status === 404 || err.status === 405 || err.status === 501 || err.status === 503;
	}
	if (err instanceof Error) {
		return /use mock|not available|requires backend refactor/i.test(err.message);
	}
	return false;
}

// Each export picks real or mock based on config.
// The selection happens at call time (not import time) so config can change.

function createProxy<T extends Record<string, any>>(domain: keyof NonNullable<ServiceConfig['overrides']>, realImpl: T, mockImpl: T): T {
	return new Proxy({} as T, {
		get(_target, prop: string) {
			const impl = shouldUseReal(domain) ? realImpl : mockImpl;
			const val = impl[prop];
			if (typeof val === 'function') {
				return (...args: any[]) => {
					try {
						const result = val.apply(impl, args);
						// If it's a promise, catch API errors and fall back to mock
						if (result && typeof result.then === 'function') {
							return result.catch((err: any) => {
								if (shouldFallbackToMock(err)) {
									console.warn(`[services] ${domain}.${prop} API unreachable, falling back to mock`);
									const mockFn = mockImpl[prop];
									if (typeof mockFn === 'function') return mockFn.apply(mockImpl, args);
								}
								throw err;
							});
						}
						return result;
					} catch (err) {
						if (shouldFallbackToMock(err)) {
							console.warn(`[services] ${domain}.${prop} API unavailable, falling back to mock`);
							const mockFn = mockImpl[prop];
							if (typeof mockFn === 'function') return mockFn.apply(mockImpl, args);
						}
						throw err;
					}
				};
			}
			return val;
		},
	});
}

export const shopService = createProxy('shops', api.shopService, mock.shopService);
export const productService = createProxy('products', api.productService, mock.productService);
export const cartService = createProxy('cart', api.cartService, mock.cartService);
export const orderService = createProxy('orders', api.orderService, mock.orderService);
export const routineService = createProxy('routines', api.routineService, mock.routineService);
export const draftService = createProxy('drafts', api.draftService, mock.draftService);
export const notificationService = createProxy('notifications', api.notificationService, mock.notificationService);
export const agentService = createProxy('agents', api.agentService, mock.agentService);
export const ledgerService = createProxy('ledger', api.ledgerService, mock.ledgerService);
export const supportService = createProxy('support', api.supportService, mock.supportService);
export const householdService = createProxy('household', api.householdService, mock.householdService);
export const adminService = createProxy('admin', api.adminService, mock.adminService);
