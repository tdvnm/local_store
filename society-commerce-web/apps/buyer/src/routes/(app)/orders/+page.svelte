<script lang="ts">
  import { orderService } from "$lib/api";
  import { auth } from "$lib/stores/auth.svelte";
  import { createOrderHub, type OrderHubClient } from "@society-commerce/api-client/realtime";
  import * as signalr from '@microsoft/signalr';
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Pill from "$lib/components/Pill.svelte";
  import Badge from "$lib/components/Badge.svelte";
  import type { OrderSummaryResponse } from "@society-commerce/api-client";
  import { OrderStatusMap, FulfillmentTypeMap } from "$lib/types";

  const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000/api';

  let orders = $state<OrderSummaryResponse[]>([]);
  let filter = $state("All");
  let loading = $state(true);
  let hub: OrderHubClient | null = null;

  let loaded = false;
  $effect(() => {
    if (!loaded) {
      loaded = true;
      loadOrders();
      startHub();
    }
    return () => { hub?.stop(); };
  });

  function startHub() {
    if (!auth.token) return;
    hub = createOrderHub(API_BASE, () => auth.token, signalr);
    hub.onOrderUpdated(async () => {
      // Refresh the orders list when any update comes in
      const data = await orderService.list({ page: 1, pageSize: 50 });
      orders = data.items;
    });
    hub.start();
  }

  async function loadOrders() {
    try {
      const data = await orderService.list({ page: 1, pageSize: 50 });
      orders = data.items;
    } finally {
      loading = false;
    }
  }

  const activeStatuses = new Set([0, 1, 2, 3, 4, 5, 6]);
  const doneStatuses = new Set([7]);

  const filtered = $derived(
    filter === "All"
      ? orders
      : filter === "Active"
        ? orders.filter((o) => activeStatuses.has(o.status))
        : orders.filter((o) => doneStatuses.has(o.status))
  );

  const statusColors: Record<string, string> = {
    created: "bg-gray-100 text-gray-800",
    awaiting_confirmation: "bg-amber-100 text-amber-800",
    confirmed: "bg-blue-100 text-blue-800",
    partially_confirmed: "bg-yellow-100 text-yellow-800",
    preparing: "bg-indigo-100 text-indigo-800",
    ready_for_pickup: "bg-purple-100 text-purple-800",
    out_for_delivery: "bg-purple-100 text-purple-800",
    completed: "bg-[#eef1ff] text-[var(--c-emerald)]",
    cancelled: "bg-red-100 text-red-800",
  };

  function formatDate(dateStr: string): string {
    const d = new Date(dateStr);
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const date = new Date(d.getFullYear(), d.getMonth(), d.getDate());
    const diff = today.getTime() - date.getTime();
    const days = Math.floor(diff / 86400000);

    const time = d.toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
    if (days === 0) return `Today, ${time}`;
    if (days === 1) return `Yesterday, ${time}`;
    return d.toLocaleDateString([], { day: "numeric", month: "short" }) + `, ${time}`;
  }
</script>

<svelte:head>
  <title>My Orders - Society Commerce</title>
</svelte:head>

<div class="min-h-screen">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3">
      <h1 class="text-xl font-bold">My Orders</h1>
    </div>
  </Header>

  <main class="max-w-lg mx-auto px-4 py-4 pb-24">
    <div class="flex gap-2 mb-4">
      {#each ["All", "Active", "Delivered"] as f (f)}
        <Pill label={f} active={filter === f} onclick={() => (filter = f)} />
      {/each}
    </div>

    {#if loading}
      <div class="text-center text-gray-400 py-12">Loading orders...</div>
    {:else if filtered.length === 0}
      <div class="text-center py-16 animate-in">
        <div class="text-4xl mb-3 text-gray-300">
          <svg class="w-12 h-12 mx-auto" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="1.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
          </svg>
        </div>
        <p class="text-gray-400">
          {filter === "All" ? "No orders yet" : filter === "Active" ? "No active orders" : "No delivered orders"}
        </p>
        {#if filter === "All"}
          <a href="/" class="inline-block mt-3 text-accent font-semibold text-sm">Start shopping</a>
        {/if}
      </div>
    {:else}
      <div class="space-y-3">
        {#each filtered as order (order.id)}
          {@const statusKey = OrderStatusMap[order.status] || "created"}
          {@const fulfillment = FulfillmentTypeMap[order.fulfillmentType] || "delivery"}
          {@const displayTotal = (order.confirmedTotalPaise ?? order.subtotalPaise) / 100}
          <a href="/orders/{order.id}" class="block animate-in">
            <Card class="p-4 hover:shadow-lg transition-shadow">
              <div class="flex items-start justify-between mb-2">
                <div class="flex items-center gap-2">
                  <span class="font-bold text-gray-800">#{order.orderNumber}</span>
                  <Badge class={statusColors[statusKey] || "bg-gray-100 text-gray-600"} label={statusKey.replace(/_/g, " ")} />
                  {#if fulfillment !== "delivery"}
                    <Badge class="bg-gray-100 text-gray-600" label={fulfillment} />
                  {/if}
                </div>
                <span class="font-bold text-gray-800">&#8377;{displayTotal}</span>
              </div>
              <p class="text-sm text-gray-500">{order.itemCount} item{order.itemCount > 1 ? "s" : ""}</p>
              <p class="text-xs text-gray-400 mt-1">{formatDate(order.createdAt)}</p>
            </Card>
          </a>
        {/each}
      </div>
    {/if}
  </main>
</div>
