<script lang="ts">
  import { auth } from "$lib/stores/auth.svelte";
  import { orderService, productService, notificationService } from "@society-commerce/api-client";
  import type { OrderResponse } from "@society-commerce/api-client";

  let pendingOrders = $state<OrderResponse[]>([]);
  let todayOrders = $state(0);
  let todayRevenue = $state(0);
  let pendingConfirmation = $state(0);
  let lowStockCount = $state(0);
  let unreadNotifs = $state(0);
  let loading = $state(true);

  let loaded = false;
  $effect(() => {
    if (!loaded && auth.shopId) { loaded = true; loadDashboard(); }
  });

  async function loadDashboard() {
    try {
      const [orders, products] = await Promise.all([
        orderService.listForShop(auth.shopId!),
        productService.list(auth.shopId!),
      ]);

      const today = new Date().toISOString().slice(0, 10);
      const todaysOrders = orders.filter(o => o.createdAt?.startsWith(today));
      todayOrders = todaysOrders.length;
      todayRevenue = todaysOrders.reduce((s, o) => s + (o.confirmedTotalPaise ?? o.subtotalPaise ?? 0), 0);
      pendingConfirmation = orders.filter(o => o.status === 1).length;
      pendingOrders = orders.filter(o => o.status < 7 && o.status !== 8).slice(0, 5);
      lowStockCount = products.filter(p => p.inventoryType === 1 && (p.stockQuantity ?? 0) < 5).length;
      const count = notificationService.getUnreadCount();
      unreadNotifs = count instanceof Promise ? await count : count;
    } catch (err) {
      console.error('[dashboard] Failed to load:', err);
    } finally { loading = false; }
  }

  function formatPrice(paise: number) { return `₹${(paise / 100).toFixed(0)}`; }

  const statusLabel: Record<number, string> = {
    0: 'New', 1: 'Awaiting Confirmation', 2: 'Confirmed',
    3: 'Preparing', 4: 'Preparing', 5: 'Ready for Pickup',
    6: 'Out for Delivery', 7: 'Completed', 8: 'Cancelled',
  };
  const statusColor: Record<number, string> = {
    0: 'bg-blue-100 text-blue-800', 1: 'bg-amber-100 text-amber-800',
    2: 'bg-green-100 text-green-800', 3: 'bg-indigo-100 text-indigo-800',
    4: 'bg-indigo-100 text-indigo-800', 5: 'bg-purple-100 text-purple-800',
    6: 'bg-purple-100 text-purple-800', 7: 'bg-green-100 text-green-700',
    8: 'bg-red-100 text-red-700',
  };
</script>

<div class="p-6">
  <div class="flex items-center justify-between mb-6">
    <h1 class="text-2xl font-bold text-gray-900">Dashboard</h1>
    {#if unreadNotifs > 0}
      <span class="px-2 py-1 text-xs bg-red-100 text-red-700 rounded-full">{unreadNotifs} new</span>
    {/if}
  </div>

  {#if loading}
    <div class="flex justify-center py-12">
      <div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
    </div>
  {:else}
    <!-- Stats grid -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
      <div class="bg-white rounded-xl p-4 border border-gray-100">
        <p class="text-2xl font-bold text-gray-900">{todayOrders}</p>
        <p class="text-xs text-gray-500 mt-1">Today's Orders</p>
      </div>
      <div class="bg-white rounded-xl p-4 border border-gray-100">
        <p class="text-2xl font-bold text-amber-600">{pendingConfirmation}</p>
        <p class="text-xs text-gray-500 mt-1">Pending Confirmation</p>
      </div>
      <div class="bg-white rounded-xl p-4 border border-gray-100">
        <p class="text-2xl font-bold text-green-600">{formatPrice(todayRevenue)}</p>
        <p class="text-xs text-gray-500 mt-1">Today's Revenue</p>
      </div>
      <div class="bg-white rounded-xl p-4 border border-gray-100">
        <p class="text-2xl font-bold {lowStockCount > 0 ? 'text-red-600' : 'text-gray-900'}">{lowStockCount}</p>
        <p class="text-xs text-gray-500 mt-1">Low Stock Items</p>
      </div>
    </div>

    <!-- Recent orders -->
    <section>
      <div class="flex items-center justify-between mb-3">
        <h2 class="text-sm font-semibold text-gray-700 uppercase tracking-wide">Active Orders</h2>
        <a href="/orders" class="text-xs text-purple-600 hover:text-purple-800">View all →</a>
      </div>
      {#if pendingOrders.length === 0}
        <div class="bg-white rounded-xl p-8 text-center text-gray-400 border border-gray-100">
          <p class="text-sm">No active orders right now</p>
        </div>
      {:else}
        <div class="space-y-2">
          {#each pendingOrders as order (order.id)}
            <a href="/orders" class="block bg-white rounded-xl p-4 border border-gray-100 hover:border-purple-200 hover:shadow-sm transition-all">
              <div class="flex items-center justify-between">
                <div>
                  <p class="font-medium text-gray-900 text-sm">{order.orderNumber}</p>
                  <p class="text-xs text-gray-500 mt-0.5">{order.items?.length ?? 0} item{(order.items?.length ?? 0) !== 1 ? 's' : ''} · {formatPrice(order.subtotalPaise)}</p>
                </div>
                <span class="px-2 py-0.5 rounded-full text-xs font-medium {statusColor[order.status] ?? 'bg-gray-100 text-gray-800'}">
                  {statusLabel[order.status] ?? 'Unknown'}
                </span>
              </div>
            </a>
          {/each}
        </div>
      {/if}
    </section>

    <!-- Alerts -->
    {#if lowStockCount > 0}
      <div class="mt-6 bg-amber-50 border border-amber-200 rounded-xl p-4 flex items-center gap-3">
        <svg class="w-5 h-5 text-amber-600 flex-shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L4.082 16.5c-.77.833.192 2.5 1.732 2.5z" />
        </svg>
        <div>
          <p class="text-sm font-medium text-amber-800">{lowStockCount} items are running low on stock</p>
          <a href="/catalog" class="text-xs text-amber-600 hover:text-amber-800">Review catalog →</a>
        </div>
      </div>
    {/if}
  {/if}
</div>
