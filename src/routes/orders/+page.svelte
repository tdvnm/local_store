<script lang="ts">
  import { onMount } from "svelte";
  import { auth } from "$lib/stores/auth.svelte";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Pill from "$lib/components/Pill.svelte";
  import Badge from "$lib/components/Badge.svelte";

  type OrderItem = { id: number; product_name: string; quantity: number; price: number };
  type Order = {
    id: number;
    status: string;
    payment_method: string;
    delivery_mode: string;
    total: number;
    created_at: string;
    items: OrderItem[];
  };

  let orders = $state<Order[]>([]);
  let filter = $state("All");
  let loading = $state(true);

  onMount(async () => {
    if (!auth.user) return;
    try {
      const res = await fetch(`/api/orders?user_id=${auth.user.id}`);
      orders = await res.json();
    } finally {
      loading = false;
    }
  });

  const activeStatuses = new Set(["pending", "confirmed", "packed", "out_for_delivery", "ready_for_pickup"]);
  const doneStatuses = new Set(["delivered"]);

  const filtered = $derived(
    filter === "All"
      ? orders
      : filter === "Active"
        ? orders.filter((o) => activeStatuses.has(o.status))
        : orders.filter((o) => doneStatuses.has(o.status))
  );

  const statusColors: Record<string, string> = {
    pending: "bg-yellow-100 text-yellow-800",
    confirmed: "bg-blue-100 text-blue-800",
    packed: "bg-indigo-100 text-indigo-800",
    out_for_delivery: "bg-purple-100 text-purple-800",
    ready_for_pickup: "bg-purple-100 text-purple-800",
    delivered: "bg-[#eef1ff] text-[var(--c-emerald)]",
    cancelled: "bg-red-100 text-red-800",
  };

  const statusLabels: Record<string, string> = {
    pending: "Pending",
    confirmed: "Confirmed",
    packed: "Packed",
    out_for_delivery: "Out for Delivery",
    ready_for_pickup: "Ready for Pickup",
    delivered: "Delivered",
    cancelled: "Cancelled",
  };

  const deliveryLabels: Record<string, string> = {
    urgent: "Urgent",
    scheduled: "Scheduled",
    pickup: "Pickup",
    delivery: "Delivery",
  };

  function itemSummary(items: OrderItem[]): string {
    if (items.length === 0) return "";
    const first = items[0].product_name;
    if (items.length === 1) return first;
    return `${first} +${items.length - 1} more`;
  }

  function formatDate(dateStr: string): string {
    const d = new Date(dateStr + "Z");
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
  <title>My Orders - Lucky Store</title>
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
          <a href="/orders/{order.id}" class="block animate-in">
            <Card class="p-4 hover:shadow-lg transition-shadow">
              <div class="flex items-start justify-between mb-2">
                <div class="flex items-center gap-2">
                  <span class="font-bold text-gray-800">#{order.id}</span>
                  <Badge class={statusColors[order.status]} label={statusLabels[order.status] || order.status} />
                  {#if order.delivery_mode && order.delivery_mode !== "delivery"}
                    <Badge class="bg-gray-100 text-gray-600" label={deliveryLabels[order.delivery_mode] || order.delivery_mode} />
                  {/if}
                </div>
                <span class="font-bold text-gray-800">&#8377;{order.total}</span>
              </div>
              <p class="text-sm text-gray-500 truncate">{itemSummary(order.items)}</p>
              <p class="text-xs text-gray-400 mt-1">{formatDate(order.created_at)}</p>
            </Card>
          </a>
        {/each}
      </div>
    {/if}
  </main>
</div>
