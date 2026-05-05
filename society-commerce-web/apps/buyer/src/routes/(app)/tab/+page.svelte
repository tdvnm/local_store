<script lang="ts">
  import { ledgerService } from "$lib/api";
  import { auth } from "$lib/stores/auth.svelte";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Badge from "$lib/components/Badge.svelte";
  import type { MonthlySummary } from "@society-commerce/api-client";

  let summaries = $state<MonthlySummary[]>([]);
  let selectedMonth = $state<MonthlySummary | null>(null);
  let loading = $state(true);

  let loaded = false;
  $effect(() => {
    if (!loaded) { loaded = true; loadData(); }
  });

  async function loadData() {
    try {
      summaries = await ledgerService.getMonthlySummaries();
      if (summaries.length > 0) selectedMonth = summaries[0];
    } finally { loading = false; }
  }

  function formatPrice(paise: number) { return `₹${(paise / 100).toFixed(0)}`; }
  function formatMonth(str: string) {
    const [y, m] = str.split('-');
    const months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    return `${months[parseInt(m)-1]} ${y}`;
  }
</script>

<svelte:head><title>Monthly Tab - Society Commerce</title></svelte:head>

<div class="min-h-screen pb-24">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3">
      <h1 class="text-xl font-bold">Monthly Tab</h1>
    </div>
  </Header>

  <main class="max-w-lg mx-auto px-4 mt-4 space-y-4">
    {#if loading}
      <div class="flex justify-center py-12">
        <div class="w-8 h-8 border-3 border-[var(--c-emerald)] border-t-transparent rounded-full animate-spin"></div>
      </div>
    {:else if summaries.length === 0}
      <div class="text-center py-12 text-gray-400">
        <p class="text-sm">No billing history yet</p>
      </div>
    {:else}
      <!-- Month selector -->
      <div class="flex gap-2 overflow-x-auto pb-2">
        {#each summaries as s (s.month)}
          <button
            onclick={() => selectedMonth = s}
            class="px-3 py-1.5 rounded-full text-sm whitespace-nowrap transition-all {selectedMonth?.month === s.month ? 'bg-[var(--c-emerald)] text-white font-medium' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}"
          >{formatMonth(s.month)}</button>
        {/each}
      </div>

      {#if selectedMonth}
        <!-- Summary card -->
        <div class="card p-4 bg-gradient-to-r from-emerald-50 to-cyan-50">
          <div class="grid grid-cols-3 gap-4 text-center">
            <div>
              <p class="text-xl font-bold text-gray-900">{formatPrice(selectedMonth.totalPaise)}</p>
              <p class="text-xs text-gray-500">Total</p>
            </div>
            <div>
              <p class="text-xl font-bold text-green-600">{formatPrice(selectedMonth.paidPaise)}</p>
              <p class="text-xs text-gray-500">Paid</p>
            </div>
            <div>
              <p class="text-xl font-bold text-red-600">{formatPrice(selectedMonth.unpaidPaise)}</p>
              <p class="text-xs text-gray-500">Due</p>
            </div>
          </div>
          <p class="text-xs text-gray-400 mt-3 text-center">{selectedMonth.orderCount} orders this month</p>
        </div>

        <!-- Entries -->
        <section>
          <h2 class="text-sm font-semibold text-gray-600 uppercase tracking-wide mb-3">Transactions</h2>
          <div class="space-y-2">
            {#each selectedMonth.entries as entry (entry.id)}
              <a href="/orders/{entry.orderId}" class="card p-3 flex items-center justify-between hover:shadow-md transition-shadow">
                <div>
                  <p class="text-sm font-medium text-gray-900">{entry.orderNumber}</p>
                  <p class="text-xs text-gray-500">{entry.shopName} · {new Date(entry.date).toLocaleDateString()}</p>
                </div>
                <div class="text-right">
                  <p class="text-sm font-semibold">{formatPrice(entry.amountPaise)}</p>
                  {#if entry.status === 'paid'}
                    <Badge color="green">Paid</Badge>
                  {:else}
                    <Badge color="red">Due</Badge>
                  {/if}
                </div>
              </a>
            {/each}
          </div>
        </section>

        <!-- Print button -->
        <button onclick={() => window.print()} class="w-full py-3 text-sm text-gray-500 hover:text-gray-700 border border-gray-200 rounded-xl hover:bg-gray-50 transition-colors">
          Print / Download Summary
        </button>
      {/if}
    {/if}
  </main>
</div>
