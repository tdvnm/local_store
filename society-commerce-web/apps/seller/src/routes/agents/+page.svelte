<script lang="ts">
  import { auth } from "$lib/stores/auth.svelte";
  import { agentService } from "@society-commerce/api-client";
  import type { AgentResponse } from "@society-commerce/api-client";

  let agents = $state<AgentResponse[]>([]);
  let loading = $state(true);

  let loaded = false;
  $effect(() => {
    if (!loaded && auth.shopId) { loaded = true; loadAgents(); }
  });

  async function loadAgents() {
    try {
      agents = await agentService.list(auth.shopId!);
    } finally { loading = false; }
  }

  async function toggleActive(agent: AgentResponse) {
    const updated = await agentService.updateStatus(agent.id, !agent.isActive);
    agents = agents.map(a => a.id === updated.id ? updated : a);
  }
</script>

<div class="p-6">
  <div class="flex items-center justify-between mb-6">
    <h1 class="text-2xl font-bold text-gray-900">Delivery Agents</h1>
    <span class="text-sm text-gray-400">{agents.length} agents</span>
  </div>

  {#if loading}
    <div class="flex justify-center py-12">
      <div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
    </div>
  {:else if agents.length === 0}
    <div class="bg-white rounded-xl p-12 text-center border border-gray-100">
      <p class="text-gray-400 text-sm">No delivery agents registered</p>
    </div>
  {:else}
    <div class="grid gap-4 md:grid-cols-2">
      {#each agents as agent (agent.id)}
        <div class="bg-white rounded-xl p-5 border border-gray-100">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-full bg-purple-100 flex items-center justify-center text-purple-700 font-semibold text-sm">
                {agent.name.split(' ').map(n => n[0]).join('')}
              </div>
              <div>
                <p class="font-medium text-gray-900">{agent.name}</p>
                <p class="text-xs text-gray-500">{agent.phone}</p>
              </div>
            </div>
            <button
              onclick={() => toggleActive(agent)}
              class="px-3 py-1 rounded-full text-xs font-medium transition-colors {agent.isActive ? 'bg-green-100 text-green-700 hover:bg-green-200' : 'bg-gray-100 text-gray-500 hover:bg-gray-200'}"
            >
              {agent.isActive ? 'Active' : 'Inactive'}
            </button>
          </div>
          <div class="mt-3 pt-3 border-t border-gray-50 flex justify-between text-sm">
            <span class="text-gray-500">Active deliveries</span>
            <span class="font-medium text-gray-900">{agent.activeDeliveries}</span>
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>
