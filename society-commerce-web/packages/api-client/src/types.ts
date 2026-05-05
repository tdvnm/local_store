// Auth
export interface AuthResponse { accessToken: string; user: UserInfo }
export interface UserInfo {
	id: string; name: string; phone: string; preferredLanguage: string;
	roles: string[]; flatId?: string; shopId?: string;
}

// Shop
export interface ShopResponse {
	id: string; name: string; category: string; description?: string;
	logoUrl?: string; isActive: boolean;
}

// Product
export interface ProductResponse {
	id: string; name: string; category: string; pricePaise: number;
	description?: string; imageUrl?: string;
	inventoryType: number; stockQuantity?: number;
	isAvailable: boolean; isRegulated: boolean;
}

// Cart
export interface CartResponse { shopId: string; shopName: string; items: CartItemResponse[]; totalPaise: number }
export interface CartItemResponse {
	productId: string; productName: string; pricePaise: number;
	quantity: number; inventoryType: number; isAvailable: boolean;
}

// Order
export interface OrderResponse {
	id: string; orderNumber: string; shopId: string; shopName: string;
	status: number; fulfillmentType: number;
	pickupCode?: string; subtotalPaise: number; confirmedTotalPaise?: number;
	orderNotes?: string; deliveryNotes?: string;
	confirmationExpiresAt?: string; createdAt: string; completedAt?: string;
	items: OrderItemResponse[]; delivery?: DeliveryAssignmentResponse;
}

export interface OrderItemResponse {
	id: string; productId: string; productName: string; unitPricePaise: number;
	requestedQuantity: number; confirmedQuantity?: number;
	itemStatus: number; rejectionReason?: string;
	substitution?: SubstitutionResponse;
}

export interface SubstitutionResponse {
	id: string; substituteProductId: string; substituteProductName: string;
	substituteQuantity: number; substitutePricePaise: number;
	status: number; expiresAt: string;
}

export interface DeliveryAssignmentResponse {
	agentId: string; agentName: string; status: number;
	pickedUpAt?: string; deliveredAt?: string;
}

export interface OrderSummaryResponse {
	id: string; orderNumber: string; shopName: string;
	status: number; fulfillmentType: number;
	confirmedTotalPaise?: number; subtotalPaise: number;
	itemCount: number; createdAt: string;
}

// Routine
export interface RoutineResponse {
	id: string; label: string; shopId: string; shopName: string;
	frequency: number; dayOfWeek?: number; dayOfMonth?: number;
	isPaused: boolean; nextRunAt?: string;
	items: RoutineItemResponse[];
}
export interface RoutineItemResponse { productId: string; productName: string; pricePaise: number; quantity: number }

// Draft
export interface DraftOrderResponse {
	id: string; routineId: string; routineLabel: string; shopId: string; shopName: string;
	status: number; itemsSnapshot: string; estimatedTotalPaise: number;
	scheduledFor: string; createdAt: string;
}

// Notification
export interface NotificationResponse {
	id: string; type: string; titleKey: string; bodyKey: string;
	params: string; data: string; isRead: boolean; createdAt: string;
}

// Pagination
export interface PagedResponse<T> { items: T[]; totalCount: number; page: number; pageSize: number }

// Delivery agent
export interface AgentResponse { id: string; name: string; phone: string; isActive: boolean; activeDeliveries: number }

// Dashboard
export interface DashboardResponse {
	totalUsers: number; totalShops: number; activeOrders: number;
	todayOrders: number; todayRevenuePaise: number;
	pendingSellers: number; openTickets: number;
}
