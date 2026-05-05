<script lang="ts">
  import { goto } from "$app/navigation";
  import { auth } from "$lib/stores/auth.svelte";
  import { getDemoAccountGroups, listDevUsers, isBuyer } from "@society-commerce/api-client";

  let devUsers = $state<Array<{ id: string; name: string; phone: string; roles: string[] }>>([]);
  let useDevUsers = $state(false);
  let error = $state<string | null>(null);

  // Try to load real users from backend
  $effect(() => {
    listDevUsers().then(users => {
      devUsers = users.filter(u => u.roles.includes('flat_owner') || u.roles.includes('household_member'));
      useDevUsers = devUsers.length > 0;
    }).catch(() => {
      useDevUsers = false;
    });
  });

  const groups = getDemoAccountGroups().filter(g =>
    g.role === 'flat_owner' || g.role === 'household_member'
  );

  async function handleLogin(identifier: string) {
    error = null;
    await auth.login(identifier);
    if (auth.loggedIn) {
      goto("/");
    } else {
      error = "Login failed. Is the backend running?";
    }
  }
</script>

<div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-emerald-50 to-cyan-50 p-4">
  <div class="w-full max-w-sm">
    <div class="text-center mb-8">
      <div class="w-16 h-16 bg-gradient-to-br from-emerald-500 to-cyan-500 rounded-2xl mx-auto mb-4 flex items-center justify-center">
        <svg class="w-8 h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 3h2l.4 2M7 13h10l4-8H5.4M7 13L5.4 5M7 13l-2.293 2.293c-.63.63-.184 1.707.707 1.707H17m0 0a2 2 0 100 4 2 2 0 000-4zm-8 2a2 2 0 100 4 2 2 0 000-4z" />
        </svg>
      </div>
      <h1 class="text-2xl font-bold text-gray-900">Society Commerce</h1>
      <p class="text-sm text-gray-500 mt-1">Quick commerce for your society</p>
    </div>

    <div class="bg-white rounded-2xl shadow-lg p-6 space-y-4">
      <h2 class="text-sm font-semibold text-gray-600 uppercase tracking-wide">
        {useDevUsers ? 'Login as' : 'Continue as Demo User'}
      </h2>

      {#if error}
        <p class="text-sm text-red-500 bg-red-50 rounded-lg p-2">{error}</p>
      {/if}

      {#if auth.loggingIn}
        <div class="flex justify-center py-8">
          <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-500"></div>
        </div>
      {:else if useDevUsers}
        <!-- Real backend users -->
        <div class="space-y-2">
          {#each devUsers as devUser}
            <button
              onclick={() => handleLogin(devUser.phone)}
              class="w-full flex items-center gap-3 p-3 rounded-xl border border-gray-100 hover:border-emerald-200 hover:bg-emerald-50 transition-all group"
            >
              <div class="w-10 h-10 rounded-full bg-emerald-100 flex items-center justify-center text-emerald-700 font-semibold text-sm group-hover:bg-emerald-200 transition-colors">
                {devUser.name.split(' ').map((n: string) => n[0]).join('')}
              </div>
              <div class="text-left flex-1">
                <p class="text-sm font-medium text-gray-900">{devUser.name}</p>
                <p class="text-xs text-gray-400">{devUser.phone} · {devUser.roles.join(', ')}</p>
              </div>
              <svg class="w-4 h-4 text-gray-300 group-hover:text-emerald-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
          {/each}
        </div>
      {:else}
        <!-- Fallback to demo users -->
        {#each groups as group}
          <div class="space-y-2">
            <p class="text-xs text-gray-400 font-medium">{group.label}</p>
            {#each group.users as user}
              <button
                onclick={() => handleLogin(user.id)}
                class="w-full flex items-center gap-3 p-3 rounded-xl border border-gray-100 hover:border-emerald-200 hover:bg-emerald-50 transition-all group"
              >
                <div class="w-10 h-10 rounded-full bg-emerald-100 flex items-center justify-center text-emerald-700 font-semibold text-sm group-hover:bg-emerald-200 transition-colors">
                  {user.name.split(' ').map((n: string) => n[0]).join('')}
                </div>
                <div class="text-left flex-1">
                  <p class="text-sm font-medium text-gray-900">{user.name}</p>
                  <p class="text-xs text-gray-400">{user.flatNumber ? `Flat ${user.flatNumber}` : user.phone}</p>
                </div>
                <svg class="w-4 h-4 text-gray-300 group-hover:text-emerald-500 transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
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
