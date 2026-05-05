<script lang="ts">
  import { adminService } from '@society-commerce/api-client';

  let users = $state<any[]>([]);
  let shops = $state<any[]>([]);
  let loading = $state(true);
  let tab = $state<'users' | 'shops'>('users');

  $effect(() => {
    Promise.all([adminService.getAllUsers(), adminService.getAllShops()]).then(([u, s]) => {
      users = u;
      shops = s;
      loading = false;
    });
  });
</script>

<div class="p-6">
  <h1 class="text-2xl font-bold text-gray-900 mb-6">Approvals & Management</h1>

  <div class="flex gap-2 mb-4">
    <button onclick={() => tab = 'users'} class="px-4 py-2 rounded-lg text-sm {tab === 'users' ? 'bg-purple-100 text-purple-800 font-medium' : 'text-gray-500 hover:bg-gray-100'}">Users ({users.length})</button>
    <button onclick={() => tab = 'shops'} class="px-4 py-2 rounded-lg text-sm {tab === 'shops' ? 'bg-purple-100 text-purple-800 font-medium' : 'text-gray-500 hover:bg-gray-100'}">Shops ({shops.length})</button>
  </div>

  {#if loading}
    <div class="flex justify-center py-12"><div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div></div>
  {:else if tab === 'users'}
    <div class="bg-white rounded-xl border border-gray-100 overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-gray-50">
          <tr>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Name</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Phone</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Flat</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Roles</th>
            <th class="text-center px-4 py-3 font-medium text-gray-600">Status</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          {#each users as user (user.id)}
            <tr class="hover:bg-gray-50">
              <td class="px-4 py-3 font-medium text-gray-900">{user.name}</td>
              <td class="px-4 py-3 text-gray-600">{user.phone}</td>
              <td class="px-4 py-3 text-gray-600">{user.flatNumber ?? '—'}</td>
              <td class="px-4 py-3"><span class="px-2 py-0.5 bg-purple-50 text-purple-700 rounded-full text-xs">{user.roles.join(', ')}</span></td>
              <td class="px-4 py-3 text-center"><span class="w-2 h-2 rounded-full inline-block {user.isActive ? 'bg-green-500' : 'bg-red-500'}"></span></td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>
  {:else}
    <div class="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      {#each shops as shop (shop.id)}
        <div class="bg-white rounded-xl p-5 border border-gray-100">
          <div class="flex justify-between items-start">
            <div>
              <h3 class="font-semibold text-gray-900">{shop.name}</h3>
              <p class="text-xs text-gray-500 mt-1">{shop.category}</p>
            </div>
            <span class="px-2 py-0.5 rounded-full text-xs {shop.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'}">{shop.isActive ? 'Active' : 'Inactive'}</span>
          </div>
          <p class="text-sm text-gray-600 mt-2">{shop.description ?? 'No description'}</p>
        </div>
      {/each}
    </div>
  {/if}
</div>
