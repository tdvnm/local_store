<script lang="ts">
  import { onMount } from "svelte";
  import Pill from "$lib/components/Pill.svelte";
  import OrderCard from "$lib/components/OrderCard.svelte";

  type OrderItem = { id: number; product_name: string; quantity: number; price: number; category: string };
  type Order = { id: number; user_name: string; flat_no: string; status: string; payment_method: string; total: number; created_at: string; items: OrderItem[] };

  let orders = $state<Order[]>([]);
  let filter = $state("pending");

  function fetchOrders() {
    const url = filter ? `/api/orders?status=${filter}` : "/api/orders";
    fetch(url).then((r) => r.json()).then((data) => (orders = data));
  }

  $effect(() => {
    // Re-fetch when filter changes
    void filter;
    fetchOrders();
  });

  onMount(() => {
    const i = setInterval(fetchOrders, 10000);
    return () => clearInterval(i);
  });

  async function updateStatus(id: number, status: string) {
    await fetch(`/api/orders/${id}`, { method: "PATCH", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ status }) });
    fetchOrders();
  }
</script>

<svelte:head>
  <title>Orders - Lucky Store Admin</title>
</svelte:head>

<div>
  <div class="flex gap-2 mb-6 overflow-x-auto">
    {#each ["pending", "confirmed", "ready", "delivered", "cancelled", ""] as s (s)}
      <Pill label={s || "All"} active={filter === s} onclick={() => (filter = s)} />
    {/each}
  </div>

  {#if orders.length === 0}
    <div class="text-center text-gray-400 py-12">No orders</div>
  {/if}

  <div class="space-y-4">
    {#each orders as order (order.id)}
      <OrderCard {order} onupdatestatus={updateStatus} />
    {/each}
  </div>
</div>
