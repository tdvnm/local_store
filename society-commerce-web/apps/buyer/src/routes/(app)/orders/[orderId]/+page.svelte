<script lang="ts">
  import { page } from "$app/state";
  import { auth } from "$lib/stores/auth.svelte";
  import { orderService } from "$lib/api";
  import { createOrderHub, type OrderHubClient } from "@society-commerce/api-client/realtime";
  import * as signalr from '@microsoft/signalr';
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import Badge from "$lib/components/Badge.svelte";
  import StatusTracker from "$lib/components/StatusTracker.svelte";
  import type { OrderResponse } from "@society-commerce/api-client";
  import { OrderStatusMap, FulfillmentTypeMap, ItemStatusMap } from "$lib/types";

  const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5000/api';

  let order = $state<OrderResponse | null>(null);
  let loading = $state(true);
  let hub: OrderHubClient | null = null;

  const orderId = $derived(page.params.orderId);

  let loaded = false;
  $effect(() => {
    if (!loaded && orderId) {
      loaded = true;
      loadOrder();
      startHub();
    }
    return () => { hub?.stop(); };
  });

  function startHub() {
    if (!auth.token) return;
    hub = createOrderHub(API_BASE, () => auth.token, signalr);
    hub.onOrderUpdated(async (data) => {
      if (data.id === orderId) {
        order = await orderService.get(orderId);
      }
    });
    hub.start();
  }

  async function loadOrder() {
    try {
      order = await orderService.get(orderId);
    } catch (e) {
      console.error("Failed to load order:", e);
    } finally {
      loading = false;
    }
  }

  const statusKey = $derived(order ? (OrderStatusMap[order.status] || "created") : "created");
  const fulfillmentLabel = $derived(order ? (FulfillmentTypeMap[order.fulfillmentType] || "Delivery") : "Delivery");
  const canCancel = $derived(order ? order.status < 4 : false);

  function formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleString([], {
      day: "numeric",
      month: "short",
      year: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  }

  async function handleCancel() {
    if (!order || !confirm("Are you sure you want to cancel this order?")) return;
    try {
      await orderService.cancel(order.id);
      order.status = 8;
    } catch (e: any) {
      alert(e.message || "Failed to cancel order");
    }
  }

  function handlePrint() {
    window.print();
  }
</script>

<svelte:head>
  <title>Order #{order?.orderNumber || orderId} - Society Commerce</title>
</svelte:head>

<div class="min-h-screen">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3 flex items-center justify-between print:hidden">
      <div class="flex items-center gap-4">
        <a href="/orders" class="text-xl">&larr;</a>
        <h1 class="text-xl font-bold">Order #{order?.orderNumber || "..."}</h1>
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
        <Card class="p-4">
          <StatusTracker status={statusKey} deliveryMode={fulfillmentLabel} />
        </Card>

        <Card class="p-4">
          <div class="flex flex-wrap gap-2 mb-3">
            <Badge class="bg-gray-100 text-gray-600" label={fulfillmentLabel} />
            {#if order.pickupCode}
              <Badge class="bg-amber-50 text-amber-700" label="Pickup: {order.pickupCode}" />
            {/if}
          </div>
          <div class="text-xs text-gray-400">Ordered {formatDate(order.createdAt)}</div>
          {#if order.shopName}
            <div class="text-xs text-gray-400 mt-1">From: {order.shopName}</div>
          {/if}
        </Card>

        <Card class="divide-y divide-gray-100">
          <div class="px-4 py-3">
            <h3 class="font-bold text-gray-800">Items</h3>
          </div>
          {#each order.items as item (item.id)}
            <div class="flex justify-between px-4 py-3">
              <div>
                <span class="text-sm text-gray-800">{item.productName}</span>
                <span class="text-xs text-gray-400 ml-1">x{item.confirmedQuantity ?? item.requestedQuantity}</span>
              </div>
              <span class="text-sm font-medium text-gray-700">&#8377;{(item.unitPricePaise * (item.confirmedQuantity ?? item.requestedQuantity)) / 100}</span>
            </div>
          {/each}
          <div class="flex justify-between px-4 py-3 font-bold">
            <span>Total</span>
            <span class="text-price">&#8377;{(order.confirmedTotalPaise ?? order.subtotalPaise) / 100}</span>
          </div>
        </Card>

        <div class="no-print space-y-2">
          {#if canCancel}
            <Button variant="danger" class="w-full py-3 rounded-xl" onclick={handleCancel}>
              Cancel Order
            </Button>
          {/if}
          <Button variant="ghost" class="w-full py-3 rounded-xl" onclick={handlePrint}>
            Print Receipt
          </Button>
        </div>
      </div>
    {/if}
  </main>
</div>
