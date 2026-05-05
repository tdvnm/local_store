<script lang="ts">
  import { adminService } from '@society-commerce/api-client';
  import type { SupportTicket } from '@society-commerce/api-client';

  let tickets = $state<SupportTicket[]>([]);
  let loading = $state(true);

  $effect(() => {
    adminService.getAllTickets().then(t => { tickets = t; loading = false; });
  });

  const statusColor: Record<string, string> = {
    open: 'bg-red-100 text-red-700', in_progress: 'bg-amber-100 text-amber-700',
    resolved: 'bg-green-100 text-green-700', closed: 'bg-gray-100 text-gray-600',
  };
  const typeColor: Record<string, string> = {
    dispute: 'bg-red-50 text-red-600', bug: 'bg-purple-50 text-purple-600', general: 'bg-blue-50 text-blue-600',
  };
</script>

<div class="p-6">
  <div class="flex items-center justify-between mb-6">
    <h1 class="text-2xl font-bold text-gray-900">Support Tickets</h1>
    <span class="text-sm text-gray-400">{tickets.length} tickets</span>
  </div>

  {#if loading}
    <div class="flex justify-center py-12"><div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div></div>
  {:else}
    <div class="space-y-3">
      {#each tickets as ticket (ticket.id)}
        <div class="bg-white rounded-xl p-5 border border-gray-100">
          <div class="flex items-start justify-between">
            <div class="flex-1">
              <div class="flex items-center gap-2 mb-1">
                <span class="px-2 py-0.5 rounded-full text-xs font-medium {typeColor[ticket.type]}">{ticket.type}</span>
                <span class="px-2 py-0.5 rounded-full text-xs font-medium {statusColor[ticket.status]}">{ticket.status.replace('_', ' ')}</span>
              </div>
              <h3 class="font-semibold text-gray-900">{ticket.subject}</h3>
              <p class="text-sm text-gray-600 mt-1">{ticket.description}</p>
              <div class="flex items-center gap-4 mt-2 text-xs text-gray-400">
                <span>By: {ticket.createdByName}</span>
                {#if ticket.orderId}<span>Order: {ticket.orderId}</span>{/if}
                <span>{new Date(ticket.createdAt).toLocaleDateString()}</span>
              </div>
            </div>
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>
