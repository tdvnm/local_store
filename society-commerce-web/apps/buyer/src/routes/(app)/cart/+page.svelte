<script lang="ts">
  import { goto } from "$app/navigation";
  import { auth } from "$lib/stores/auth.svelte";
  import { cart } from "$lib/stores/cart.svelte";
  import { orderService } from "$lib/api";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import QtyStepper from "$lib/components/QtyStepper.svelte";

  let placing = $state(false);
  let orderPlaced = $state<{ id: string; orderNumber: string; totalPaise: number } | null>(null);
  let fulfillmentType = $state(1); // 1=delivery, 2=pickup (matches backend FulfillmentType enum)
  let orderNotes = $state("");
  let loading = $state(true);

  const shopId = localStorage.getItem("sc_shop") ?? "shop-lucky";

  let loaded = false;
  $effect(() => {
    if (!loaded) {
      loaded = true;
      loadCart();
    }
  });

  async function loadCart() {
    try {
      await cart.load(shopId);
    } catch (e) {
      console.error("Failed to load cart:", e);
    }
    loading = false;
  }

  const total = $derived(cart.totalPaise);
  const displayTotal = $derived(total / 100);
  const items = $derived(cart.items);

  async function updateQty(productId: string, currentQty: number, delta: number) {
    const newQty = currentQty + delta;
    if (newQty <= 0) {
      await cart.removeItem(productId);
    } else {
      await cart.updateItem(productId, newQty);
    }
  }

  async function handlePlaceOrder() {
    if (!auth.loggedIn || items.length === 0) return;
    placing = true;
    try {
      const order = await orderService.place({
        shopId, fulfillmentType,
        orderNotes: orderNotes || undefined,
      });
      orderPlaced = { id: order.id, orderNumber: order.orderNumber, totalPaise: order.subtotalPaise };
      cart.reset();
    } catch (e: any) {
      alert(e.message || "Failed to place order. Please try again.");
    } finally {
      placing = false;
    }
  }

  const deliveryModes = [
    { value: 1, label: "Delivery", desc: "Deliver to your flat" },
    { value: 2, label: "Pickup", desc: "I'll pick it up from the shop" },
  ];
</script>

<svelte:head>
  <title>Cart - Society Commerce</title>
</svelte:head>

{#if orderPlaced}
  <div class="min-h-screen flex items-center justify-center">
    <Card class="p-8 mx-4 text-center max-w-sm w-full animate-in">
      <div class="w-16 h-16 mx-auto mb-4 rounded-full bg-gradient-to-b from-[#1a7a5e] to-[var(--c-emerald)] flex items-center justify-center">
        <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
        </svg>
      </div>
      <h2 class="text-2xl font-bold text-accent mb-2">Order Placed!</h2>
      <p class="text-gray-500 mb-1">Order #{orderPlaced.orderNumber}</p>
      <p class="text-2xl font-bold text-gray-800 mb-6">&#8377;{orderPlaced.totalPaise / 100}</p>
      <div class="flex gap-3">
        <a href="/orders/{orderPlaced.id}" class="btn btn-ghost px-5 py-3 rounded-xl flex-1">Track Order</a>
        <a href="/" class="btn btn-primary px-5 py-3 rounded-xl flex-1">Back to Store</a>
      </div>
    </Card>
  </div>
{:else}
  <div class="min-h-screen">
    <Header>
      <div class="max-w-lg mx-auto px-4 py-3 flex items-center gap-4">
        <button onclick={() => history.back()} class="text-xl">&larr;</button>
        <h1 class="text-xl font-bold">Your Cart</h1>
      </div>
    </Header>

    <main class="max-w-lg mx-auto px-4 py-4 pb-32">
      {#if loading}
        <div class="text-center text-gray-400 py-12">Loading cart...</div>
      {:else if items.length === 0}
        <div class="text-center py-16 animate-in">
          <div class="text-5xl mb-4">&#x1F6D2;</div>
          <p class="text-gray-400 text-lg">Your cart is empty</p>
          <a href="/" class="inline-block mt-4 text-accent font-semibold">Browse products</a>
        </div>
      {:else}
        <div class="space-y-4 animate-in">
          <Card class="divide-y divide-gray-100">
            {#each items as item (item.productId)}
              <div class="flex items-center justify-between p-4">
                <div class="flex-1">
                  <h3 class="text-sm font-medium text-gray-800">{item.productName}</h3>
                  <p class="text-price font-semibold text-sm mt-1">&#8377;{item.pricePaise / 100}</p>
                </div>
                <div class="flex items-center gap-2">
                  <QtyStepper
                    quantity={item.quantity}
                    onincrement={() => updateQty(item.productId, item.quantity, 1)}
                    ondecrement={() => updateQty(item.productId, item.quantity, -1)}
                    class="h-8"
                  />
                  <span class="text-sm font-bold text-gray-700 w-16 text-right">&#8377;{(item.pricePaise * item.quantity) / 100}</span>
                </div>
              </div>
            {/each}
          </Card>

          <Card class="p-4">
            <div class="flex justify-between text-lg font-bold">
              <span>Total</span>
              <span class="text-price">&#8377;{displayTotal}</span>
            </div>
          </Card>

          <Card class="p-4">
            <h3 class="font-bold text-gray-800 mb-3">Fulfillment</h3>
            <div class="space-y-2">
              {#each deliveryModes as mode (mode.value)}
                <label class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors {fulfillmentType === mode.value ? 'border-[var(--c-cyan)] bg-[#eef1ff]' : 'border-gray-200'}">
                  <input type="radio" name="fulfillment" checked={fulfillmentType === mode.value} onchange={() => (fulfillmentType = mode.value)} class="accent-[var(--c-emerald)]" />
                  <div>
                    <div class="font-medium text-sm">{mode.label}</div>
                    <div class="text-xs text-gray-400">{mode.desc}</div>
                  </div>
                </label>
              {/each}
            </div>
          </Card>

          {#if auth.user}
            <Card class="p-4">
              <h3 class="font-bold text-gray-800 mb-2">Delivering to</h3>
              <div class="text-sm text-gray-600">
                <div class="font-medium">{auth.user.name}</div>
                <div>{auth.user.phone}</div>
              </div>
            </Card>
          {/if}
        </div>
      {/if}
    </main>

    {#if items.length > 0}
      <div class="fixed bottom-14 left-0 right-0 z-50 animate-slide-up">
        <div class="frosted">
          <div class="max-w-lg mx-auto px-4 py-4">
            <Button variant="primary" class="w-full py-4 rounded-xl text-lg disabled:opacity-50" onclick={handlePlaceOrder} disabled={placing}>
              {placing ? "Placing Order..." : `Place Order - \u20B9${displayTotal}`}
            </Button>
          </div>
        </div>
      </div>
    {/if}
  </div>
{/if}
