/**
 * Mock service implementations.
 * These match the intended API contracts and can be swapped for real HTTP adapters later.
 */
import type {
	ShopResponse, ProductResponse, CartResponse, CartItemResponse,
	OrderResponse, OrderSummaryResponse, RoutineResponse, DraftOrderResponse,
	NotificationResponse, AgentResponse, DashboardResponse, PagedResponse
} from './types.js';
import {
	DEMO_SHOPS, DEMO_PRODUCTS, DEMO_ORDERS, DEMO_ROUTINES, DEMO_DRAFTS,
	DEMO_NOTIFICATIONS, DEMO_AGENTS, DEMO_DASHBOARD, DEMO_CATEGORIES,
	DEMO_LEDGER, DEMO_AUDIT_LOG, DEMO_TICKETS, DEMO_HOUSEHOLDS,
	getProductsForShop, PRODUCT_SHOP_MAP,
	type AuditEntry, type SupportTicket, type MonthlySummary, type Household
} from './demo-data.js';
import { getDemoSession } from './demo-auth.js';

// ── Simulated delay ──
const delay = (ms = 200) => new Promise(r => setTimeout(r, ms + Math.random() * 100));

// ── Local mutable state (simulates server state in memory) ──
let orders = [...DEMO_ORDERS];
let notifications = [...DEMO_NOTIFICATIONS];
let routines = [...DEMO_ROUTINES];
let drafts = [...DEMO_DRAFTS];
let tickets = [...DEMO_TICKETS];
let agents = [...DEMO_AGENTS];

// Cart state (per user, per shop)
const carts = new Map<string, CartItemResponse[]>();

function getCartKey(userId: string, shopId: string) { return `${userId}:${shopId}`; }

function getCurrentUserId(): string {
	return getDemoSession()?.user.id ?? 'usr-buyer-001';
}

// ── Shop Service ──
export const shopService = {
	async list(): Promise<ShopResponse[]> {
		await delay();
		return DEMO_SHOPS.filter(s => s.isActive);
	},
	async get(shopId: string): Promise<ShopResponse | null> {
		await delay();
		return DEMO_SHOPS.find(s => s.id === shopId) ?? null;
	},
	async getCategories(shopId: string): Promise<string[]> {
		await delay(100);
		return DEMO_CATEGORIES[shopId] ?? [];
	},
};

// ── Product Service ──
export const productService = {
	async list(shopId: string, opts?: { category?: string; search?: string }): Promise<ProductResponse[]> {
		await delay();
		let products = getProductsForShop(shopId);
		if (opts?.category) products = products.filter(p => p.category === opts.category);
		if (opts?.search) {
			const q = opts.search.toLowerCase();
			products = products.filter(p => p.name.toLowerCase().includes(q) || p.category.toLowerCase().includes(q));
		}
		return products;
	},
	async get(productId: string): Promise<ProductResponse | null> {
		await delay(100);
		return DEMO_PRODUCTS.find(p => p.id === productId) ?? null;
	},
};

// ── Cart Service ──
export const cartService = {
	async get(shopId: string): Promise<CartResponse> {
		await delay(100);
		const key = getCartKey(getCurrentUserId(), shopId);
		const items = carts.get(key) ?? [];
		const shop = DEMO_SHOPS.find(s => s.id === shopId);
		return {
			shopId, shopName: shop?.name ?? '',
			items: [...items],
			totalPaise: items.reduce((sum, i) => sum + i.pricePaise * i.quantity, 0),
		};
	},
	async addItem(shopId: string, productId: string, quantity: number): Promise<CartResponse> {
		await delay(100);
		const key = getCartKey(getCurrentUserId(), shopId);
		const items = carts.get(key) ?? [];
		const existing = items.find(i => i.productId === productId);
		if (existing) {
			existing.quantity += quantity;
		} else {
			const product = DEMO_PRODUCTS.find(p => p.id === productId);
			if (product) {
				items.push({
					productId: product.id, productName: product.name,
					pricePaise: product.pricePaise, quantity,
					inventoryType: product.inventoryType, isAvailable: product.isAvailable,
				});
			}
		}
		carts.set(key, items);
		return this.get(shopId);
	},
	async updateItem(shopId: string, productId: string, quantity: number): Promise<CartResponse> {
		await delay(100);
		const key = getCartKey(getCurrentUserId(), shopId);
		const items = carts.get(key) ?? [];
		const item = items.find(i => i.productId === productId);
		if (item) item.quantity = quantity;
		carts.set(key, items);
		return this.get(shopId);
	},
	async removeItem(shopId: string, productId: string): Promise<CartResponse> {
		await delay(100);
		const key = getCartKey(getCurrentUserId(), shopId);
		const items = (carts.get(key) ?? []).filter(i => i.productId !== productId);
		carts.set(key, items);
		return this.get(shopId);
	},
	async clear(shopId: string): Promise<void> {
		const key = getCartKey(getCurrentUserId(), shopId);
		carts.delete(key);
	},
};

