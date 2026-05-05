/**
 * Buyer app API layer.
 * Routes all data access through mock services (swap for real HTTP later).
 */
export { auth } from '$lib/stores/auth.svelte';
export {
	shopService, productService, cartService, orderService,
	routineService, draftService, notificationService, ledgerService,
	supportService, householdService,
} from '@society-commerce/api-client';
