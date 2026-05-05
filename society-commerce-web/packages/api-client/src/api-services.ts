/**
 * Real API service implementations using apiFetch.
 * These call the ASP.NET backend at the configured baseUrl.
 * Each service matches the same interface as mock-services.ts.
 */
import { apiFetch } from './client.js';
import type {
	ShopResponse, ProductResponse, CartResponse,
	OrderResponse, OrderSummaryResponse, RoutineResponse, DraftOrderResponse,
	NotificationResponse, AgentResponse, DashboardResponse, PagedResponse,
} from './types.js';
import type { AuditEntry, SupportTicket, MonthlySummary, Household } from './demo-data.js';

// ── Shop Service ──
export const shopService = {
	async list(): Promise<ShopResponse[]> {
		return apiFetch<ShopResponse[]>('/shops');
	},
	async get(shopId: string): Promise<ShopResponse | null> {
		try {
			return await apiFetch<ShopResponse>(`/shops/${shopId}`);
		} catch { return null; }
	},
	async getCategories(shopId: string): Promise<string[]> {
		// Backend returns products; derive categories client-side
		const products = await apiFetch<ProductResponse[]>(`/shops/${shopId}/products`);
		return [...new Set(products.map(p => p.category))].sort();
	},
};

// ── Product Service ──
export const productService = {
	async list(shopId: string, opts?: { category?: string; search?: string }): Promise<ProductResponse[]> {
		const params: Record<string, string> = {};
		if (opts?.category) params.category = opts.category;
		if (opts?.search) params.q = opts.search;
		return apiFetch<ProductResponse[]>(`/shops/${shopId}/products`, { params });
	},
	async get(productId: string): Promise<ProductResponse | null> {
		// Backend doesn't have a single-product endpoint outside shop context.
		// This is used rarely; return null as fallback.
		return null;
	},
};

// ── Cart Service ──
export const cartService = {
	async get(shopId: string): Promise<CartResponse> {
		return apiFetch<CartResponse>(`/cart/${shopId}`);
	},
	async addItem(shopId: string, productId: string, quantity: number): Promise<CartResponse> {
		await apiFetch(`/cart/${shopId}/items`, {
			method: 'POST',
			body: JSON.stringify({ productId, quantity }),
		});
		return this.get(shopId);
	},
	async updateItem(shopId: string, productId: string, quantity: number): Promise<CartResponse> {
		await apiFetch(`/cart/${shopId}/items/${productId}`, {
			method: 'PUT',
			body: JSON.stringify({ quantity }),
		});
		return this.get(shopId);
	},
	async removeItem(shopId: string, productId: string): Promise<CartResponse> {
		await apiFetch(`/cart/${shopId}/items/${productId}`, { method: 'DELETE' });
		return this.get(shopId);
	},
	async clear(shopId: string): Promise<void> {
		await apiFetch(`/cart/${shopId}`, { method: 'DELETE' });
	},
};