// ── Order Service ──
let orderCounter = 1005;

export const orderService = {
	async place(opts: {
		shopId: string; fulfillmentType: number;
		orderNotes?: string; deliveryNotes?: string;
	}): Promise<OrderResponse> {
		await delay(300);
		const userId = getCurrentUserId();
		const key = getCartKey(userId, opts.shopId);
		const items = carts.get(key) ?? [];
		if (items.length === 0) throw new Error('Cart is empty');

		const shop = DEMO_SHOPS.find(s => s.id === opts.shopId);
		orderCounter++;
		const hasAbundant = items.some(i => i.inventoryType === 2);

		const order: OrderResponse = {
			id: `ord-gen-${orderCounter}`,
			orderNumber: `ORD-${orderCounter}`,
			shopId: opts.shopId,
			shopName: shop?.name ?? '',
			status: hasAbundant ? 1 : 2, // 1=pending_confirmation if abundant items, 2=confirmed if all finite
			fulfillmentType: opts.fulfillmentType,
			pickupCode: opts.fulfillmentType === 1 ? String(Math.floor(1000 + Math.random() * 9000)) : undefined,
			subtotalPaise: items.reduce((sum, i) => sum + i.pricePaise * i.quantity, 0),
			confirmedTotalPaise: hasAbundant ? undefined : items.reduce((sum, i) => sum + i.pricePaise * i.quantity, 0),
			orderNotes: opts.orderNotes,
			deliveryNotes: opts.deliveryNotes,
			confirmationExpiresAt: hasAbundant ? new Date(Date.now() + 5 * 60 * 1000).toISOString() : undefined,
			createdAt: new Date().toISOString(),
			items: items.map((ci, idx) => ({
				id: `oi-gen-${orderCounter}-${idx}`,
				productId: ci.productId,
				productName: ci.productName,
				unitPricePaise: ci.pricePaise,
				requestedQuantity: ci.quantity,
				confirmedQuantity: ci.inventoryType === 1 ? ci.quantity : undefined,
				itemStatus: ci.inventoryType === 1 ? 3 : 0, // finite items auto-confirmed, abundant pending
			})),
		};

		orders = [order, ...orders];
		carts.delete(key);

		// Add notification
		notifications = [{
			id: `notif-gen-${Date.now()}`, type: 'order_placed',
			titleKey: 'notification.order_placed', bodyKey: 'notification.order_placed_body',
			params: JSON.stringify({ orderNumber: order.orderNumber }),
			data: JSON.stringify({ orderId: order.id }),
			isRead: false, createdAt: new Date().toISOString(),
		}, ...notifications];

		return order;
	},

	async list(opts?: { page?: number; pageSize?: number; status?: number; userId?: string }): Promise<PagedResponse<OrderSummaryResponse>> {
		await delay();
		const page = opts?.page ?? 1;
		const pageSize = opts?.pageSize ?? 20;
		let filtered = [...orders];
		if (opts?.status !== undefined) filtered = filtered.filter(o => o.status === opts.status);

		const summaries: OrderSummaryResponse[] = filtered.map(o => ({
			id: o.id, orderNumber: o.orderNumber, shopName: o.shopName,
			status: o.status, fulfillmentType: o.fulfillmentType,
			confirmedTotalPaise: o.confirmedTotalPaise, subtotalPaise: o.subtotalPaise,
			itemCount: o.items.length, createdAt: o.createdAt,
		}));

		const start = (page - 1) * pageSize;
		return { items: summaries.slice(start, start + pageSize), totalCount: summaries.length, page, pageSize };
	},

	async get(orderId: string): Promise<OrderResponse | null> {
		await delay();
		return orders.find(o => o.id === orderId) ?? null;
	},

	async cancel(orderId: string, reason?: string): Promise<OrderResponse> {
		await delay();
		const order = orders.find(o => o.id === orderId);
		if (!order) throw new Error('Order not found');
		order.status = 8; // cancelled
		return order;
	},

	// Seller actions
	async confirmItem(orderId: string, itemId: string, confirmedQty: number): Promise<OrderResponse> {
		await delay(100);
		const order = orders.find(o => o.id === orderId);
		if (!order) throw new Error('Order not found');
		const item = order.items.find(i => i.id === itemId);
		if (!item) throw new Error('Item not found');
		item.confirmedQuantity = confirmedQty;
		item.itemStatus = confirmedQty > 0 ? 3 : 4; // 3=confirmed, 4=rejected
		if (confirmedQty > 0 && confirmedQty < item.requestedQuantity) {
			item.itemStatus = 5; // partial
		}

		// Recalculate confirmed total
		const allConfirmed = order.items.every(i => i.itemStatus >= 3);
		if (allConfirmed) {
			order.confirmedTotalPaise = order.items.reduce((sum, i) => sum + (i.confirmedQuantity ?? 0) * i.unitPricePaise, 0);
			order.status = 3; // preparing
		}
		return order;
	},

	async rejectItem(orderId: string, itemId: string, reason: string): Promise<OrderResponse> {
		await delay(100);
		const order = orders.find(o => o.id === orderId);
		if (!order) throw new Error('Order not found');
		const item = order.items.find(i => i.id === itemId);
		if (!item) throw new Error('Item not found');
		item.confirmedQuantity = 0;
		item.itemStatus = 4;
		item.rejectionReason = reason;

		const allConfirmed = order.items.every(i => i.itemStatus >= 3);
		if (allConfirmed) {
			order.confirmedTotalPaise = order.items.reduce((sum, i) => sum + (i.confirmedQuantity ?? 0) * i.unitPricePaise, 0);
			order.status = order.confirmedTotalPaise > 0 ? 3 : 8;
		}
		return order;
	},

	async updateStatus(orderId: string, status: number, _extra?: { pickupCode?: string }): Promise<OrderResponse> {
		await delay(100);
		const order = orders.find(o => o.id === orderId);
		if (!order) throw new Error('Order not found');
		order.status = status;
		if (status === 7) order.completedAt = new Date().toISOString();
		return order;
	},

	async assignAgent(orderId: string, agentId: string): Promise<OrderResponse> {
		await delay(100);
		const order = orders.find(o => o.id === orderId);
		if (!order) throw new Error('Order not found');
		const agent = agents.find(a => a.id === agentId);
		if (!agent) throw new Error('Agent not found');
		order.delivery = { agentId, agentName: agent.name, status: 0 };
		agent.activeDeliveries++;
		return order;
	},

	// Get all orders for seller (by shopId)
	async listForShop(shopId: string): Promise<OrderResponse[]> {
		await delay();
		return orders.filter(o => o.shopId === shopId);
	},
};

