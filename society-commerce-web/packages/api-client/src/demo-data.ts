/**
 * Demo/seed data for the Society Commerce MVP.
 * This provides realistic interconnected data for all roles.
 */
import type {
	UserInfo, ShopResponse, ProductResponse, CartItemResponse,
	OrderResponse, OrderItemResponse, RoutineResponse, RoutineItemResponse,
	NotificationResponse, AgentResponse, DashboardResponse, OrderSummaryResponse,
	DraftOrderResponse
} from './types.js';

// ── Demo Users ──
export interface DemoUser extends UserInfo {
	password?: string; // for demo auth only
	flatNumber?: string;
	householdId?: string;
}

export const DEMO_USERS: DemoUser[] = [
	{
		id: 'usr-buyer-001', name: 'Rahul Sharma', phone: '+919876543210',
		preferredLanguage: 'en', roles: ['flat_owner'],
		flatId: 'flat-a101', flatNumber: 'A-101', householdId: 'hh-001',
	},
	{
		id: 'usr-buyer-002', name: 'Priya Sharma', phone: '+919876543211',
		preferredLanguage: 'en', roles: ['household_member'],
		flatId: 'flat-a101', flatNumber: 'A-101', householdId: 'hh-001',
	},
	{
		id: 'usr-buyer-003', name: 'Amit Patel', phone: '+919876543212',
		preferredLanguage: 'hi', roles: ['flat_owner'],
		flatId: 'flat-b204', flatNumber: 'B-204', householdId: 'hh-002',
	},
	{
		id: 'usr-seller-001', name: 'Vijay Kumar', phone: '+919876543220',
		preferredLanguage: 'en', roles: ['seller_owner'],
		shopId: 'shop-lucky',
	},
	{
		id: 'usr-seller-002', name: 'Anita Devi', phone: '+919876543221',
		preferredLanguage: 'hi', roles: ['seller_manager'],
		shopId: 'shop-lucky',
	},
	{
		id: 'usr-agent-001', name: 'Raju Singh', phone: '+919876543230',
		preferredLanguage: 'hi', roles: ['delivery_agent'],
		shopId: 'shop-lucky',
	},
	{
		id: 'usr-admin-001', name: 'Suresh Admin', phone: '+919876543240',
		preferredLanguage: 'en', roles: ['admin'],
	},
];

// ── Households ──
export interface Household {
	id: string;
	flatId: string;
	flatNumber: string;
	ownerId: string;
	members: string[]; // user IDs
}

export const DEMO_HOUSEHOLDS: Household[] = [
	{ id: 'hh-001', flatId: 'flat-a101', flatNumber: 'A-101', ownerId: 'usr-buyer-001', members: ['usr-buyer-001', 'usr-buyer-002'] },
	{ id: 'hh-002', flatId: 'flat-b204', flatNumber: 'B-204', ownerId: 'usr-buyer-003', members: ['usr-buyer-003'] },
];

// ── Shops ──
export const DEMO_SHOPS: ShopResponse[] = [
	{
		id: 'shop-lucky', name: 'Lucky General Store', category: 'General Store',
		description: 'Your neighbourhood store for daily essentials, dairy, snacks, and household items.',
		logoUrl: null as unknown as string, isActive: true,
	},
	{
		id: 'shop-fresh', name: 'Fresh Fruits & Vegetables', category: 'Fruits & Vegetables',
		description: 'Farm-fresh fruits and vegetables delivered daily.',
		logoUrl: null as unknown as string, isActive: true,
	},
	{
		id: 'shop-medplus', name: 'MedPlus Pharmacy', category: 'Pharmacy',
		description: 'Medicines, health supplements, and personal care products.',
		logoUrl: null as unknown as string, isActive: true,
	},
];

// ── Product Categories ──
export const DEMO_CATEGORIES: Record<string, string[]> = {
	'shop-lucky': ['Dairy', 'Bread & Bakery', 'Snacks', 'Beverages', 'Atta & Rice', 'Dal & Pulses', 'Household', 'Eggs'],
	'shop-fresh': ['Fruits', 'Vegetables', 'Herbs & Greens'],
	'shop-medplus': ['OTC Medicines', 'Supplements', 'Personal Care'],
};

