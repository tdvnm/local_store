/**
 * Shared reactive cart store for the buyer app.
 * Single source of truth for cart state across all pages.
 */
import { cartService } from '$lib/api';
import type { CartResponse } from '@society-commerce/api-client';

let cartData = $state<CartResponse | null>(null);
let shopId = $state<string | null>(null);
let loading = $state(false);
let _pendingOps = 0;

export const cart = {
	get data() { return cartData; },
	get items() { return cartData?.items ?? []; },
	get count() { return (cartData?.items ?? []).reduce((s, i) => s + i.quantity, 0); },
	get totalPaise() { return cartData?.totalPaise ?? 0; },
	get total() { return (cartData?.totalPaise ?? 0) / 100; },
	get shopName() { return cartData?.shopName ?? ''; },
	get shopId() { return shopId; },
	get loading() { return loading; },
	get hasPendingOps() { return _pendingOps > 0; },

	setShopId(id: string) {
		shopId = id;
	},

	async load(forShopId?: string) {
		const sid = forShopId ?? shopId;
		if (!sid) return;
		shopId = sid;
		loading = true;
		try {
			cartData = await cartService.get(sid);
		} catch (e) {
			console.error('[cart] Failed to load:', e);
		} finally {
			loading = false;
		}
	},

	async addItem(productId: string, productName: string, pricePaise: number, inventoryType: number, isAvailable: boolean) {
		if (!shopId) return;
		// Optimistic update
		if (cartData) {
			const existing = cartData.items.find(i => i.productId === productId);
			if (existing) {
				existing.quantity++;
			} else {
				cartData.items.push({ productId, productName, pricePaise, quantity: 1, inventoryType, isAvailable });
			}
			cartData.totalPaise = cartData.items.reduce((s, i) => s + i.pricePaise * i.quantity, 0);
			// Trigger reactivity
			cartData = { ...cartData, items: [...cartData.items] };
		}
		// Sync with service
		_pendingOps++;
		try {
			cartData = await cartService.addItem(shopId, productId, 1);
		} catch (e) {
			console.error('[cart] addItem failed:', e);
		} finally {
			_pendingOps--;
		}
	},

	async updateItem(productId: string, quantity: number) {
		if (!shopId || !cartData) return;
		// Optimistic update
		const item = cartData.items.find(i => i.productId === productId);
		if (item) {
			item.quantity = quantity;
			cartData.totalPaise = cartData.items.reduce((s, i) => s + i.pricePaise * i.quantity, 0);
			cartData = { ...cartData, items: [...cartData.items] };
		}
		_pendingOps++;
		try {
			cartData = await cartService.updateItem(shopId, productId, quantity);
		} catch (e) {
			console.error('[cart] updateItem failed:', e);
		} finally {
			_pendingOps--;
		}
	},

	async removeItem(productId: string) {
		if (!shopId || !cartData) return;
		// Optimistic update
		cartData.items = cartData.items.filter(i => i.productId !== productId);
		cartData.totalPaise = cartData.items.reduce((s, i) => s + i.pricePaise * i.quantity, 0);
		cartData = { ...cartData, items: [...cartData.items] };
		_pendingOps++;
		try {
			cartData = await cartService.removeItem(shopId, productId);
		} catch (e) {
			console.error('[cart] removeItem failed:', e);
		} finally {
			_pendingOps--;
		}
	},

	async decrementItem(productId: string) {
		if (!shopId || !cartData) return;
		const item = cartData.items.find(i => i.productId === productId);
		if (!item) return;
		if (item.quantity <= 1) {
			return this.removeItem(productId);
		}
		return this.updateItem(productId, item.quantity - 1);
	},

	clear() {
		if (shopId) cartService.clear(shopId).catch(console.error);
		cartData = { shopId: shopId ?? '', shopName: cartData?.shopName ?? '', items: [], totalPaise: 0 };
	},

	/** Reset after order placed */
	reset() {
		cartData = { shopId: shopId ?? '', shopName: cartData?.shopName ?? '', items: [], totalPaise: 0 };
	},

	getQty(productId: string): number {
		return cartData?.items.find(i => i.productId === productId)?.quantity ?? 0;
	},
};
