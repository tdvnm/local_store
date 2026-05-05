<script lang="ts">
  import "../app.css";
  import type { Snippet } from "svelte";
  import { page } from "$app/state";
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import { auth } from "$lib/stores/auth.svelte";

  let { children }: { children: Snippet } = $props();
  let mounted = $state(false);

  const isLogin = $derived(page.url.pathname === "/login");
  const showShell = $derived(!isLogin && auth.loggedIn);

  onMount(() => {
    mounted = true;
    if (!auth.loggedIn && !isLogin) goto("/login");
  });

  $effect(() => {
    if (mounted && !auth.loggedIn && !isLogin) goto("/login");
  });

  const nav = [
    { href: "/", label: "Dashboard", icon: "M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-4 0a1 1 0 01-1-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 01-1 1" },
    { href: "/orders", label: "Orders", icon: "M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" },
    { href: "/catalog", label: "Catalog", icon: "M20 7l-8-4-8 4m16 0l-8 4m8-4v10l-8 4m0-10L4 7m8 4v10M4 7v10l8 4" },
    { href: "/agents", label: "Agents", icon: "M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z" },
    { href: "/settings", label: "Settings", icon: "M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.066 2.573c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.573 1.066c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.066-2.573c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z M15 12a3 3 0 11-6 0 3 3 0 016 0z" },
  ];
</script>

{#if !showShell}
  {@render children()}
{:else}
  <div class="flex min-h-screen bg-gray-50">
    <!-- Sidebar -->
    <aside class="w-56 bg-white border-r border-gray-100 flex flex-col">
      <div class="p-4 border-b border-gray-100">
        <h1 class="font-bold text-lg text-purple-700">SC Seller</h1>
        <p class="text-xs text-gray-400 mt-0.5">{auth.user?.name ?? ''}</p>
      </div>
      <nav class="flex-1 p-2 space-y-0.5">
        {#each nav as item (item.href)}
          {@const active = item.href === "/" ? page.url.pathname === "/" : page.url.pathname.startsWith(item.href)}
          <a
            href={item.href}
            class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm transition-colors
              {active ? 'bg-purple-50 text-purple-700 font-medium' : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900'}"
          >
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="1.5">
              <path stroke-linecap="round" stroke-linejoin="round" d={item.icon} />
            </svg>
            {item.label}
          </a>
        {/each}
      </nav>
      <div class="p-3 border-t border-gray-100">
        <button onclick={() => auth.logout()} class="text-xs text-gray-400 hover:text-red-500 transition-colors">Logout</button>
      </div>
    </aside>

    <!-- Main -->
    <main class="flex-1 overflow-y-auto">
      {@render children()}
    </main>
  </div>
{/if}