// ── Products ──
export const DEMO_PRODUCTS: ProductResponse[] = [
	// Lucky Store - Dairy
	{ id: 'p-001', name: 'Amul Taaza Toned Milk 500ml', category: 'Dairy', pricePaise: 2900, imageUrl: '/products/amul_taza_tonedmilk.avif', inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-002', name: 'Amul Gold Full Cream Milk 500ml', category: 'Dairy', pricePaise: 3400, imageUrl: '/products/amul_gold_full_cream.avif', inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-003', name: 'Mother Dairy Full Cream 500ml', category: 'Dairy', pricePaise: 3200, imageUrl: '/products/motherdairyfullcream.avif', inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-004', name: 'Amul Cow Milk 500ml', category: 'Dairy', pricePaise: 2800, imageUrl: '/products/amulcowmilk.avif', inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-005', name: 'Mother Dairy Toned Milk 500ml', category: 'Dairy', pricePaise: 2600, imageUrl: '/products/motehrdairytonedmilk.avif', inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-006', name: 'Amul Salted Butter 100g', category: 'Dairy', pricePaise: 5600, imageUrl: '/products/amulsaltedbutter100g.avif', inventoryType: 1, stockQuantity: 15, isAvailable: true, isRegulated: false },
	{ id: 'p-007', name: 'Amul Masti Pouch Curd 1kg', category: 'Dairy', pricePaise: 7500, imageUrl: '/products/amulmastipouchcurd1kg.avif', inventoryType: 1, stockQuantity: 10, isAvailable: true, isRegulated: false },
	{ id: 'p-008', name: 'Amul Masti Pouch Curd 390g', category: 'Dairy', pricePaise: 3500, imageUrl: '/products/amulmastipouchcurd390gs.avif', inventoryType: 1, stockQuantity: 12, isAvailable: true, isRegulated: false },
	{ id: 'p-009', name: 'Mother Dairy Salted Butter 100g', category: 'Dairy', pricePaise: 5400, imageUrl: '/products/motherdairysaltedbutter100g.avif', inventoryType: 1, stockQuantity: 8, isAvailable: true, isRegulated: false },
	{ id: 'p-010', name: 'Amul Unsalted Butter 100g', category: 'Dairy', pricePaise: 5600, imageUrl: '/products/amulunslatedbutter100g.avif', inventoryType: 1, stockQuantity: 6, isAvailable: true, isRegulated: false },
	// Lucky Store - Bread
	{ id: 'p-011', name: 'Harvest Gold White Bread 350g', category: 'Bread & Bakery', pricePaise: 4000, imageUrl: '/products/harvestgoldwhite350gs.avif', inventoryType: 1, stockQuantity: 8, isAvailable: true, isRegulated: false },
	{ id: 'p-012', name: 'English Oven 100% Atta Bread 400g', category: 'Bread & Bakery', pricePaise: 5000, imageUrl: '/products/englishover100percentatta400gs.avif', inventoryType: 1, stockQuantity: 6, isAvailable: true, isRegulated: false },
	{ id: 'p-013', name: 'Harvest Gold Bombay Pao', category: 'Bread & Bakery', pricePaise: 2500, imageUrl: '/products/harvestgoldbombaypao.avif', inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-014', name: 'Britannia Brown Bread', category: 'Bread & Bakery', pricePaise: 4500, imageUrl: '/products/britanniabrownbread.avif', inventoryType: 1, stockQuantity: 5, isAvailable: true, isRegulated: false },
	{ id: 'p-015', name: 'English Oven Sandwich White Bread 400g', category: 'Bread & Bakery', pricePaise: 4500, imageUrl: '/products/englishovensandwhichwhitebread400gs.avif', inventoryType: 1, stockQuantity: 4, isAvailable: true, isRegulated: false },
	// Lucky Store - Snacks
	{ id: 'p-016', name: 'Maggi Noodles 4-pack', category: 'Snacks', pricePaise: 5600, inventoryType: 1, stockQuantity: 25, isAvailable: true, isRegulated: false },
	{ id: 'p-017', name: 'Lays Classic Salted 52g', category: 'Snacks', pricePaise: 2000, inventoryType: 1, stockQuantity: 30, isAvailable: true, isRegulated: false },
	{ id: 'p-018', name: 'Kurkure Masala Munch 90g', category: 'Snacks', pricePaise: 2000, inventoryType: 1, stockQuantity: 20, isAvailable: true, isRegulated: false },
	// Lucky Store - Beverages
	{ id: 'p-019', name: 'Coca Cola 750ml', category: 'Beverages', pricePaise: 3800, inventoryType: 1, stockQuantity: 15, isAvailable: true, isRegulated: false },
	{ id: 'p-020', name: 'Tata Tea Gold 250g', category: 'Beverages', pricePaise: 12000, inventoryType: 1, stockQuantity: 10, isAvailable: true, isRegulated: false },
	{ id: 'p-021', name: 'Nescafé Classic 50g', category: 'Beverages', pricePaise: 14500, inventoryType: 1, stockQuantity: 8, isAvailable: true, isRegulated: false },
	// Lucky Store - Atta & Rice
	{ id: 'p-022', name: 'Aashirvaad Atta 5kg', category: 'Atta & Rice', pricePaise: 27000, inventoryType: 1, stockQuantity: 5, isAvailable: true, isRegulated: false },
	{ id: 'p-023', name: 'India Gate Basmati Rice 1kg', category: 'Atta & Rice', pricePaise: 16000, inventoryType: 1, stockQuantity: 8, isAvailable: true, isRegulated: false },
	// Lucky Store - Dal
	{ id: 'p-024', name: 'Toor Dal 1kg', category: 'Dal & Pulses', pricePaise: 14500, inventoryType: 1, stockQuantity: 10, isAvailable: true, isRegulated: false },
	{ id: 'p-025', name: 'Moong Dal 1kg', category: 'Dal & Pulses', pricePaise: 13000, inventoryType: 1, stockQuantity: 7, isAvailable: true, isRegulated: false },
	// Lucky Store - Household
	{ id: 'p-026', name: 'Surf Excel 1kg', category: 'Household', pricePaise: 12500, inventoryType: 1, stockQuantity: 8, isAvailable: true, isRegulated: false },
	{ id: 'p-027', name: 'Vim Dishwash Bar 200g', category: 'Household', pricePaise: 2000, inventoryType: 1, stockQuantity: 15, isAvailable: true, isRegulated: false },
	// Lucky Store - Eggs
	{ id: 'p-028', name: 'Eggs (6 pack)', category: 'Eggs', pricePaise: 4200, inventoryType: 1, stockQuantity: 20, isAvailable: true, isRegulated: false },
	{ id: 'p-029', name: 'Eggs (12 pack)', category: 'Eggs', pricePaise: 7800, inventoryType: 1, stockQuantity: 12, isAvailable: true, isRegulated: false },
	// Fresh Store - Fruits
	{ id: 'p-030', name: 'Banana (1 dozen)', category: 'Fruits', pricePaise: 4000, inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-031', name: 'Apple Shimla (1kg)', category: 'Fruits', pricePaise: 16000, inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-032', name: 'Mango Alphonso (1kg)', category: 'Fruits', pricePaise: 40000, inventoryType: 2, isAvailable: true, isRegulated: false },
	// Fresh Store - Vegetables
	{ id: 'p-033', name: 'Onion (1kg)', category: 'Vegetables', pricePaise: 3000, inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-034', name: 'Tomato (1kg)', category: 'Vegetables', pricePaise: 4000, inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-035', name: 'Potato (1kg)', category: 'Vegetables', pricePaise: 2500, inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-036', name: 'Green Chilli (100g)', category: 'Vegetables', pricePaise: 1000, inventoryType: 2, isAvailable: true, isRegulated: false },
	// Fresh Store - Herbs
	{ id: 'p-037', name: 'Coriander Bunch', category: 'Herbs & Greens', pricePaise: 1000, inventoryType: 2, isAvailable: true, isRegulated: false },
	{ id: 'p-038', name: 'Mint Leaves Bunch', category: 'Herbs & Greens', pricePaise: 1000, inventoryType: 2, isAvailable: true, isRegulated: false },
	// MedPlus - OTC
	{ id: 'p-039', name: 'Crocin 650mg (15 tablets)', category: 'OTC Medicines', pricePaise: 3200, inventoryType: 1, stockQuantity: 50, isAvailable: true, isRegulated: true },
	{ id: 'p-040', name: 'Dolo 650mg (15 tablets)', category: 'OTC Medicines', pricePaise: 3000, inventoryType: 1, stockQuantity: 40, isAvailable: true, isRegulated: true },
	{ id: 'p-041', name: 'Vicks VapoRub 50ml', category: 'OTC Medicines', pricePaise: 14500, inventoryType: 1, stockQuantity: 20, isAvailable: true, isRegulated: false },
	// MedPlus - Supplements
	{ id: 'p-042', name: 'Revital H (30 capsules)', category: 'Supplements', pricePaise: 42000, inventoryType: 1, stockQuantity: 12, isAvailable: true, isRegulated: false },
	// MedPlus - Personal Care
	{ id: 'p-043', name: 'Dettol Antiseptic 250ml', category: 'Personal Care', pricePaise: 18000, inventoryType: 1, stockQuantity: 10, isAvailable: true, isRegulated: false },
];

// ── Product-Shop mapping ──
export const PRODUCT_SHOP_MAP: Record<string, string> = {};
DEMO_PRODUCTS.forEach(p => {
	if (p.id <= 'p-029') PRODUCT_SHOP_MAP[p.id] = 'shop-lucky';
	else if (p.id <= 'p-038') PRODUCT_SHOP_MAP[p.id] = 'shop-fresh';
	else PRODUCT_SHOP_MAP[p.id] = 'shop-medplus';
});

export function getProductsForShop(shopId: string): ProductResponse[] {
	return DEMO_PRODUCTS.filter(p => PRODUCT_SHOP_MAP[p.id] === shopId);
}

// ── Demo Orders ──
export const DEMO_ORDERS: OrderResponse[] = [
	{
		id: 'ord-001', orderNumber: 'ORD-1001', shopId: 'shop-lucky', shopName: 'Lucky General Store',
		status: 7, fulfillmentType: 0, subtotalPaise: 14700, confirmedTotalPaise: 14700,
		orderNotes: null as unknown as string, deliveryNotes: 'Leave at door',
		createdAt: '2026-05-01T08:30:00Z', completedAt: '2026-05-01T09:15:00Z',
		items: [
			{ id: 'oi-001', productId: 'p-001', productName: 'Amul Taaza Toned Milk 500ml', unitPricePaise: 2900, requestedQuantity: 2, confirmedQuantity: 2, itemStatus: 3 },
			{ id: 'oi-002', productId: 'p-011', productName: 'Harvest Gold White Bread 350g', unitPricePaise: 4000, requestedQuantity: 1, confirmedQuantity: 1, itemStatus: 3 },
			{ id: 'oi-003', productId: 'p-028', productName: 'Eggs (6 pack)', unitPricePaise: 4200, requestedQuantity: 1, confirmedQuantity: 1, itemStatus: 3 },
		],
		delivery: { agentId: 'usr-agent-001', agentName: 'Raju Singh', status: 3, pickedUpAt: '2026-05-01T08:50:00Z', deliveredAt: '2026-05-01T09:15:00Z' },
	},
	{
		id: 'ord-002', orderNumber: 'ORD-1002', shopId: 'shop-lucky', shopName: 'Lucky General Store',
		status: 3, fulfillmentType: 0, subtotalPaise: 9500, confirmedTotalPaise: 6600,
		orderNotes: 'Please check expiry date on curd',
		createdAt: '2026-05-03T07:00:00Z',
		confirmationExpiresAt: '2026-05-03T07:05:00Z',
		items: [
			{ id: 'oi-004', productId: 'p-002', productName: 'Amul Gold Full Cream Milk 500ml', unitPricePaise: 3400, requestedQuantity: 1, confirmedQuantity: 1, itemStatus: 3 },
			{ id: 'oi-005', productId: 'p-008', productName: 'Amul Masti Pouch Curd 390g', unitPricePaise: 3500, requestedQuantity: 1, confirmedQuantity: 0, itemStatus: 4, rejectionReason: 'Out of stock today' },
			{ id: 'oi-006', productId: 'p-017', productName: 'Lays Classic Salted 52g', unitPricePaise: 2000, requestedQuantity: 1, confirmedQuantity: 1, itemStatus: 3 },
		],
	},
	{
		id: 'ord-003', orderNumber: 'ORD-1003', shopId: 'shop-fresh', shopName: 'Fresh Fruits & Vegetables',
		status: 1, fulfillmentType: 0, subtotalPaise: 11000,
		createdAt: '2026-05-03T09:00:00Z',
		confirmationExpiresAt: '2026-05-03T09:05:00Z',
		items: [
			{ id: 'oi-007', productId: 'p-030', productName: 'Banana (1 dozen)', unitPricePaise: 4000, requestedQuantity: 1, itemStatus: 0 },
			{ id: 'oi-008', productId: 'p-033', productName: 'Onion (1kg)', unitPricePaise: 3000, requestedQuantity: 2, itemStatus: 0 },
			{ id: 'oi-009', productId: 'p-037', productName: 'Coriander Bunch', unitPricePaise: 1000, requestedQuantity: 1, itemStatus: 0 },
		],
	},
	{
		id: 'ord-004', orderNumber: 'ORD-1004', shopId: 'shop-lucky', shopName: 'Lucky General Store',
		status: 5, fulfillmentType: 1, pickupCode: '4829', subtotalPaise: 5600, confirmedTotalPaise: 5600,
		createdAt: '2026-05-03T10:00:00Z',
		items: [
			{ id: 'oi-010', productId: 'p-016', productName: 'Maggi Noodles 4-pack', unitPricePaise: 5600, requestedQuantity: 1, confirmedQuantity: 1, itemStatus: 3 },
		],
	},
];

// ── Demo Routines ──
export const DEMO_ROUTINES: RoutineResponse[] = [
	{
		id: 'rtn-001', label: 'Daily Milk & Bread', shopId: 'shop-lucky', shopName: 'Lucky General Store',
		frequency: 0, isPaused: false, nextRunAt: '2026-05-04T06:00:00Z',
		items: [
			{ productId: 'p-001', productName: 'Amul Taaza Toned Milk 500ml', pricePaise: 2900, quantity: 2 },
			{ productId: 'p-011', productName: 'Harvest Gold White Bread 350g', pricePaise: 4000, quantity: 1 },
		],
	},
	{
		id: 'rtn-002', label: 'Weekly Groceries', shopId: 'shop-lucky', shopName: 'Lucky General Store',
		frequency: 1, dayOfWeek: 0, isPaused: false, nextRunAt: '2026-05-05T06:00:00Z',
		items: [
			{ productId: 'p-028', productName: 'Eggs (6 pack)', pricePaise: 4200, quantity: 2 },
			{ productId: 'p-024', productName: 'Toor Dal 1kg', pricePaise: 14500, quantity: 1 },
			{ productId: 'p-020', productName: 'Tata Tea Gold 250g', pricePaise: 12000, quantity: 1 },
		],
	},
];

// ── Demo Draft Orders (from routines) ──
export const DEMO_DRAFTS: DraftOrderResponse[] = [
	{
		id: 'draft-001', routineId: 'rtn-001', routineLabel: 'Daily Milk & Bread',
		shopId: 'shop-lucky', shopName: 'Lucky General Store',
		status: 0, estimatedTotalPaise: 9800,
		itemsSnapshot: JSON.stringify([
			{ productId: 'p-001', productName: 'Amul Taaza Toned Milk 500ml', pricePaise: 2900, quantity: 2 },
			{ productId: 'p-011', productName: 'Harvest Gold White Bread 350g', pricePaise: 4000, quantity: 1 },
		]),
		scheduledFor: '2026-05-04T06:00:00Z', createdAt: '2026-05-03T22:00:00Z',
	},
];

// ── Demo Notifications ──
export const DEMO_NOTIFICATIONS: NotificationResponse[] = [
	{ id: 'notif-001', type: 'order_confirmed', titleKey: 'notification.order_confirmed', bodyKey: 'notification.order_confirmed_body', params: JSON.stringify({ orderNumber: 'ORD-1002' }), data: JSON.stringify({ orderId: 'ord-002' }), isRead: false, createdAt: '2026-05-03T07:02:00Z' },
	{ id: 'notif-002', type: 'item_rejected', titleKey: 'notification.item_rejected', bodyKey: 'notification.item_rejected_body', params: JSON.stringify({ productName: 'Amul Masti Pouch Curd 390g', orderNumber: 'ORD-1002' }), data: JSON.stringify({ orderId: 'ord-002' }), isRead: false, createdAt: '2026-05-03T07:02:00Z' },
	{ id: 'notif-003', type: 'order_delivered', titleKey: 'notification.order_delivered', bodyKey: 'notification.order_delivered_body', params: JSON.stringify({ orderNumber: 'ORD-1001' }), data: JSON.stringify({ orderId: 'ord-001' }), isRead: true, createdAt: '2026-05-01T09:15:00Z' },
	{ id: 'notif-004', type: 'routine_draft', titleKey: 'notification.routine_draft', bodyKey: 'notification.routine_draft_body', params: JSON.stringify({ routineLabel: 'Daily Milk & Bread' }), data: JSON.stringify({ draftId: 'draft-001' }), isRead: false, createdAt: '2026-05-03T22:00:00Z' },
];

// ── Demo Agents ──
export const DEMO_AGENTS: AgentResponse[] = [
	{ id: 'usr-agent-001', name: 'Raju Singh', phone: '+919876543230', isActive: true, activeDeliveries: 1 },
	{ id: 'usr-agent-002', name: 'Mohan Das', phone: '+919876543231', isActive: true, activeDeliveries: 0 },
];

// ── Demo Audit Log ──
export interface AuditEntry {
	id: string;
	action: string;
	actorId: string;
	actorName: string;
	targetType: string;
	targetId: string;
	details: string;
	timestamp: string;
}

export const DEMO_AUDIT_LOG: AuditEntry[] = [
	{ id: 'audit-001', action: 'user.approve', actorId: 'usr-admin-001', actorName: 'Suresh Admin', targetType: 'user', targetId: 'usr-buyer-001', details: 'Approved user registration for flat A-101', timestamp: '2026-04-28T10:00:00Z' },
	{ id: 'audit-002', action: 'shop.approve', actorId: 'usr-admin-001', actorName: 'Suresh Admin', targetType: 'shop', targetId: 'shop-lucky', details: 'Approved shop: Lucky General Store', timestamp: '2026-04-28T11:00:00Z' },
	{ id: 'audit-003', action: 'order.override', actorId: 'usr-admin-001', actorName: 'Suresh Admin', targetType: 'order', targetId: 'ord-001', details: 'Manually marked order as delivered', timestamp: '2026-05-01T09:20:00Z' },
	{ id: 'audit-004', action: 'user.deactivate', actorId: 'usr-admin-001', actorName: 'Suresh Admin', targetType: 'user', targetId: 'usr-buyer-003', details: 'Temporarily deactivated user account', timestamp: '2026-05-02T14:00:00Z' },
	{ id: 'audit-005', action: 'config.update', actorId: 'usr-admin-001', actorName: 'Suresh Admin', targetType: 'config', targetId: 'confirmation_window', details: 'Changed confirmation window from 5min to 7min', timestamp: '2026-05-02T15:00:00Z' },
];

// ── Demo Support Tickets ──
export interface SupportTicket {
	id: string;
	type: 'dispute' | 'bug' | 'general';
	status: 'open' | 'in_progress' | 'resolved' | 'closed';
	subject: string;
	description: string;
	createdBy: string;
	createdByName: string;
	assignedTo?: string;
	orderId?: string;
	createdAt: string;
	updatedAt: string;
}

export const DEMO_TICKETS: SupportTicket[] = [
	{ id: 'ticket-001', type: 'dispute', status: 'open', subject: 'Wrong item delivered', description: 'I ordered Amul Gold but received Amul Taaza.', createdBy: 'usr-buyer-001', createdByName: 'Rahul Sharma', orderId: 'ord-001', createdAt: '2026-05-01T10:00:00Z', updatedAt: '2026-05-01T10:00:00Z' },
	{ id: 'ticket-002', type: 'bug', status: 'in_progress', subject: 'Cart not updating', description: 'When I add items to cart, the count shows wrong number.', createdBy: 'usr-buyer-003', createdByName: 'Amit Patel', createdAt: '2026-05-02T16:00:00Z', updatedAt: '2026-05-03T08:00:00Z' },
	{ id: 'ticket-003', type: 'general', status: 'resolved', subject: 'How to add household member?', description: 'I want to add my wife to the app.', createdBy: 'usr-buyer-001', createdByName: 'Rahul Sharma', createdAt: '2026-04-29T12:00:00Z', updatedAt: '2026-04-30T09:00:00Z' },
];

// ── Ledger / Monthly Summary ──
export interface LedgerEntry {
	id: string;
	orderId: string;
	orderNumber: string;
	shopName: string;
	amountPaise: number;
	date: string;
	status: 'paid' | 'unpaid';
}

export interface MonthlySummary {
	month: string; // YYYY-MM
	totalPaise: number;
	paidPaise: number;
	unpaidPaise: number;
	orderCount: number;
	entries: LedgerEntry[];
}

export const DEMO_LEDGER: MonthlySummary[] = [
	{
		month: '2026-05', totalPaise: 35800, paidPaise: 14700, unpaidPaise: 21100, orderCount: 4,
		entries: [
			{ id: 'led-001', orderId: 'ord-001', orderNumber: 'ORD-1001', shopName: 'Lucky General Store', amountPaise: 14700, date: '2026-05-01', status: 'paid' },
			{ id: 'led-002', orderId: 'ord-002', orderNumber: 'ORD-1002', shopName: 'Lucky General Store', amountPaise: 6600, date: '2026-05-03', status: 'unpaid' },
			{ id: 'led-003', orderId: 'ord-003', orderNumber: 'ORD-1003', shopName: 'Fresh Fruits & Vegetables', amountPaise: 11000, date: '2026-05-03', status: 'unpaid' },
			{ id: 'led-004', orderId: 'ord-004', orderNumber: 'ORD-1004', shopName: 'Lucky General Store', amountPaise: 5600, date: '2026-05-03', status: 'unpaid' },
		],
	},
	{
		month: '2026-04', totalPaise: 45200, paidPaise: 45200, unpaidPaise: 0, orderCount: 8,
		entries: [
			{ id: 'led-010', orderId: 'ord-old-1', orderNumber: 'ORD-0993', shopName: 'Lucky General Store', amountPaise: 12400, date: '2026-04-05', status: 'paid' },
			{ id: 'led-011', orderId: 'ord-old-2', orderNumber: 'ORD-0994', shopName: 'Lucky General Store', amountPaise: 8900, date: '2026-04-10', status: 'paid' },
			{ id: 'led-012', orderId: 'ord-old-3', orderNumber: 'ORD-0995', shopName: 'Fresh Fruits & Vegetables', amountPaise: 6500, date: '2026-04-15', status: 'paid' },
			{ id: 'led-013', orderId: 'ord-old-4', orderNumber: 'ORD-0996', shopName: 'Lucky General Store', amountPaise: 17400, date: '2026-04-22', status: 'paid' },
		],
	},
];

// ── Dashboard Stats ──
export const DEMO_DASHBOARD: DashboardResponse = {
	totalUsers: 47, totalShops: 3, activeOrders: 3,
	todayOrders: 3, todayRevenuePaise: 26100,
	pendingSellers: 0, openTickets: 2,
};
