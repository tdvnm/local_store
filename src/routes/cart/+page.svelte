<script lang="ts">
  import { onMount } from "svelte";
  import { goto } from "$app/navigation";
  import { auth } from "$lib/stores/auth.svelte";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import QtyStepper from "$lib/components/QtyStepper.svelte";

  type Product = { id: number; name: string; price: number; category: string; unit: string };
  type CartItem = { product: Product; quantity: number };

  let cart = $state<CartItem[]>([]);
  let paymentMethod = $state<"cod" | "tab">("cod");
  let deliveryMode = $state<"urgent" | "scheduled" | "pickup">("urgent");
  let scheduledTime = $state("");
  let placing = $state(false);
  let orderPlaced = $state<{ id: number; total: number } | null>(null);
  let cartLoaded = false;

  onMount(() => {
    const saved = localStorage.getItem("lucky_cart");
    if (saved) { try { cart = JSON.parse(saved); } catch { /* */ } }
    cartLoaded = true;
  });

  $effect(() => {
    if (cartLoaded) localStorage.setItem("lucky_cart", JSON.stringify(cart));
  });

  const total = $derived(cart.reduce((s, i) => s + i.product.price * i.quantity, 0));

  function updateQty(id: number, delta: number) {
    const idx = cart.findIndex((i) => i.product.id === id);
    if (idx === -1) return;
    cart[idx].quantity += delta;
    if (cart[idx].quantity <= 0) cart.splice(idx, 1);
  }

  async function placeOrder() {
    if (!auth.user) { goto("/login"); return; }
    if (cart.length === 0) return;
    if (deliveryMode === "scheduled" && !scheduledTime) { alert("Please select a delivery time"); return; }

    placing = true;
    try {
      const res = await fetch("/api/orders", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          user_id: auth.user.id,
          payment_method: paymentMethod,
          delivery_mode: deliveryMode,
          scheduled_time: deliveryMode === "scheduled" ? scheduledTime : null,
          items: cart.map((i) => ({ product_id: i.product.id, quantity: i.quantity })),
        }),
      });
      const order = await res.json();
      orderPlaced = order;
      cart = [];
      localStorage.removeItem("lucky_cart");
    } catch {
      alert("Failed to place order. Please try again.");
    } finally {
      placing = false;
    }
  }

  const deliveryModes = [
    { value: "urgent", label: "Urgent", desc: "ASAP delivery" },
    { value: "scheduled", label: "Scheduled", desc: "Deliver before a time" },
    { value: "pickup", label: "Pickup", desc: "I'll pick it up" },
  ] as const;
</script>

<svelte:head>
  <title>Cart - Lucky Store</title>
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
      <p class="text-gray-500 mb-1">Order #{orderPlaced.id}</p>
      <p class="text-2xl font-bold text-gray-800 mb-2">&#8377;{orderPlaced.total}</p>
      <p class="text-sm text-gray-400 mb-1">
        {paymentMethod === "cod" ? "Pay when delivered" : "Added to your monthly tab"}
      </p>
      <p class="text-sm text-gray-400 mb-6">
        {deliveryMode === "urgent" ? "We'll deliver ASAP" : deliveryMode === "scheduled" ? `Delivery before ${scheduledTime}` : "Pack & pick up from shop"}
      </p>
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
      {#if cart.length === 0}
        <div class="text-center py-16 animate-in">
          <div class="text-5xl mb-4">&#x1F6D2;</div>
          <p class="text-gray-400 text-lg">Your cart is empty</p>
          <a href="/" class="inline-block mt-4 text-accent font-semibold">Browse products</a>
        </div>
      {:else}
        <div class="space-y-4 animate-in">
          <Card class="divide-y divide-gray-100">
            {#each cart as item (item.product.id)}
              <div class="flex items-center justify-between p-4">
                <div class="flex-1">
                  <h3 class="text-sm font-medium text-gray-800">{item.product.name}</h3>
                  <p class="text-price font-semibold text-sm mt-1">&#8377;{item.product.price} / {item.product.unit}</p>
                </div>
                <div class="flex items-center gap-2">
                  <QtyStepper
                    quantity={item.quantity}
                    onincrement={() => updateQty(item.product.id, 1)}
                    ondecrement={() => updateQty(item.product.id, -1)}
                    class="h-8"
                  />
                  <span class="text-sm font-bold text-gray-700 w-16 text-right">&#8377;{item.product.price * item.quantity}</span>
                </div>
              </div>
            {/each}
          </Card>

          <Card class="p-4">
            <div class="flex justify-between text-lg font-bold">
              <span>Total</span>
              <span class="text-price">&#8377;{total}</span>
            </div>
          </Card>

          <Card class="p-4">
            <h3 class="font-bold text-gray-800 mb-3">Delivery Mode</h3>
            <div class="space-y-2">
              {#each deliveryModes as mode (mode.value)}
                <label class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors {deliveryMode === mode.value ? 'border-[var(--c-cyan)] bg-[#eef1ff]' : 'border-gray-200'}">
                  <input type="radio" name="delivery" checked={deliveryMode === mode.value} onchange={() => (deliveryMode = mode.value)} class="accent-[var(--c-emerald)]" />
                  <div>
                    <div class="font-medium text-sm">{mode.label}</div>
                    <div class="text-xs text-gray-400">{mode.desc}</div>
                  </div>
                </label>
              {/each}
            </div>

            {#if deliveryMode === "scheduled"}
              <div class="mt-3">
                <label class="text-sm text-gray-600 mb-1 block">Deliver before</label>
                <input
                  type="time"
                  bind:value={scheduledTime}
                  class="input"
                />
              </div>
            {/if}
          </Card>

          <Card class="p-4">
            <h3 class="font-bold text-gray-800 mb-3">Payment Method</h3>
            <div class="space-y-2">
              {#each ["cod", "tab"] as m (m)}
                <label class="flex items-center gap-3 p-3 rounded-lg border cursor-pointer transition-colors {paymentMethod === m ? 'border-[var(--c-cyan)] bg-[#eef1ff]' : 'border-gray-200'}">
                  <input type="radio" name="payment" checked={paymentMethod === m} onchange={() => (paymentMethod = m as "cod" | "tab")} class="accent-[var(--c-emerald)]" />
                  <div>
                    <div class="font-medium text-sm">{m === "cod" ? "Cash on Delivery" : "Add to Monthly Tab"}</div>
                    <div class="text-xs text-gray-400">{m === "cod" ? "Pay when you receive" : "Pay at end of month"}</div>
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
                <div>Flat {auth.user.flat_no}</div>
                <div>{auth.user.phone}</div>
              </div>
            </Card>
          {/if}
        </div>
      {/if}
    </main>

    {#if cart.length > 0}
      <div class="fixed bottom-14 left-0 right-0 z-50 animate-slide-up">
        <div class="frosted">
          <div class="max-w-lg mx-auto px-4 py-4">
            <Button variant="primary" class="w-full py-4 rounded-xl text-lg disabled:opacity-50" onclick={placeOrder} disabled={placing}>
              {placing ? "Placing Order..." : `Place Order - \u20B9${total}`}
            </Button>
          </div>
        </div>
      </div>
    {/if}
  </div>
{/if}
