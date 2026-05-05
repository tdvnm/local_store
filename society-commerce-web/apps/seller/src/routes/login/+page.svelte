<script lang="ts">
  import { goto } from "$app/navigation";
  import { auth } from "$lib/stores/auth.svelte";
  import { getDemoAccountGroups, listDevUsers } from "@society-commerce/api-client";

  let devUsers = $state<Array<{ id: string; name: string; phone: string; roles: string[] }>>([]);
  let useDevUsers = $state(false);

  $effect(() => {
    listDevUsers().then(users => {
      devUsers = users.filter(u =>
        u.roles.includes('seller_owner') || u.roles.includes('seller_manager') || u.roles.includes('delivery_agent')
      );
      useDevUsers = devUsers.length > 0;
    }).catch(() => { useDevUsers = false; });
  });

  const groups = getDemoAccountGroups().filter(g =>
    g.role === 'seller_owner' || g.role === 'seller_manager' || g.role === 'delivery_agent'
  );

  async function handleLogin(identifier: string) {
    await auth.login(identifier);
    if (auth.loggedIn) goto("/");
  }
</script>

<div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-50 to-indigo-50 p-4">
  <div class="w-full max-w-sm">
    <div class="text-center mb-8">
      <div class="w-16 h-16 bg-gradient-to-br from-purple-500 to-indigo-500 rounded-2xl mx-auto mb-4 flex items-center justify-center">
        <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
        </svg>
      </div>
      <h1 class="text-2xl font-bold text-gray-900">Seller Portal</h1>
      <p class="text-sm text-gray-500 mt-1">Manage your shop on Society Commerce</p>
    </div>

    <div class="bg-white rounded-2xl shadow-lg p-6 space-y-4">
      <h2 class="text-sm font-semibold text-gray-600 uppercase tracking-wide">
        {useDevUsers ? 'Login as' : 'Continue as Demo User'}
      </h2>

      {#if auth.loggingIn}
        <div class="flex justify-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-500"></div>
        </div>
      {:else if useDevUsers}
        <div class="space-y-2">
          {#each devUsers as devUser}
            <button
              onclick={() => handleLogin(devUser.phone)}
              class="w-full flex items-center gap-3 p-3 rounded-xl border border-gray-100 hover:border-purple-200 hover:bg-purple-50 transition-all group"
            >
              <div class="w-10 h-10 rounded-full bg-purple-100 flex items-center justify-center text-purple-700 font-semibold text-sm group-hover:bg-purple-200 transition-colors">
                {devUser.name.split(' ').map((n: string) => n[0]).join('')}
              </div>
              <div class="text-left flex-1">
                <p class="text-sm font-medium text-gray-900">{devUser.name}</p>
                <p class="text-xs text-gray-400">{devUser.phone} · {devUser.roles.join(', ')}</p>
              </div>
              <svg class="w-4 h-4 text-gray-300 group-hover:text-purple-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
          {/each}
        </div>
      {:else}
        {#each groups as group}
          <div class="space-y-2">
            <p class="text-xs text-gray-400 font-medium">{group.label}</p>
            {#each group.users as user}
              <button
                onclick={() => handleLogin(user.id)}
                class="w-full flex items-center gap-3 p-3 rounded-xl border border-gray-100 hover:border-purple-200 hover:bg-purple-50 transition-all group"
              >
                <div class="w-10 h-10 rounded-full bg-purple-100 flex items-center justify-center text-purple-700 font-semibold text-sm group-hover:bg-purple-200 transition-colors">
                  {user.name.split(' ').map((n: string) => n[0]).join('')}
                </div>
                <div class="text-left flex-1">
                  <p class="text-sm font-medium text-gray-900">{user.name}</p>
                  <p class="text-xs text-gray-400">{user.roles.join(', ').replace(/_/g, ' ')}</p>
                </div>
                <svg class="w-4 h-4 text-gray-300 group-hover:text-purple-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
                </svg>
              </button>
            {/each}
          </div>
        {/each}
      {/if}
    </div>

    <p class="text-center text-xs text-gray-400 mt-4">
      {useDevUsers ? 'Dev mode · Real backend auth' : 'Demo mode · No real authentication'}
    </p>
  </div>
</div>
