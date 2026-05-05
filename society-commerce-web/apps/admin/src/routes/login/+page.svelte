<script lang="ts">
  import { getDemoAccountGroups, listDevUsers } from '@society-commerce/api-client';
  import { auth } from '$lib/stores/auth.svelte';
  import { goto } from '$app/navigation';

  let devUsers = $state<Array<{ id: string; name: string; phone: string; roles: string[] }>>([]);
  let useDevUsers = $state(false);

  $effect(() => {
    listDevUsers().then(users => {
      devUsers = users.filter(u => u.roles.includes('admin'));
      useDevUsers = devUsers.length > 0;
    }).catch(() => { useDevUsers = false; });
  });

  const groups = getDemoAccountGroups();
  const adminUsers = groups.find(g => g.role === 'admin')?.users ?? [];

  async function login(identifier: string) {
    await auth.login(identifier);
    if (auth.loggedIn) goto('/');
  }
</script>

<div class="min-h-screen bg-gray-900 flex items-center justify-center p-6">
  <div class="w-full max-w-sm">
    <h1 class="text-2xl font-bold text-white text-center mb-2">SC Admin</h1>
    <p class="text-gray-400 text-center text-sm mb-8">Society Commerce Platform Admin</p>

    <div class="bg-gray-800 rounded-2xl p-6 border border-gray-700">
      <p class="text-xs text-gray-400 uppercase tracking-wider mb-4">
        {useDevUsers ? 'Select account (real auth)' : 'Select admin account'}
      </p>
      {#if auth.loggingIn}
        <div class="flex justify-center py-6">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-400"></div>
        </div>
      {:else if useDevUsers}
        <div class="space-y-3">
          {#each devUsers as devUser}
            <button
              onclick={() => login(devUser.phone)}
              class="w-full flex items-center gap-3 p-3 bg-gray-700/50 rounded-xl hover:bg-gray-700 transition-colors text-left"
            >
              <div class="w-9 h-9 rounded-full bg-purple-600 flex items-center justify-center text-white text-sm font-semibold">
                {devUser.name.split(' ').map((n: string) => n[0]).join('')}
              </div>
              <div>
                <p class="text-white text-sm font-medium">{devUser.name}</p>
                <p class="text-gray-400 text-xs">{devUser.phone}</p>
              </div>
            </button>
          {/each}
        </div>
      {:else}
        <div class="space-y-3">
          {#each adminUsers as user}
            <button
              onclick={() => login(user.id)}
              class="w-full flex items-center gap-3 p-3 bg-gray-700/50 rounded-xl hover:bg-gray-700 transition-colors text-left"
            >
              <div class="w-9 h-9 rounded-full bg-purple-600 flex items-center justify-center text-white text-sm font-semibold">
                {user.name.split(' ').map((n: string) => n[0]).join('')}
              </div>
              <div>
                <p class="text-white text-sm font-medium">{user.name}</p>
                <p class="text-gray-400 text-xs">{user.phone}</p>
              </div>
            </button>
          {/each}
        </div>
      {/if}
    </div>
  </div>
</div>
