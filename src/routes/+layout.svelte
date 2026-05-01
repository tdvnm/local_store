<script lang="ts">
  import "../app.css";
  import type { Snippet } from "svelte";
  import { page } from "$app/state";
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import { auth } from "$lib/stores/auth.svelte";
  import BottomNav from "$lib/components/BottomNav.svelte";

  let { children }: { children: Snippet } = $props();
  let ready = $state(false);

  const isLogin = $derived(page.url.pathname === "/login");
  const isAdmin = $derived(page.url.pathname.startsWith("/admin"));
  const showNav = $derived(!isLogin && !isAdmin && auth.loggedIn);

  onMount(() => {
    ready = true;
  });

  $effect(() => {
    if (ready && !auth.loggedIn && !isLogin && !isAdmin) {
      goto("/login");
    }
  });
</script>

{#if !ready}
  <div class="min-h-screen flex items-center justify-center">
    <div class="w-8 h-8 border-3 border-[var(--c-emerald)] border-t-transparent rounded-full animate-spin"></div>
  </div>
{:else}
  {@render children()}
  {#if showNav}
    <BottomNav />
  {/if}
{/if}