// ── Order Service ──
export const orderService = {
	async place(opts: {
		shopId: string; fulfillmentType: number;
		orderNotes?: string; deliveryNotes?: string;
	}): Promise<OrderResponse> {
		return apiFetch<OrderResponse>('/orders', {
			method: 'POST',
			body: JSON.stringify(opts),
		});
	},
	async list(opts?: { page?: number; pageSize?: number; status?: number }): Promise<PagedResponse<OrderSummaryResponse>> {
		const params: Record<string, string> = {};
		if (opts?.page) params.page = String(opts.page);
		if (opts?.pageSize) params.pageSize = String(opts.pageSize);
		if (opts?.status !== undefined) params.status = String(opts.status);
		return apiFetch<PagedResponse<OrderSummaryResponse>>('/orders', { params });
	},
	async get(orderId: string): Promise<OrderResponse | null> {
		try {
			return await apiFetch<OrderResponse>(`/orders/${orderId}`);
		} catch { return null; }
	},
	async cancel(orderId: string, reason?: string): Promise<OrderResponse> {
		await apiFetch(`/orders/${orderId}/cancel`, {
			method: 'POST',
			body: JSON.stringify({ reason }),
		});
		// Backend returns message, re-fetch for full order
		return (await this.get(orderId))!;
	},

	// Seller actions
	async confirmItem(orderId: string, itemId: string, confirmedQty: number): Promise<OrderResponse> {
		await apiFetch(`/orders/${orderId}/confirm-items`, {
			method: 'POST',
			body: JSON.stringify({ items: [{ itemId, confirmedQuantity: confirmedQty }] }),
		});
		return (await this.get(orderId))!;
	},
	async rejectItem(orderId: string, itemId: string, reason: string): Promise<OrderResponse> {
		await apiFetch(`/orders/${orderId}/confirm-items`, {
			method: 'POST',
			body: JSON.stringify({ items: [{ itemId, confirmedQuantity: 0, reason }] }),
		});
		return (await this.get(orderId))!;
	},
	async updateStatus(orderId: string, status: number, extra?: { pickupCode?: string }): Promise<OrderResponse> {
		// Map target status numbers to backend action endpoints
		// For status 7 (Completed): use mark-collected for pickup, mark-delivered for delivery
		let endpoint: string | undefined;
		if (status === 7 && extra?.pickupCode) {
			endpoint = `/orders/${orderId}/mark-collected`;
		} else {
			const statusEndpoints: Record<number, string> = {
				4: `/orders/${orderId}/start-preparing`,
				5: `/orders/${orderId}/mark-ready`,
				6: `/orders/${orderId}/mark-dispatched`,
				7: `/orders/${orderId}/mark-delivered`,
			};
			endpoint = statusEndpoints[status];
		}
		if (endpoint) {
			const opts: RequestInit = { method: 'POST' };
			if (extra?.pickupCode) {
				opts.body = JSON.stringify({ pickupCode: extra.pickupCode });
			}
			await apiFetch(endpoint, opts);
		}
		return (await this.get(orderId))!;
	},
	async assignAgent(orderId: string, agentId: string): Promise<OrderResponse> {
		await apiFetch(`/orders/${orderId}/assign-delivery`, {
			method: 'POST',
			body: JSON.stringify({ agentId }),
		});
		return (await this.get(orderId))!;
	},
	async listForShop(shopId: string): Promise<OrderResponse[]> {
		// Backend /api/orders returns shop's orders when called by seller
		const result = await apiFetch<PagedResponse<OrderSummaryResponse>>('/orders', {
			params: { pageSize: '100' },
		});
		// For full order detail per item, we need individual fetches
		// For now, return summaries cast to OrderResponse with items populated via individual gets
		// This is a known trade-off — batch endpoint would be better
		const orders = await Promise.all(
			result.items.map(s => this.get(s.id))
		);
		return orders.filter((o): o is OrderResponse => o !== null);
	},
};

// ── Routine Service ──
export const routineService = {
	async list(): Promise<RoutineResponse[]> {
		return apiFetch<RoutineResponse[]>('/routines');
	},
	async get(routineId: string): Promise<RoutineResponse | null> {
		try {
			return await apiFetch<RoutineResponse>(`/routines/${routineId}`);
		} catch { return null; }
	},
	async create(data: { label: string; shopId: string; frequency: number; dayOfWeek?: number; dayOfMonth?: number; items: { productId: string; quantity: number }[] }): Promise<RoutineResponse> {
		const { id } = await apiFetch<{ id: string }>('/routines', {
			method: 'POST', body: JSON.stringify(data),
		});
		return (await this.get(id))!;
	},
	async togglePause(routineId: string): Promise<RoutineResponse> {
		// Need to know current state to decide pause vs resume
		const routine = await this.get(routineId);
		if (!routine) throw new Error('Routine not found');
		const action = routine.isPaused ? 'resume' : 'pause';
		await apiFetch(`/routines/${routineId}/${action}`, { method: 'PATCH' });
		return (await this.get(routineId))!;
	},
	async delete(routineId: string): Promise<void> {
		await apiFetch(`/routines/${routineId}`, { method: 'DELETE' });
	},
};

// ── Draft Service ──
export const draftService = {
	async list(): Promise<DraftOrderResponse[]> {
		return apiFetch<DraftOrderResponse[]>('/drafts');
	},
	async confirm(draftId: string): Promise<OrderResponse> {
		return apiFetch<OrderResponse>(`/drafts/${draftId}/place`, { method: 'POST' });
	},
	async dismiss(draftId: string): Promise<void> {
		await apiFetch(`/drafts/${draftId}/skip`, { method: 'POST' });
	},
};

// ── Notification Service ──
export const notificationService = {
	async list(): Promise<NotificationResponse[]> {
		const result = await apiFetch<{ items: NotificationResponse[]; total: number }>('/notifications', {
			params: { pageSize: '50' },
		});
		return result.items;
	},
	async markRead(notifId: string): Promise<void> {
		await apiFetch(`/notifications/${notifId}/read`, { method: 'PATCH' });
	},
	async markAllRead(): Promise<void> {
		await apiFetch('/notifications/read-all', { method: 'PATCH' });
	},
	async getUnreadCount(): Promise<number> {
		const result = await apiFetch<{ count: number }>('/notifications/unread-count');
		return result.count;
	},
};

