<script lang="ts">
  import { onMount } from "svelte";
  import { auth } from "$lib/stores/auth.svelte";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import Input from "$lib/components/Input.svelte";

  type Product = { id: number; name: string; price: number; category: string; unit: string };
  type Routine = {
    id: number;
    product_id: number;
    product_name: string;
    price: number;
    unit: string;
    quantity: number;
    frequency: string;
    active: number;
  };

  let routines = $state<Routine[]>([]);
  let products = $state<Product[]>([]);
  let loading = $state(true);
  let showAdd = $state(false);

  let searchQuery = $state("");
  let selectedProduct = $state<Product | null>(null);
  let newQty = $state(1);
  let newFreq = $state("daily");
  let saving = $state(false);

  const frequencies = [
    { value: "daily", label: "Daily" },
    { value: "every_2_days", label: "Every 2 Days" },
    { value: "every_3_days", label: "Every 3 Days" },
    { value: "weekly", label: "Weekly" },
  ];

  const freqLabels: Record<string, string> = Object.fromEntries(frequencies.map((f) => [f.value, f.label]));

  onMount(async () => {
    if (!auth.user) return;
    try {
      const [routinesRes, productsRes] = await Promise.all([
        fetch(`/api/routines?user_id=${auth.user.id}`),
        fetch("/api/products"),
      ]);
      routines = await routinesRes.json();
      products = await productsRes.json();
    } finally {
      loading = false;
    }
  });

  const filteredProducts = $derived(
    searchQuery.trim()
      ? products.filter((p) => p.name.toLowerCase().includes(searchQuery.toLowerCase())).slice(0, 5)
      : []
  );

  function selectProduct(p: Product) {
    selectedProduct = p;
    searchQuery = p.name;
  }

  async function addRoutine() {
    if (!auth.user || !selectedProduct) return;
    saving = true;
    try {
      const res = await fetch("/api/routines", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          user_id: auth.user.id,
          product_id: selectedProduct.id,
          quantity: newQty,
          frequency: newFreq,
        }),
      });
      const routine = await res.json();
      routines = [routine, ...routines];
      showAdd = false;
      searchQuery = "";
      selectedProduct = null;
      newQty = 1;
      newFreq = "daily";
    } finally {
      saving = false;
    }
  }

  async function toggleRoutine(routine: Routine) {
    const newActive = routine.active ? 0 : 1;
    routine.active = newActive;
    routines = [...routines];
    await fetch(`/api/routines/${routine.id}`, {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ active: newActive }),
    });
  }

  async function deleteRoutine(id: number) {
    routines = routines.filter((r) => r.id !== id);
    await fetch(`/api/routines/${id}`, { method: "DELETE" });
  }
</script>

<svelte:head>
  <title>Routines - Lucky Store</title>
</svelte:head>

<div class="min-h-screen">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3 flex items-center justify-between">
      <h1 class="text-xl font-bold">Routines</h1>
      {#if !showAdd}
        <button
          class="text-sm font-semibold opacity-80 hover:opacity-100"
          onclick={() => (showAdd = true)}
        >
          + Add
        </button>
      {/if}
    </div>
  </Header>

  <main class="max-w-lg mx-auto px-4 py-4 pb-24">
    {#if loading}
      <div class="text-center text-gray-400 py-12">Loading...</div>
    {:else}
      <div class="space-y-4">
        <!-- Add routine form -->
        {#if showAdd}
          <Card class="p-4 animate-in">
            <h3 class="font-bold text-gray-800 mb-3">New Routine</h3>
            <div class="space-y-3">
              <div class="relative">
                <Input type="text" placeholder="Search product..." bind:value={searchQuery} />
                {#if filteredProducts.length > 0 && !selectedProduct}
                  <div class="absolute z-10 left-0 right-0 mt-1 bg-white rounded-lg shadow-lg border border-gray-100 overflow-hidden">
                    {#each filteredProducts as p (p.id)}
                      <button
                        class="w-full text-left px-4 py-2.5 hover:bg-gray-50 text-sm flex justify-between items-center"
                        onclick={() => selectProduct(p)}
                      >
                        <span>{p.name}</span>
                        <span class="text-xs text-gray-400">&#8377;{p.price}/{p.unit}</span>
                      </button>
                    {/each}
                  </div>
                {/if}
              </div>

              {#if selectedProduct}
                <div class="flex gap-3">
                  <div class="flex-1">
                    <label class="text-xs text-gray-500 mb-1 block">Quantity</label>
                    <input type="number" min="1" bind:value={newQty} class="input" />
                  </div>
                  <div class="flex-1">
                    <label class="text-xs text-gray-500 mb-1 block">Frequency</label>
                    <select bind:value={newFreq} class="input">
                      {#each frequencies as f (f.value)}
                        <option value={f.value}>{f.label}</option>
                      {/each}
                    </select>
                  </div>
                </div>

                <div class="flex gap-2">
                  <Button variant="primary" class="flex-1 py-2.5" onclick={addRoutine} disabled={saving}>
                    {saving ? "Adding..." : "Add Routine"}
                  </Button>
                  <Button variant="ghost" class="px-4 py-2.5" onclick={() => { showAdd = false; searchQuery = ''; selectedProduct = null; }}>
                    Cancel
                  </Button>
                </div>
              {/if}
            </div>
          </Card>
        {/if}

        <!-- Routines list -->
        {#if routines.length === 0 && !showAdd}
          <div class="text-center py-16 animate-in">
            <svg class="w-12 h-12 mx-auto text-gray-300 mb-3" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <p class="text-gray-400">No routines yet</p>
            <p class="text-xs text-gray-400 mt-1">Set up recurring orders for daily essentials</p>
            <button class="mt-4 text-accent font-semibold text-sm" onclick={() => (showAdd = true)}>
              Add your first routine
            </button>
          </div>
        {:else}
          {#each routines as routine (routine.id)}
            <Card class="p-4 animate-in {routine.active ? '' : 'opacity-50'}">
              <div class="flex items-start justify-between">
                <div class="flex-1">
                  <h3 class="font-medium text-gray-800 text-sm">{routine.product_name}</h3>
                  <div class="flex items-center gap-2 mt-1">
                    <span class="text-xs text-gray-400">Qty: {routine.quantity}</span>
                    <span class="text-xs text-gray-300">|</span>
                    <span class="text-xs text-gray-400">{freqLabels[routine.frequency] || routine.frequency}</span>
                    <span class="text-xs text-gray-300">|</span>
                    <span class="text-xs text-price font-medium">&#8377;{routine.price * routine.quantity}/{routine.unit}</span>
                  </div>
                </div>
                <div class="flex items-center gap-2">
                  <button
                    class="relative w-10 h-6 rounded-full transition-colors {routine.active ? 'bg-[var(--c-emerald)]' : 'bg-gray-300'}"
                    onclick={() => toggleRoutine(routine)}
                  >
                    <span class="absolute top-0.5 {routine.active ? 'right-0.5' : 'left-0.5'} w-5 h-5 bg-white rounded-full shadow transition-all"></span>
                  </button>
                  <button class="text-gray-400 hover:text-red-500 p-1" onclick={() => deleteRoutine(routine.id)}>
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                    </svg>
                  </button>
                </div>
              </div>
            </Card>
          {/each}
        {/if}
      </div>
    {/if}
  </main>
</div>
