<script lang="ts">
  import { onMount } from "svelte";
  import { auth } from "$lib/stores/auth.svelte";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";

  type TabOrder = {
    id: number;
    total: number;
    status: string;
    payment_method: string;
    created_at: string;
    items: { product_name: string; quantity: number; price: number }[];
  };

  let tabTotal = $state(0);
  let tabPaid = $state(0);
  let tabOrders = $state<TabOrder[]>([]);
  let loading = $state(true);

  const balance = $derived(tabTotal - tabPaid);

  onMount(async () => {
    if (!auth.user) return;
    try {
      const [tabRes, ordersRes] = await Promise.all([
        fetch("/api/tabs"),
        fetch(`/api/orders?user_id=${auth.user.id}`),
      ]);

      const tabs = await tabRes.json();
      const myTab = tabs.find((t: any) => t.user_id === auth.user!.id);
      if (myTab) {
        tabTotal = myTab.total_tab;
        tabPaid = myTab.total_paid;
      }

      const allOrders: TabOrder[] = await ordersRes.json();
      tabOrders = allOrders.filter((o) => o.payment_method === "tab" && o.status !== "cancelled");
    } finally {
      loading = false;
    }
  });

  function formatDate(dateStr: string): string {
    const d = new Date(dateStr + "Z");
    return d.toLocaleDateString([], { day: "numeric", month: "short" });
  }
</script>

<svelte:head>
  <title>Monthly Tab - Lucky Store</title>
</svelte:head>

<div class="min-h-screen">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3">
      <h1 class="text-xl font-bold">Monthly Tab</h1>
    </div>
  </Header>

  <main class="max-w-lg mx-auto px-4 py-4 pb-24">
    {#if loading}
      <div class="text-center text-gray-400 py-12">Loading...</div>
    {:else}
      <div class="space-y-4 animate-in">
        <!-- Balance card -->
        <Card class="p-6 text-center">
          <p class="text-sm text-gray-400 mb-1">Outstanding Balance</p>
          <p class="text-4xl font-bold {balance > 0 ? 'text-[var(--c-emerald)]' : 'text-gray-800'}">
            &#8377;{balance}
          </p>
          <div class="flex justify-center gap-6 mt-4 text-sm">
            <div>
              <span class="text-gray-400">Total Tab</span>
              <div class="font-bold text-gray-700">&#8377;{tabTotal}</div>
            </div>
            <div class="w-px bg-gray-200"></div>
            <div>
              <span class="text-gray-400">Paid</span>
              <div class="font-bold text-paid">&#8377;{tabPaid}</div>
            </div>
          </div>
        </Card>

        <!-- User info -->
        {#if auth.user}
          <Card class="p-4">
            <div class="flex items-center justify-between">
              <div>
                <div class="font-bold text-gray-800">{auth.user.name}</div>
                <div class="text-sm text-gray-500">Flat {auth.user.flat_no}</div>
              </div>
              <div class="text-sm text-gray-400">{auth.user.phone}</div>
            </div>
          </Card>
        {/if}

        <!-- Tab orders -->
        {#if tabOrders.length > 0}
          <div>
            <h2 class="text-sm font-bold text-gray-500 uppercase tracking-wider mb-3">Tab Orders</h2>
            <Card class="divide-y divide-gray-100">
              {#each tabOrders as order (order.id)}
                <a href="/orders/{order.id}" class="flex items-center justify-between p-4 hover:bg-gray-50 transition-colors">
                  <div>
                    <div class="flex items-center gap-2">
                      <span class="text-sm font-medium text-gray-800">#{order.id}</span>
                      <span class="text-xs text-gray-400">{formatDate(order.created_at)}</span>
                    </div>
                    <p class="text-xs text-gray-400 mt-0.5">
                      {order.items.length} item{order.items.length > 1 ? "s" : ""}
                    </p>
                  </div>
                  <span class="font-bold text-sm text-gray-700">&#8377;{order.total}</span>
                </a>
              {/each}
            </Card>
          </div>
        {:else}
          <div class="text-center text-gray-400 py-8">
            <p>No tab orders yet</p>
            <p class="text-xs mt-1">Choose "Monthly Tab" at checkout</p>
          </div>
        {/if}
      </div>
    {/if}
  </main>
</div>
