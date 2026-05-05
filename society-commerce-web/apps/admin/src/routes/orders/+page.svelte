<script lang="ts">
  import { adminService } from '@society-commerce/api-client';
  import type { OrderResponse } from '@society-commerce/api-client';

  let orders = $state<OrderResponse[]>([]);
  let loading = $state(true);
  let statusFilter = $state('all');

  $effect(() => {
    adminService.getAllOrders().then(o => { orders = o; loading = false; });
  });

  const statusLabel: Record<number, string> = {
    0: 'New', 1: 'Pending Confirmation', 2: 'Confirmed', 3: 'Partially Confirmed',
    4: 'Preparing', 5: 'Ready', 6: 'Out for Delivery', 7: 'Completed', 8: 'Cancelled',
  };
  const statusColor: Record<number, string> = {
    0: 'bg-blue-100 text-blue-800', 1: 'bg-amber-100 text-amber-800',
    2: 'bg-green-100 text-green-800', 3: 'bg-yellow-100 text-yellow-800',
    4: 'bg-indigo-100 text-indigo-800',
    7: 'bg-green-50 text-green-700', 8: 'bg-red-50 text-red-700',
  };

  const filtered = $derived(statusFilter === 'all' ? orders : orders.filter(o => String(o.status) === statusFilter));

  function formatPrice(paise: number) { return `\u20B9${(paise / 100).toFixed(0)}`; }
</script>

<div class="p-6">
  <div class="flex items-center justify-between mb-6">
    <h1 class="text-2xl font-bold text-gray-900">All Orders</h1>
    <span class="text-sm text-gray-400">{orders.length} total</span>
  </div>

  <div class="flex gap-2 mb-4 flex-wrap">
    <button onclick={() => statusFilter = 'all'} class="px-3 py-1.5 rounded-lg text-xs {statusFilter === 'all' ? 'bg-purple-100 text-purple-800 font-medium' : 'text-gray-500 hover:bg-gray-100'}">All</button>
    {#each Object.entries(statusLabel) as [key, label]}
      <button onclick={() => statusFilter = key} class="px-3 py-1.5 rounded-lg text-xs {statusFilter === key ? 'bg-purple-100 text-purple-800 font-medium' : 'text-gray-500 hover:bg-gray-100'}">{label}</button>
    {/each}
  </div>

  {#if loading}
    <div class="flex justify-center py-12"><div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div></div>
  {:else}
    <div class="bg-white rounded-xl border border-gray-100 overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-gray-50">
          <tr>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Order #</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Items</th>
            <th class="text-right px-4 py-3 font-medium text-gray-600">Total</th>
            <th class="text-center px-4 py-3 font-medium text-gray-600">Type</th>
            <th class="text-center px-4 py-3 font-medium text-gray-600">Status</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Created</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          {#each filtered as order (order.id)}
            <tr class="hover:bg-gray-50">
              <td class="px-4 py-3 font-medium text-gray-900">{order.orderNumber}</td>
              <td class="px-4 py-3 text-gray-600">{(order as any).itemCount ?? order.items?.length ?? '?'} items</td>
              <td class="px-4 py-3 text-right font-medium">{formatPrice(order.confirmedTotalPaise ?? order.subtotalPaise)}</td>
              <td class="px-4 py-3 text-center text-xs">{order.fulfillmentType === 2 ? 'Pickup' : 'Delivery'}</td>
              <td class="px-4 py-3 text-center"><span class="px-2 py-0.5 rounded-full text-xs font-medium {statusColor[order.status] ?? 'bg-gray-100 text-gray-700'}">{statusLabel[order.status] ?? 'Unknown'}</span></td>
              <td class="px-4 py-3 text-gray-500 text-xs">{new Date(order.createdAt).toLocaleString()}</td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>
  {/if}
</div>
