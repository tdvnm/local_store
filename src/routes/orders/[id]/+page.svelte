<script lang="ts">
  import { onMount } from "svelte";
  import { page } from "$app/state";
  import { auth } from "$lib/stores/auth.svelte";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import Badge from "$lib/components/Badge.svelte";
  import StatusTracker from "$lib/components/StatusTracker.svelte";

  type OrderItem = { id: number; product_name: string; quantity: number; price: number; category: string };
  type Order = {
    id: number;
    user_name: string;
    flat_no: string;
    status: string;
    payment_method: string;
    delivery_mode: string;
    scheduled_time: string | null;
    total: number;
    created_at: string;
    items: OrderItem[];
  };

  let order = $state<Order | null>(null);
  let loading = $state(true);

  const orderId = $derived(page.params.id);

  onMount(async () => {
    try {
      const res = await fetch(`/api/orders?user_id=${auth.user?.id}`);
      const orders: Order[] = await res.json();
      order = orders.find((o) => o.id === Number(orderId)) || null;
    } finally {
      loading = false;
    }
  });

  const deliveryLabels: Record<string, string> = {
    urgent: "Urgent Delivery",
    scheduled: "Scheduled Delivery",
    pickup: "Self Pickup",
    delivery: "Standard Delivery",
  };

  const paymentLabels: Record<string, string> = {
    cod: "Cash on Delivery",
    tab: "Monthly Tab",
  };

  function formatDate(dateStr: string): string {
    return new Date(dateStr + "Z").toLocaleString([], {
      day: "numeric",
      month: "short",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  function handlePrint() {
    window.print();
  }
</script>

<svelte:head>
  <title>Order #{orderId} - Lucky Store</title>
</svelte:head>

<div class="min-h-screen">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3 flex items-center justify-between print:hidden">
      <div class="flex items-center gap-4">
        <a href="/orders" class="text-xl">&larr;</a>
        <h1 class="text-xl font-bold">Order #{orderId}</h1>
      </div>
      {#if order}
        <button onclick={handlePrint} class="text-sm opacity-80 hover:opacity-100 flex items-center gap-1 no-print">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
          </svg>
          Print
        </button>
      {/if}
    </div>
  </Header>

  <main class="max-w-lg mx-auto px-4 py-4 pb-24">
    {#if loading}
      <div class="text-center text-gray-400 py-12">Loading...</div>
    {:else if !order}
      <div class="text-center py-16">
        <p class="text-gray-400">Order not found</p>
        <a href="/orders" class="inline-block mt-3 text-accent font-semibold text-sm">View all orders</a>
      </div>
    {:else}
      <div class="space-y-4 animate-in">
        <!-- Print header (hidden on screen, shown in print) -->
        <div class="hidden print-only mb-4">
          <h1 class="text-xl font-bold">Lucky Store - Order #{order.id}</h1>
          <p class="text-sm text-gray-500">{formatDate(order.created_at)}</p>
          <p class="text-sm">{order.user_name} - Flat {order.flat_no}</p>
        </div>

        <!-- Status tracker -->
        <Card class="p-4">
          <StatusTracker status={order.status} deliveryMode={order.delivery_mode || "delivery"} />
        </Card>

        <!-- Order info -->
        <Card class="p-4">
          <div class="flex flex-wrap gap-2 mb-3">
            <Badge class="bg-gray-100 text-gray-600" label={deliveryLabels[order.delivery_mode] || "Delivery"} />
            <Badge class="bg-gray-100 text-gray-600" label={paymentLabels[order.payment_method] || order.payment_method} />
            {#if order.scheduled_time}
              <Badge class="bg-amber-50 text-amber-700" label="Before {order.scheduled_time}" />
            {/if}
          </div>
          <div class="text-xs text-gray-400">Ordered {formatDate(order.created_at)}</div>
        </Card>

        <!-- Items -->
        <Card class="divide-y divide-gray-100">
          <div class="px-4 py-3">
            <h3 class="font-bold text-gray-800">Items</h3>
          </div>
          {#each order.items as item (item.id)}
            <div class="flex justify-between px-4 py-3">
              <div>
                <span class="text-sm text-gray-800">{item.product_name}</span>
                <span class="text-xs text-gray-400 ml-1">x{item.quantity}</span>
              </div>
              <span class="text-sm font-medium text-gray-700">&#8377;{item.price * item.quantity}</span>
            </div>
          {/each}
          <div class="flex justify-between px-4 py-3 font-bold">
            <span>Total</span>
            <span class="text-price">&#8377;{order.total}</span>
          </div>
        </Card>

        <div class="no-print">
          <Button variant="ghost" class="w-full py-3 rounded-xl" onclick={handlePrint}>
            Print Receipt
          </Button>
        </div>
      </div>
    {/if}
  </main>
</div>