// ── Routine Service ──
export const routineService = {
	async list(): Promise<RoutineResponse[]> {
		await delay();
		return [...routines];
	},
	async get(routineId: string): Promise<RoutineResponse | null> {
		await delay(100);
		return routines.find(r => r.id === routineId) ?? null;
	},
	async create(data: { label: string; shopId: string; frequency: number; dayOfWeek?: number; dayOfMonth?: number; items: { productId: string; quantity: number }[] }): Promise<RoutineResponse> {
		await delay();
		const shop = DEMO_SHOPS.find(s => s.id === data.shopId);
		const routine: RoutineResponse = {
			id: `rtn-gen-${Date.now()}`, label: data.label,
			shopId: data.shopId, shopName: shop?.name ?? '',
			frequency: data.frequency, dayOfWeek: data.dayOfWeek, dayOfMonth: data.dayOfMonth,
			isPaused: false, nextRunAt: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
			items: data.items.map(i => {
				const product = DEMO_PRODUCTS.find(p => p.id === i.productId);
				return { productId: i.productId, productName: product?.name ?? '', pricePaise: product?.pricePaise ?? 0, quantity: i.quantity };
			}),
		};
		routines = [routine, ...routines];
		return routine;
	},
	async togglePause(routineId: string): Promise<RoutineResponse> {
		await delay(100);
		const routine = routines.find(r => r.id === routineId);
		if (!routine) throw new Error('Routine not found');
		routine.isPaused = !routine.isPaused;
		return routine;
	},
	async delete(routineId: string): Promise<void> {
		await delay(100);
		routines = routines.filter(r => r.id !== routineId);
	},
};

// ── Draft Service ──
export const draftService = {
	async list(): Promise<DraftOrderResponse[]> {
		await delay();
		return [...drafts];
	},
	async confirm(draftId: string): Promise<OrderResponse> {
		await delay(200);
		const draft = drafts.find(d => d.id === draftId);
		if (!draft) throw new Error('Draft not found');
		const items = JSON.parse(draft.itemsSnapshot) as { productId: string; productName: string; pricePaise: number; quantity: number }[];
		// Create order from draft
		orderCounter++;
		const order: OrderResponse = {
			id: `ord-draft-${orderCounter}`, orderNumber: `ORD-${orderCounter}`,
			shopId: draft.shopId, shopName: draft.shopName,
			status: 1, fulfillmentType: 0, subtotalPaise: draft.estimatedTotalPaise,
			createdAt: new Date().toISOString(),
			confirmationExpiresAt: new Date(Date.now() + 5 * 60 * 1000).toISOString(),
			items: items.map((it, idx) => ({
				id: `oi-draft-${orderCounter}-${idx}`, productId: it.productId,
				productName: it.productName, unitPricePaise: it.pricePaise,
				requestedQuantity: it.quantity, itemStatus: 0,
			})),
		};
		orders = [order, ...orders];
		drafts = drafts.filter(d => d.id !== draftId);
		return order;
	},
	async dismiss(draftId: string): Promise<void> {
		await delay(100);
		drafts = drafts.filter(d => d.id !== draftId);
	},
};

