<script lang="ts">
  import { onMount } from "svelte";
  import { auth } from "$lib/stores/auth.svelte";
  import Header from "$lib/components/Header.svelte";
  import Input from "$lib/components/Input.svelte";
  import Pill from "$lib/components/Pill.svelte";
  import ProductCard from "$lib/components/ProductCard.svelte";
  import CartBar from "$lib/components/CartBar.svelte";

  type Product = { id: number; name: string; price: number; category: string; unit: string; in_stock: number; stock_mode?: string };
  type CartItem = { product: Product; quantity: number };

  let products = $state<Product[]>([]);
  let categories = $state<string[]>([]);
  let selectedCategory = $state("All");
  let search = $state("");
  let cart = $state<CartItem[]>([]);
  let favouriteIds = $state<Set<number>>(new Set());
  let cartLoaded = false;

  onMount(() => {
    const saved = localStorage.getItem("lucky_cart");
    if (saved) { try { cart = JSON.parse(saved); } catch { /* */ } }
    cartLoaded = true;

    fetch("/api/categories").then((r) => r.json()).then((data) => (categories = data));
    fetch("/api/products").then((r) => r.json()).then((data) => (products = data));

    if (auth.user) {
      fetch(`/api/favourites?user_id=${auth.user.id}`).then((r) => r.json()).then((favs: { product_id: number }[]) => {
        favouriteIds = new Set(favs.map((f) => f.product_id));
      });
    }
  });

  $effect(() => {
    if (cartLoaded) localStorage.setItem("lucky_cart", JSON.stringify(cart));
  });

  const filtered = $derived(
    products.filter(
      (p) =>
        (selectedCategory === "All" || (selectedCategory === "Favourites" ? favouriteIds.has(p.id) : p.category === selectedCategory)) &&
        (!search || p.name.toLowerCase().includes(search.toLowerCase()))
    )
  );

  const cartCount = $derived(cart.reduce((s, i) => s + i.quantity, 0));
  const cartTotal = $derived(cart.reduce((s, i) => s + i.product.price * i.quantity, 0));

  const grouped = $derived.by(() => {
    const g: Record<string, Product[]> = {};
    for (const p of filtered) {
      if (!g[p.category]) g[p.category] = [];
      g[p.category].push(p);
    }
    return g;
  });

  function addToCart(product: Product) {
    const existing = cart.find((i) => i.product.id === product.id);
    if (existing) {
      existing.quantity++;
    } else {
      cart.push({ product, quantity: 1 });
    }
  }

  function removeFromCart(id: number) {
    const idx = cart.findIndex((i) => i.product.id === id);
    if (idx === -1) return;
    if (cart[idx].quantity > 1) {
      cart[idx].quantity--;
    } else {
      cart.splice(idx, 1);
    }
  }

  function getQty(id: number): number {
    return cart.find((i) => i.product.id === id)?.quantity || 0;
  }

  async function toggleFavourite(productId: number) {
    if (!auth.user) return;
    const isFav = favouriteIds.has(productId);

    if (isFav) {
      favouriteIds.delete(productId);
      favouriteIds = new Set(favouriteIds);
      await fetch("/api/favourites", {
        method: "DELETE",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ user_id: auth.user.id, product_id: productId }),
      });
    } else {
      favouriteIds.add(productId);
      favouriteIds = new Set(favouriteIds);
      await fetch("/api/favourites", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ user_id: auth.user.id, product_id: productId }),
      });
    }
  }

  function handleLogout() {
    auth.logout();
  }
</script>

<svelte:head>
  <title>Lucky Store</title>
</svelte:head>

<div class="min-h-screen">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3 flex items-center justify-between">
      <h1 class="text-xl font-bold">Lucky Store</h1>
      <div class="flex items-center gap-3">
        {#if auth.user}
          <span class="text-sm opacity-80">{auth.user.flat_no}</span>
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
              onadd={() => addToCart(product)}
              onremove={() => removeFromCart(product.id)}
              onfavtoggle={() => toggleFavourite(product.id)}
            />
          {/each}
        </div>
      </div>
    {/each}

    {#if filtered.length === 0}
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