// ── Agent Service ──
export const agentService = {
	async list(shopId: string): Promise<AgentResponse[]> {
		return apiFetch<AgentResponse[]>(`/shops/${shopId}/agents`);
	},
	async updateStatus(agentId: string, isActive: boolean): Promise<AgentResponse> {
		// Backend uses PUT on /shops/{shopId}/agents/{agentId}
		// We don't have shopId here in the mock interface — this is a mismatch.
		// For now, throw; caller should pass shopId or we adjust the interface later.
		throw new Error('agentService.updateStatus requires backend refactor — use mock for now');
	},
};

// ── Ledger / Reporting Service ──
// Backend doesn't have a direct buyer ledger endpoint — this stays mocked
export const ledgerService = {
	async getMonthlySummaries(): Promise<MonthlySummary[]> {
		throw new Error('Ledger endpoint not available in backend — use mock');
	},
	async getMonthDetail(_month: string): Promise<MonthlySummary | null> {
		throw new Error('Ledger endpoint not available in backend — use mock');
	},
};

// ── Support / Ticket Service ──
export const supportService = {
	async list(): Promise<SupportTicket[]> {
		const result = await apiFetch<{ items: SupportTicket[] }>('/tickets', {
			params: { pageSize: '50' },
		});
		return result.items;
	},
	async create(data: { type: string; subject: string; description: string; orderId?: string }): Promise<SupportTicket> {
		// Backend ticket type is numeric, frontend uses string
		const typeMap: Record<string, number> = { dispute: 1, bug: 2, general: 3 };
		const { id } = await apiFetch<{ id: string }>('/tickets', {
			method: 'POST',
			body: JSON.stringify({
				orderId: data.orderId,
				type: typeMap[data.type] ?? 3,
				description: data.description,
			}),
		});
		// Backend doesn't return full ticket on create, re-fetch
		const tickets = await this.list();
		return tickets.find(t => t.id === id) ?? { id, ...data, status: 'open', createdBy: '', createdByName: '', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() } as SupportTicket;
	},
	async updateStatus(ticketId: string, status: string): Promise<SupportTicket> {
		throw new Error('Ticket status update requires admin endpoint');
	},
};

// ── Household Service ──
export const householdService = {
	async getForUser(_userId: string): Promise<Household | null> {
		// Backend uses /api/household/members for current user
		try {
			const result = await apiFetch<{ flatNumber: string; block?: string; cap: number; members: { id: string; name: string; phone: string; role: string }[] }>('/household/members');
			return {
				id: `flat-${result.flatNumber}`,
				flatNumber: result.flatNumber,
				members: result.members.map(m => m.id),
			};
		} catch { return null; }
	},
	async getMembers(_householdId: string): Promise<{ id: string; name: string; phone: string; role: string }[]> {
		try {
			const result = await apiFetch<{ members: { id: string; name: string; phone: string; role: string }[] }>('/household/members');
			return result.members;
		} catch { return []; }
	},
};

// ── Admin Service ──
export const adminService = {
	async getDashboard(): Promise<DashboardResponse> {
		return apiFetch<DashboardResponse>('/admin/dashboard');
	},
	async getAuditLog(): Promise<AuditEntry[]> {
		const result = await apiFetch<{ items: AuditEntry[] }>('/admin/audit-log', {
			params: { pageSize: '100' },
		});
		return result.items;
	},
	async getAllUsers() {
		// No direct admin users-list endpoint in backend
		throw new Error('Admin users list not available — use mock');
	},
	async getAllShops(): Promise<ShopResponse[]> {
		return apiFetch<ShopResponse[]>('/shops');
	},
	async getAllOrders(): Promise<OrderResponse[]> {
		const result = await apiFetch<PagedResponse<OrderSummaryResponse>>('/orders', {
			params: { pageSize: '100' },
		});
		// Return summaries as-is (partial OrderResponse shape)
		return result.items as unknown as OrderResponse[];
	},
	async getAllTickets(): Promise<SupportTicket[]> {
		const result = await apiFetch<{ items: SupportTicket[] }>('/tickets', {
			params: { pageSize: '100' },
		});
		return result.items;
	},
};