// ── Notification Service ──
export const notificationService = {
	async list(): Promise<NotificationResponse[]> {
		await delay();
		return [...notifications];
	},
	async markRead(notifId: string): Promise<void> {
		await delay(50);
		const n = notifications.find(x => x.id === notifId);
		if (n) n.isRead = true;
	},
	async markAllRead(): Promise<void> {
		await delay(50);
		notifications.forEach(n => { n.isRead = true; });
	},
	getUnreadCount(): number {
		return notifications.filter(n => !n.isRead).length;
	},
};

// ── Agent Service ──
export const agentService = {
	async list(shopId: string): Promise<AgentResponse[]> {
		await delay();
		return [...agents];
	},
	async updateStatus(agentId: string, isActive: boolean): Promise<AgentResponse> {
		await delay(100);
		const agent = agents.find(a => a.id === agentId);
		if (!agent) throw new Error('Agent not found');
		agent.isActive = isActive;
		return agent;
	},
};

// ── Ledger / Reporting Service ──
export const ledgerService = {
	async getMonthlySummaries(): Promise<MonthlySummary[]> {
		await delay();
		return DEMO_LEDGER;
	},
	async getMonthDetail(month: string): Promise<MonthlySummary | null> {
		await delay();
		return DEMO_LEDGER.find(l => l.month === month) ?? null;
	},
};

// ── Support / Ticket Service ──
export const supportService = {
	async list(): Promise<SupportTicket[]> {
		await delay();
		return [...tickets];
	},
	async create(data: { type: SupportTicket['type']; subject: string; description: string; orderId?: string }): Promise<SupportTicket> {
		await delay();
		const session = getDemoSession();
		const ticket: SupportTicket = {
			id: `ticket-gen-${Date.now()}`,
			...data,
			status: 'open',
			createdBy: session?.user.id ?? 'usr-buyer-001',
			createdByName: session?.user.name ?? 'Unknown',
			createdAt: new Date().toISOString(),
			updatedAt: new Date().toISOString(),
		};
		tickets = [ticket, ...tickets];
		return ticket;
	},
	async updateStatus(ticketId: string, status: SupportTicket['status']): Promise<SupportTicket> {
		await delay(100);
		const ticket = tickets.find(t => t.id === ticketId);
		if (!ticket) throw new Error('Ticket not found');
		ticket.status = status;
		ticket.updatedAt = new Date().toISOString();
		return ticket;
	},
};

// ── Household Service ──
export const householdService = {
	async getForUser(userId: string): Promise<Household | null> {
		await delay();
		return DEMO_HOUSEHOLDS.find(h => h.members.includes(userId)) ?? null;
	},
	async getMembers(householdId: string): Promise<{ id: string; name: string; phone: string; role: string }[]> {
		await delay();
		const hh = DEMO_HOUSEHOLDS.find(h => h.id === householdId);
		if (!hh) return [];
		const { DEMO_USERS } = await import('./demo-data.js');
		return hh.members.map(mid => {
			const u = DEMO_USERS.find(x => x.id === mid);
			return { id: mid, name: u?.name ?? '', phone: u?.phone ?? '', role: u?.roles[0] ?? '' };
		});
	},
};

// ── Admin Service ──
export const adminService = {
	async getDashboard(): Promise<DashboardResponse> {
		await delay();
		return { ...DEMO_DASHBOARD, activeOrders: orders.filter(o => o.status < 7).length };
	},
	async getAuditLog(): Promise<AuditEntry[]> {
		await delay();
		return [...DEMO_AUDIT_LOG];
	},
	async getAllUsers() {
		await delay();
		const { DEMO_USERS } = await import('./demo-data.js');
		return DEMO_USERS.map(u => ({
			id: u.id, name: u.name, phone: u.phone, roles: u.roles,
			flatNumber: u.flatNumber, isActive: true,
		}));
	},
	async getAllShops(): Promise<ShopResponse[]> {
		await delay();
		return [...DEMO_SHOPS];
	},
	async getAllOrders(): Promise<OrderResponse[]> {
		await delay();
		return [...orders];
	},
	async getAllTickets(): Promise<SupportTicket[]> {
		await delay();
		return [...tickets];
	},
};
