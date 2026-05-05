<script lang="ts">
  import { routineService, draftService } from "$lib/api";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import Badge from "$lib/components/Badge.svelte";
  import type { RoutineResponse, DraftOrderResponse } from "@society-commerce/api-client";

  let routines = $state<RoutineResponse[]>([]);
  let drafts = $state<DraftOrderResponse[]>([]);
  let loading = $state(true);

  let loaded = false;
  $effect(() => {
    if (!loaded) { loaded = true; loadData(); }
  });

  async function loadData() {
    try {
      const [r, d] = await Promise.all([routineService.list(), draftService.list()]);
      routines = r;
      drafts = d;
    } finally { loading = false; }
  }

  const frequencyLabel: Record<number, string> = { 0: 'Daily', 1: 'Weekly', 2: 'Monthly' };

  async function togglePause(routine: RoutineResponse) {
    const updated = await routineService.togglePause(routine.id);
    routines = routines.map(r => r.id === updated.id ? updated : r);
  }

  async function confirmDraft(draft: DraftOrderResponse) {
    await draftService.confirm(draft.id);
    drafts = drafts.filter(d => d.id !== draft.id);
  }

  async function dismissDraft(draft: DraftOrderResponse) {
    await draftService.dismiss(draft.id);
    drafts = drafts.filter(d => d.id !== draft.id);
  }

  function formatPrice(paise: number) {
    return `₹${(paise / 100).toFixed(0)}`;
  }
</script>

<svelte:head><title>Routines - Society Commerce</title></svelte:head>

<div class="min-h-screen pb-24">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3">
      <h1 class="text-xl font-bold">Routines</h1>
    </div>
  </Header>

  <main class="max-w-lg mx-auto px-4 mt-4 space-y-6">
    {#if loading}
      <div class="flex justify-center py-12">
        <div class="w-8 h-8 border-3 border-[var(--c-emerald)] border-t-transparent rounded-full animate-spin"></div>
      </div>
    {:else}
      <!-- Pending Drafts -->
      {#if drafts.length > 0}
        <section>
          <h2 class="text-sm font-semibold text-amber-700 uppercase tracking-wide mb-3 flex items-center gap-2">
            <span class="w-2 h-2 bg-amber-500 rounded-full animate-pulse"></span>
            Pending Draft Orders
          </h2>
          {#each drafts as draft (draft.id)}
            {@const items = JSON.parse(draft.itemsSnapshot)}
            <div class="card p-4 mb-3 border-l-4 border-l-amber-400">
              <div class="flex justify-between items-start mb-2">
                <div>
                  <p class="font-semibold text-gray-900">{draft.routineLabel}</p>
                  <p class="text-xs text-gray-500">{draft.shopName} · Scheduled for {new Date(draft.scheduledFor).toLocaleDateString()}</p>
                </div>
                <span class="text-sm font-bold text-gray-900">{formatPrice(draft.estimatedTotalPaise)}</span>
              </div>
              <div class="text-xs text-gray-500 mb-3">
                {items.length} item{items.length !== 1 ? 's' : ''}: {items.map((i: any) => `${i.productName} ×${i.quantity}`).join(', ')}
              </div>
              <div class="flex gap-2">
                <Button variant="primary" size="sm" onclick={() => confirmDraft(draft)}>Place Order</Button>
                <Button variant="ghost" size="sm" onclick={() => dismissDraft(draft)}>Skip</Button>
              </div>
            </div>
          {/each}
        </section>
      {/if}

      <!-- Active Routines -->
      <section>
        <h2 class="text-sm font-semibold text-gray-600 uppercase tracking-wide mb-3">Your Routines</h2>
        {#if routines.length === 0}
          <div class="text-center py-12 text-gray-400">
            <svg class="w-12 h-12 mx-auto mb-3 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            <p class="text-sm">No routines yet</p>
            <p class="text-xs text-gray-300 mt-1">Set up recurring orders for items you buy regularly</p>
          </div>
        {:else}
          {#each routines as routine (routine.id)}
            <div class="card p-4 mb-3">
              <div class="flex justify-between items-start">
                <div class="flex-1">
                  <div class="flex items-center gap-2">
                    <p class="font-semibold text-gray-900">{routine.label}</p>
                    {#if routine.isPaused}
                      <Badge color="yellow">Paused</Badge>
                    {:else}
                      <Badge color="green">{frequencyLabel[routine.frequency] ?? 'Custom'}</Badge>
                    {/if}
                  </div>
                  <p class="text-xs text-gray-500 mt-0.5">{routine.shopName}</p>
                </div>
                <button onclick={() => togglePause(routine)} class="text-xs text-gray-400 hover:text-gray-700 transition-colors">
                  {routine.isPaused ? 'Resume' : 'Pause'}
                </button>
              </div>
              <div class="mt-3 space-y-1">
                {#each routine.items as item}
                  <div class="flex justify-between text-sm">
                    <span class="text-gray-700">{item.productName} ×{item.quantity}</span>
                    <span class="text-gray-500">{formatPrice(item.pricePaise * item.quantity)}</span>
                  </div>
                {/each}
              </div>
              <div class="mt-3 pt-3 border-t border-gray-100 flex justify-between items-center">
                <span class="text-xs text-gray-400">
                  {routine.nextRunAt ? `Next: ${new Date(routine.nextRunAt).toLocaleDateString()}` : 'No schedule'}
                </span>
                <span class="text-sm font-semibold">
                  {formatPrice(routine.items.reduce((s, i) => s + i.pricePaise * i.quantity, 0))}
                </span>
              </div>
            </div>
          {/each}
        {/if}
      </section>
    {/if}
  </main>
</div>
