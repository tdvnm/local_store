<script lang="ts">
  import { auth } from "$lib/stores/auth.svelte";
  import { cart } from "$lib/stores/cart.svelte";
  import { shopService, productService } from "$lib/api";
  import Header from "$lib/components/Header.svelte";
  import Input from "$lib/components/Input.svelte";
  import Pill from "$lib/components/Pill.svelte";
  import ProductCard from "$lib/components/ProductCard.svelte";
  import CartBar from "$lib/components/CartBar.svelte";
  import type { ProductResponse } from "@society-commerce/api-client";

  let products = $state<ProductResponse[]>([]);
  let categories = $state<string[]>([]);
  let selectedCategory = $state("All");
  let search = $state("");
  let favouriteIds = $state<Set<string>>(new Set());
  let shopName = $state("Store");
  let shopId = $state<string | null>(null);
  let loading = $state(true);

  let loaded = false;

  $effect(() => {
    if (!loaded) {
      loaded = true;
      loadData();
    }
  });

  async function loadData() {
    try {
      const savedFavs = localStorage.getItem("sc_favs");
      if (savedFavs) favouriteIds = new Set(JSON.parse(savedFavs));
    } catch { /* */ }

    try {
      const shops = await shopService.list();
      if (shops.length > 0) {
        shopId = shops[0].id;
        shopName = shops[0].name;
        localStorage.setItem("sc_shop", shopId);
        cart.setShopId(shopId);
      }
    } catch (e) { console.error("fetchShops error:", e); }

    if (shopId) {
      try {
        const [prods] = await Promise.all([
          productService.list(shopId),
          cart.load(shopId),
        ]);
        products = prods;

        const cats = new Set<string>();
        for (const p of prods) cats.add(p.category);
        categories = [...cats].sort();

        if (cart.shopName) shopName = cart.shopName;
      } catch (e) {
        console.error("Failed to load products:", e);
      }
    }
    loading = false;
  }

  const filtered = $derived(
    products.filter(
      (p) =>
        (selectedCategory === "All" || (selectedCategory === "Favourites" ? favouriteIds.has(p.id) : p.category === selectedCategory)) &&
        (!search || p.name.toLowerCase().includes(search.toLowerCase()))
    )
  );

  const cartCount = $derived(cart.count);
  const cartTotal = $derived(cart.total);

  const grouped = $derived.by(() => {
    const g: Record<string, ProductResponse[]> = {};
    for (const p of filtered) {
      if (!g[p.category]) g[p.category] = [];
      g[p.category].push(p);
    }
    return g;
  });

  function addToCartLocal(product: ProductResponse) {
    cart.addItem(product.id, product.name, product.pricePaise, product.inventoryType, product.isAvailable);
  }

  function removeFromCartLocal(id: string) {
    cart.decrementItem(id);
  }

  function getQty(id: string): number {
    return cart.getQty(id);
  }

  function toggleFavourite(productId: string) {
    if (favouriteIds.has(productId)) {
      favouriteIds.delete(productId);
    } else {
      favouriteIds.add(productId);
    }
    favouriteIds = new Set(favouriteIds);
    localStorage.setItem("sc_favs", JSON.stringify([...favouriteIds]));
  }

  function handleLogout() {
    auth.logout();
  }
</script>

<svelte:head>
  <title>{shopName} - Society Commerce</title>
</svelte:head>

{#if loading}
  <div class="min-h-screen flex items-center justify-center">
    <div class="w-8 h-8 border-3 border-[var(--c-emerald)] border-t-transparent rounded-full animate-spin"></div>
  </div>
{:else}
  <div class="min-h-screen">
    <Header>
      <div class="max-w-lg mx-auto px-4 py-3 flex items-center justify-between">
        <h1 class="text-xl font-bold">{shopName}</h1>
        <div class="flex items-center gap-3">
          {#if auth.user}
            <span class="text-sm opacity-80">{auth.user.name}</span>
            <button onclick={handleLogout} class="text-xs opacity-60 hover:opacity-100">Logout</button>
          {/if}
          <a href="/cart" class="relative btn btn-ghost !bg-white !text-[var(--c-emerald)] !border-0 px-4 py-1.5 rounded-full text-sm">
            Cart
            {#if cartCount > 0}
              <span class="absolute -top-2 -right-2 bg-red-500 text-white text-xs w-5 h-5 rounded-full flex items-center justify-center">{cartCount}</span>
            {/if}
          </a>
        </div>
      </div>
    </Header>

    <main class="max-w-lg mx-auto px-4 pb-32">
      <div class="mt-4">
        <Input type="text" placeholder="Search for atta, milk, eggs..." bind:value={search} />
      </div>

      <div class="flex gap-2 mt-4 overflow-x-auto pb-2">
        {#each ["All", "Favourites", ...categories] as cat (cat)}
          <Pill label={cat} active={selectedCategory === cat} onclick={() => (selectedCategory = cat)} />
        {/each}
      </div>

      {#each Object.entries(grouped) as [category, prods] (category)}
        <div class="mt-6 animate-in">
          <h2 class="text-lg font-bold text-gray-800 mb-3">{category}</h2>
          <div class="grid grid-cols-2 gap-3">
            {#each prods as product (product.id)}
              <ProductCard
                {product}
                quantity={getQty(product.id)}
                favourited={favouriteIds.has(product.id)}
                onadd={() => addToCartLocal(product)}
                onremove={() => removeFromCartLocal(product.id)}
                onfavtoggle={() => toggleFavourite(product.id)}
              />
            {/each}
          </div>
        </div>
      {/each}

      {#if filtered.length === 0 && !loading}
        <div class="text-center text-gray-400 mt-12">
          {#if selectedCategory === "Favourites"}
            No favourites yet. Tap the heart on products you love.
          {:else}
            No products found
          {/if}
        </div>
      {/if}
    </main>

    <CartBar count={cartCount} total={cartTotal} />
  </div>
{/if}
