<script lang="ts">
  import { auth } from "$lib/stores/auth.svelte";
  import { orderService, agentService } from "@society-commerce/api-client";
  import { createOrderHub, type OrderHubClient } from "@society-commerce/api-client/realtime";
  import * as signalr from '@microsoft/signalr';
  import type { OrderResponse, AgentResponse } from "@society-commerce/api-client";

  const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000/api';

  let orders = $state<OrderResponse[]>([]);
  let agents = $state<AgentResponse[]>([]);
  let filter = $state<'all' | 'pending' | 'active' | 'completed'>('active');
  let selectedOrder = $state<OrderResponse | null>(null);
  let loading = $state(true);
  let hub: OrderHubClient | null = null;

  let loaded = false;
  $effect(() => {
    if (!loaded && auth.shopId) { loaded = true; loadOrders(); startHub(); }
    return () => { hub?.stop(); };
  });

  function startHub() {
    if (!auth.token) return;
    try {
      hub = createOrderHub(API_BASE, () => auth.token, signalr);
      hub.onNewOrder(async (data) => {
        const order = await orderService.get(data.id);
        if (order && !orders.find(o => o.id === order.id)) {
          orders = [order, ...orders];
        }
      });
      hub.onOrderUpdated(async (data) => {
        const updated = await orderService.get(data.id);
        if (updated) {
          orders = orders.map(o => o.id === updated.id ? updated : o);
          if (selectedOrder?.id === updated.id) selectedOrder = updated;
        }
      });
      hub.start();
    } catch (err) {
      console.warn('[SignalR] Failed to initialize order hub:', err);
    }
  }

  async function loadOrders() {
    try {
      const [o, a] = await Promise.all([
        orderService.listForShop(auth.shopId!),
        agentService.list(auth.shopId!),
      ]);
      orders = o;
      agents = a;
    } catch (err) {
      console.error('[orders] Failed to load:', err);
    } finally { loading = false; }
  }

  const filtered = $derived.by(() => {
    if (filter === 'pending') return orders.filter(o => o.status === 1);
    if (filter === 'active') return orders.filter(o => o.status > 0 && o.status < 7 && o.status !== 8);
    if (filter === 'completed') return orders.filter(o => o.status === 7 || o.status === 8);
    return orders;
  });

  const pendingCount = $derived(orders.filter(o => o.status === 1).length);

  function formatPrice(paise: number) { return `₹${(paise / 100).toFixed(0)}`; }
  function timeAgo(date: string) {
    const diff = Date.now() - new Date(date).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 60) return `${mins}m ago`;
    return `${Math.floor(mins/60)}h ago`;
  }

  const statusLabel: Record<number, string> = {
    0: 'New', 1: 'Needs Confirmation', 2: 'Confirmed',
    3: 'Partially Confirmed', 4: 'Preparing', 5: 'Ready for Pickup',
    6: 'Out for Delivery', 7: 'Completed', 8: 'Cancelled',
  };
  const statusColor: Record<number, string> = {
    0: 'bg-blue-100 text-blue-800', 1: 'bg-amber-100 text-amber-800',
    2: 'bg-green-100 text-green-800', 3: 'bg-yellow-100 text-yellow-800',
    4: 'bg-indigo-100 text-indigo-800',
    5: 'bg-purple-100 text-purple-800', 6: 'bg-purple-100 text-purple-800',
    7: 'bg-green-50 text-green-700', 8: 'bg-red-50 text-red-700',
  };
  const itemStatusLabel: Record<number, string> = {
    0: 'Pending', 1: 'Confirmed', 2: 'Partial', 3: 'Rejected',
    4: 'Auto-rejected', 5: 'Sub Proposed', 6: 'Sub Accepted', 7: 'Sub Rejected',
  };

  // ── Order Actions ──
  async function confirmItem(orderId: string, itemId: string, qty: number) {
    const updated = await orderService.confirmItem(orderId, itemId, qty);
    orders = orders.map(o => o.id === updated.id ? updated : o);
    if (selectedOrder?.id === updated.id) selectedOrder = updated;
  }

  async function rejectItem(orderId: string, itemId: string) {
    const reason = prompt('Reason for rejection:') ?? 'Not available';
    const updated = await orderService.rejectItem(orderId, itemId, reason);
    orders = orders.map(o => o.id === updated.id ? updated : o);
    if (selectedOrder?.id === updated.id) selectedOrder = updated;
  }

  async function updateStatus(orderId: string, status: number) {
    const updated = await orderService.updateStatus(orderId, status);
    orders = orders.map(o => o.id === updated.id ? updated : o);
    if (selectedOrder?.id === updated.id) selectedOrder = updated;
  }

  async function assignAgent(orderId: string, agentId: string) {
    const updated = await orderService.assignAgent(orderId, agentId);
    orders = orders.map(o => o.id === updated.id ? updated : o);
    if (selectedOrder?.id === updated.id) selectedOrder = updated;
  }
