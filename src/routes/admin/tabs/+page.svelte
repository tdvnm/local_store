<script lang="ts">
  import { onMount } from "svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import Input from "$lib/components/Input.svelte";

  type Tab = { user_id: number; name: string; flat_no: string; phone: string; total_tab: number; total_paid: number; balance: number };

  let tabs = $state<Tab[]>([]);
  let payingUser = $state<number | null>(null);
  let payAmount = $state("");

  function fetchTabs() {
    fetch("/api/tabs").then((r) => r.json()).then((data) => (tabs = data));
  }

  onMount(fetchTabs);

  async function recordPayment(userId: number) {
    const amount = parseFloat(payAmount);
    if (!amount || amount <= 0) { alert("Enter a valid amount"); return; }
    await fetch("/api/tabs", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ user_id: userId, amount }) });
    payingUser = null;
    payAmount = "";
    fetchTabs();
  }

  const totalOwed = $derived(tabs.reduce((s, t) => s + Math.max(0, t.balance), 0));
</script>

<svelte:head>
  <title>Tabs - Lucky Store Admin</title>
</svelte:head>

<div>
  <div class="flex items-center justify-between mb-6">
    <h2 class="text-xl font-bold text-gray-800">Monthly Tabs</h2>
    <div class="text-right">
      <div class="text-sm text-gray-400">Total Outstanding</div>
      <div class="text-2xl font-bold text-red-600">&#8377;{totalOwed}</div>
    </div>
  </div>

  {#if tabs.length === 0}
    <div class="text-center text-gray-400 py-12">No tabs yet. Tabs appear when users choose "Add to Monthly Tab" payment.</div>
  {/if}

  <div class="space-y-3">
    {#each tabs as tab (tab.user_id)}
      <Card class="p-4">
        <div class="flex items-start justify-between">
          <div>
            <div class="font-bold text-gray-800">{tab.name}</div>
            <div class="text-sm text-gray-500">{tab.flat_no} &middot; {tab.phone}</div>
          </div>
          <div class="text-right">
            <div class="text-xl font-bold {tab.balance > 0 ? 'text-red-600' : 'text-paid'}">&#8377;{tab.balance}</div>
            <div class="text-xs text-gray-400">Total: &#8377;{tab.total_tab} | Paid: &#8377;{tab.total_paid}</div>
          </div>
        </div>
        {#if tab.balance > 0}
          <div class="mt-3">
            {#if payingUser === tab.user_id}
              <div class="flex gap-2">
                <Input type="number" placeholder="Amount" bind:value={payAmount} class="flex-1 !py-2" />
                <Button variant="primary" class="px-4 py-2" onclick={() => recordPayment(tab.user_id)}>Record</Button>
                <Button variant="muted" class="px-4 py-2" onclick={() => { payingUser = null; payAmount = ""; }}>Cancel</Button>
              </div>
            {:else}
              <Button variant="ghost" class="w-full py-2" onclick={() => { payingUser = tab.user_id; payAmount = String(tab.balance); }}>Record Payment</Button>
            {/if}
          </div>
        {/if}
      </Card>
    {/each}
  </div>
</div>
