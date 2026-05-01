<script lang="ts">
  import { page } from "$app/state";
  import type { Snippet } from "svelte";
  import Header from "$lib/components/Header.svelte";

  let { children }: { children: Snippet } = $props();

  const tabs = [
    { href: "/admin", label: "Orders" },
    { href: "/admin/products", label: "Products" },
    { href: "/admin/tabs", label: "Tabs" },
  ];
</script>

<div class="min-h-screen">
  <Header>
    <div class="max-w-3xl mx-auto px-4 py-3 flex items-center justify-between">
      <h1 class="text-xl font-bold">Lucky Store Admin</h1>
      <a href="/" class="text-sm text-white/60 hover:text-white transition-colors">View Store &rarr;</a>
    </div>
    <nav class="max-w-3xl mx-auto px-4">
      <div class="flex gap-1">
        {#each tabs as tab (tab.href)}
          <a
            href={tab.href}
            class="px-4 py-2.5 text-sm font-medium rounded-t-lg transition-colors {page.url.pathname === tab.href ? 'bg-[var(--c-bg)] text-gray-900' : 'text-white/50 hover:text-white'}"
          >
            {tab.label}
          </a>
        {/each}
      </div>
    </nav>
  </Header>
  <main class="max-w-3xl mx-auto px-4 py-6">
    {@render children()}
  </main>
</div>
