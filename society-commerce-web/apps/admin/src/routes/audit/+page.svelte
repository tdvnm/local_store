<script lang="ts">
  import { adminService } from '@society-commerce/api-client';
  import type { AuditEntry } from '@society-commerce/api-client';

  let entries = $state<AuditEntry[]>([]);
  let loading = $state(true);

  $effect(() => {
    adminService.getAuditLog().then(e => { entries = e; loading = false; });
  });

  const actionColor: Record<string, string> = {
    'user.approve': 'bg-green-100 text-green-700',
    'shop.approve': 'bg-blue-100 text-blue-700',
    'order.override': 'bg-purple-100 text-purple-700',
    'user.deactivate': 'bg-red-100 text-red-700',
    'config.update': 'bg-amber-100 text-amber-700',
  };
</script>

<div class="p-6">
  <h1 class="text-2xl font-bold text-gray-900 mb-6">Audit Log</h1>

  {#if loading}
    <div class="flex justify-center py-12"><div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div></div>
  {:else}
    <div class="bg-white rounded-xl border border-gray-100 overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-gray-50">
          <tr>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Action</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Actor</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Details</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Timestamp</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          {#each entries as entry (entry.id)}
            <tr class="hover:bg-gray-50">
              <td class="px-4 py-3"><span class="px-2 py-0.5 rounded-full text-xs font-medium {actionColor[entry.action] ?? 'bg-gray-100 text-gray-600'}">{entry.action}</span></td>
              <td class="px-4 py-3 text-gray-700">{entry.actorName}</td>
              <td class="px-4 py-3 text-gray-600">{entry.details}</td>
              <td class="px-4 py-3 text-gray-400 text-xs">{new Date(entry.timestamp).toLocaleString()}</td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>
  {/if}
</div>
