<script lang="ts">
  import { page } from "$app/state";

  const items = [
    { href: "/", label: "Home", match: "/" },
    { href: "/routines", label: "Routines", match: "/routines" },
    { href: "/orders", label: "Orders", match: "/orders" },
    { href: "/tab", label: "Tab", match: "/tab" },
  ];

  const path = $derived(page.url.pathname);
</script>

<nav class="fixed bottom-0 left-0 right-0 z-40 frosted safe-bottom">
  <div class="max-w-lg mx-auto grid grid-cols-4">
    {#each items as item (item.href)}
      {@const active = item.match === "/" ? path === "/" : path.startsWith(item.match)}
      <a
        href={item.href}
        class="flex flex-col items-center gap-0.5 py-2.5 text-xs font-medium transition-colors
          {active ? 'text-[var(--c-emerald)]' : 'text-gray-400 hover:text-gray-600'}"
      >
        {#if item.match === "/"}
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-4 0a1 1 0 01-1-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 01-1 1" />
          </svg>
        {:else if item.match === "/routines"}
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
        {:else if item.match === "/orders"}
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
          </svg>
        {:else if item.match === "/tab"}
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M17 9V7a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2m2 4h10a2 2 0 002-2v-6a2 2 0 00-2-2H9a2 2 0 00-2 2v6a2 2 0 002 2zm7-5a2 2 0 11-4 0 2 2 0 014 0z" />
          </svg>
        {/if}
        <span>{item.label}</span>
      </a>
    {/each}
  </div>
</nav>
