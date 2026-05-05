<script lang="ts">
  import { adminService } from '@society-commerce/api-client';

  let orders = $state<any[]>([]);
  let loading = $state(true);

  $effect(() => {
    adminService.getAllOrders().then(o => { orders = o; loading = false; });
  });

  const totalRevenue = $derived(orders.reduce((sum, o) => sum + (o.confirmedTotalPaise ?? o.subtotalPaise), 0));
  const completedOrders = $derived(orders.filter(o => o.status === 7).length);
  const cancelledOrders = $derived(orders.filter(o => o.status === 8).length);
  const avgOrderValue = $derived(orders.length ? Math.round(totalRevenue / orders.length) : 0);

  function formatPrice(paise: number) { return `\u20B9${(paise / 100).toFixed(0)}`; }
</script>

<div class="p-6">
  <h1 class="text-2xl font-bold text-gray-900 mb-6">Reports & Analytics</h1>

  {#if loading}
    <div class="flex justify-center py-12"><div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div></div>
  {:else}
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <p class="text-sm text-gray-500">Total Revenue</p>
        <p class="text-2xl font-bold text-green-600 mt-1">{formatPrice(totalRevenue)}</p>
      </div>
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <p class="text-sm text-gray-500">Total Orders</p>
        <p class="text-2xl font-bold text-gray-900 mt-1">{orders.length}</p>
      </div>
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <p class="text-sm text-gray-500">Completed</p>
        <p class="text-2xl font-bold text-purple-600 mt-1">{completedOrders}</p>
      </div>
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <p class="text-sm text-gray-500">Avg Order Value</p>
        <p class="text-2xl font-bold text-gray-900 mt-1">{formatPrice(avgOrderValue)}</p>
      </div>
    </div>

    <div class="bg-white rounded-xl p-5 border border-gray-100">
      <h3 class="font-semibold text-gray-900 mb-4">Summary</h3>
      <div class="space-y-3">
        <div class="flex justify-between text-sm">
          <span class="text-gray-500">Fulfillment Rate</span>
          <span class="font-medium">{orders.length ? ((completedOrders / orders.length) * 100).toFixed(0) : 0}%</span>
        </div>
        <div class="flex justify-between text-sm">
          <span class="text-gray-500">Cancellation Rate</span>
          <span class="font-medium text-red-600">{orders.length ? ((cancelledOrders / orders.length) * 100).toFixed(0) : 0}%</span>
        </div>
        <div class="flex justify-between text-sm">
          <span class="text-gray-500">Delivery vs Pickup</span>
          <span class="font-medium">{orders.filter(o => o.fulfillmentType === 1).length} / {orders.filter(o => o.fulfillmentType === 2).length}</span>
        </div>
      </div>
    </div>
  {/if}
</div>
