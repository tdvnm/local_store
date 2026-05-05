<script lang="ts">
  import "../app.css";
  import type { Snippet } from "svelte";
  import { page } from "$app/state";
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";
  import { auth } from "$lib/stores/auth.svelte";
  import BottomNav from "$lib/components/BottomNav.svelte";

  let { children }: { children: Snippet } = $props();
  let mounted = $state(false);

  const isLogin = $derived(page.url.pathname === "/login");
  const showNav = $derived(!isLogin && auth.loggedIn);

  onMount(() => {
    mounted = true;
    if (!auth.loggedIn && !isLogin) {
      goto("/login");
    }
  });

  $effect(() => {
    if (mounted && !auth.loggedIn && !isLogin) {
      goto("/login");
    }
  });
</script>

{@render children()}
{#if showNav}
  <BottomNav />
{/if}