</script>

<div class="p-6 h-screen flex flex-col">
  <div class="flex items-center justify-between mb-4">
    <h1 class="text-2xl font-bold text-gray-900">Orders</h1>
    {#if pendingCount > 0}
      <span class="px-3 py-1 text-sm bg-amber-100 text-amber-800 rounded-full font-medium animate-pulse">{pendingCount} need confirmation</span>
    {/if}
  </div>

  <!-- Filter tabs -->
  <div class="flex gap-2 mb-4">
    {#each [['all', 'All'], ['pending', 'Pending'], ['active', 'Active'], ['completed', 'Done']] as [key, label]}
      <button
        onclick={() => filter = key as any}
        class="px-3 py-1.5 rounded-lg text-sm transition-colors {filter === key ? 'bg-purple-100 text-purple-800 font-medium' : 'text-gray-500 hover:bg-gray-100'}"
      >{label}</button>
    {/each}
  </div>

  {#if loading}
    <div class="flex-1 flex items-center justify-center">
      <div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
    </div>
  {:else}
    <div class="flex-1 flex gap-4 min-h-0">
      <!-- Order list -->
      <div class="w-96 overflow-y-auto space-y-2 flex-shrink-0">
        {#if filtered.length === 0}
          <div class="text-center py-12 text-gray-400 text-sm">No orders in this category</div>
        {/if}
        {#each filtered as order (order.id)}
          <button
            onclick={() => selectedOrder = order}
            class="w-full text-left bg-white rounded-xl p-4 border transition-all {selectedOrder?.id === order.id ? 'border-purple-300 shadow-md' : 'border-gray-100 hover:border-gray-200'}"
          >
            <div class="flex justify-between items-start">
              <div>
                <p class="font-semibold text-gray-900">{order.orderNumber}</p>
                <p class="text-xs text-gray-500 mt-0.5">{order.items?.length ?? 0} items · {formatPrice(order.subtotalPaise)}</p>
              </div>
              <span class="px-2 py-0.5 rounded-full text-xs font-medium {statusColor[order.status] ?? 'bg-gray-100'}">
                {statusLabel[order.status]}
              </span>
            </div>
            <p class="text-xs text-gray-400 mt-2">{timeAgo(order.createdAt)} · {order.fulfillmentType === 1 ? 'Delivery' : 'Pickup'}</p>
          </button>
        {/each}
      </div>

      <!-- Order detail panel -->
      <div class="flex-1 overflow-y-auto">
        {#if selectedOrder}
          <div class="bg-white rounded-xl border border-gray-100 p-6">
            <div class="flex justify-between items-start mb-4">
              <div>
                <h2 class="text-xl font-bold text-gray-900">{selectedOrder.orderNumber}</h2>
                <p class="text-sm text-gray-500">{new Date(selectedOrder.createdAt).toLocaleString()}</p>
              </div>
              <span class="px-3 py-1 rounded-full text-sm font-medium {statusColor[selectedOrder.status]}">
                {statusLabel[selectedOrder.status]}
              </span>
            </div>

            {#if selectedOrder.orderNotes}
              <div class="bg-yellow-50 border border-yellow-200 rounded-lg p-3 mb-4 text-sm text-yellow-800">
                <strong>Note:</strong> {selectedOrder.orderNotes}
              </div>
            {/if}

            <!-- Items with confirmation controls -->
            <div class="space-y-3 mb-6">
              <h3 class="text-sm font-semibold text-gray-700 uppercase tracking-wide">Items</h3>
              {#each selectedOrder.items ?? [] as item (item.id)}
                <div class="flex items-center justify-between p-3 rounded-lg bg-gray-50">
                  <div class="flex-1">
                    <p class="text-sm font-medium text-gray-900">{item.productName}</p>
                    <p class="text-xs text-gray-500">
                      Qty: {item.requestedQuantity} · {formatPrice(item.unitPricePaise * item.requestedQuantity)}
                      {#if item.confirmedQuantity !== undefined && item.confirmedQuantity !== null}
                        <span class="ml-2 font-medium {item.confirmedQuantity < item.requestedQuantity ? 'text-amber-600' : 'text-green-600'}">
                          → Confirmed: {item.confirmedQuantity}
                        </span>
                      {/if}
                    </p>
                    {#if item.rejectionReason}
                      <p class="text-xs text-red-500 mt-0.5">{item.rejectionReason}</p>
                    {/if}
                  </div>
                  <div class="flex items-center gap-1">
                    {#if item.itemStatus === 0 && selectedOrder.status === 1}
                      <!-- Confirmation controls for pending items -->
                      <button onclick={() => confirmItem(selectedOrder!.id, item.id, item.requestedQuantity)}
                        class="px-2 py-1 text-xs bg-green-100 text-green-700 rounded-lg hover:bg-green-200 transition-colors">✓ Full</button>
                      <button onclick={() => { const qty = prompt(`Confirm how many? (requested: ${item.requestedQuantity})`); if (qty) confirmItem(selectedOrder!.id, item.id, parseInt(qty)); }}
                        class="px-2 py-1 text-xs bg-yellow-100 text-yellow-700 rounded-lg hover:bg-yellow-200 transition-colors">Partial</button>
                      <button onclick={() => rejectItem(selectedOrder!.id, item.id)}
                        class="px-2 py-1 text-xs bg-red-100 text-red-700 rounded-lg hover:bg-red-200 transition-colors">✗</button>
                    {:else}
                      <span class="px-2 py-0.5 text-xs rounded-full {item.itemStatus === 1 ? 'bg-green-100 text-green-700' : item.itemStatus === 3 || item.itemStatus === 4 ? 'bg-red-100 text-red-700' : item.itemStatus === 2 ? 'bg-yellow-100 text-yellow-700' : 'bg-gray-100 text-gray-700'}">
                        {itemStatusLabel[item.itemStatus] ?? 'Unknown'}
                      </span>
                    {/if}
                  </div>
                </div>
              {/each}
            </div>

            <!-- Total -->
            <div class="flex justify-between items-center py-3 border-t border-gray-100 mb-6">
              <span class="text-sm text-gray-600">Total</span>
              <span class="text-lg font-bold">{formatPrice(selectedOrder.confirmedTotalPaise ?? selectedOrder.subtotalPaise)}</span>
            </div>

            <!-- Actions based on status -->
            <div class="space-y-2">
              {#if selectedOrder.status === 2 || selectedOrder.status === 3}
                <!-- Confirmed / Partially Confirmed → Start Preparing -->
                <button onclick={() => updateStatus(selectedOrder!.id, 4)} class="w-full py-2.5 bg-indigo-600 text-white rounded-lg text-sm font-medium hover:bg-indigo-700 transition-colors">
                  Start Preparing
                </button>
              {/if}

              {#if selectedOrder.status === 4}
                {#if selectedOrder.fulfillmentType === 1}
                  <!-- Delivery flow: assign agent then dispatch -->
                  {#if !selectedOrder.delivery}
                    <div class="flex items-center gap-2">
                      <select onchange={(e) => assignAgent(selectedOrder!.id, (e.target as HTMLSelectElement).value)} class="flex-1 p-2 border border-gray-200 rounded-lg text-sm">
                        <option value="">Assign delivery agent...</option>
                        {#each agents.filter(a => a.isActive) as agent}
                          <option value={agent.id}>{agent.name} ({agent.activeDeliveries} active)</option>
                        {/each}
                      </select>
                    </div>
                  {:else}
                    <p class="text-sm text-gray-600">Agent: <strong>{selectedOrder.delivery.agentName}</strong></p>
                    <button onclick={() => updateStatus(selectedOrder!.id, 6)} class="w-full py-2.5 bg-purple-600 text-white rounded-lg text-sm font-medium hover:bg-purple-700 transition-colors">
                      Mark Out for Delivery
                    </button>
                  {/if}
                {:else}
                  <!-- Pickup flow: mark ready -->
                  <button onclick={() => updateStatus(selectedOrder!.id, 5)} class="w-full py-2.5 bg-purple-600 text-white rounded-lg text-sm font-medium hover:bg-purple-700 transition-colors">
                    Mark Ready for Pickup
                  </button>
                {/if}
              {/if}

              {#if selectedOrder.status === 5}
                {#if selectedOrder.pickupCode}
                  <p class="text-center text-sm text-gray-500">Pickup code: <strong class="text-gray-900 text-lg">{selectedOrder.pickupCode}</strong></p>
                {/if}
                <button onclick={() => updateStatus(selectedOrder!.id, 7)} class="w-full py-2.5 bg-green-600 text-white rounded-lg text-sm font-medium hover:bg-green-700 transition-colors">
                  Mark Collected
                </button>
              {/if}

              {#if selectedOrder.status === 6}
                <button onclick={() => updateStatus(selectedOrder!.id, 7)} class="w-full py-2.5 bg-green-600 text-white rounded-lg text-sm font-medium hover:bg-green-700 transition-colors">
                  Mark Delivered
                </button>
              {/if}
            </div>

            <!-- Confirmation timer -->
            {#if selectedOrder.status === 1 && selectedOrder.confirmationExpiresAt}
              <div class="mt-4 bg-amber-50 border border-amber-200 rounded-lg p-3 text-center">
                <p class="text-sm text-amber-800 font-medium">⏱ Confirmation window active</p>
                <p class="text-xs text-amber-600 mt-1">Confirm or reject items before timer expires</p>
              </div>
            {/if}
          </div>
        {:else}
          <div class="h-full flex items-center justify-center text-gray-400">
            <p class="text-sm">Select an order to view details</p>
          </div>
        {/if}
      </div>
    </div>
  {/if}
</div>
