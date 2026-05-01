<script lang="ts">
  import Button from "./Button.svelte";
  import QtyStepper from "./QtyStepper.svelte";

  interface Product {
    id: number;
    name: string;
    price: number;
    category: string;
    unit: string;
    in_stock: number;
    stock_mode?: string;
    image?: string | null;
  }

  interface Props {
    product: Product;
    quantity: number;
    favourited?: boolean;
    onadd: () => void;
    onremove: () => void;
    onfavtoggle?: () => void;
  }

  let { product, quantity, favourited = false, onadd, onremove, onfavtoggle }: Props = $props();

  const oos = $derived(!product.in_stock);
  const needsConfirmation = $derived(product.stock_mode === "infinite");

  const emojiMap: Record<string, string> = {
    Dairy: "\uD83E\uDD5B",
    Eggs: "\uD83E\uDD5A",
    "Atta & Rice": "\uD83C\uDF3E",
    "Dal & Pulses": "\uD83E\uDED8",
    "Oil & Ghee": "\uD83C\uDFFA",
    Spices: "\uD83C\uDF36\uFE0F",
    Snacks: "\uD83C\uDF5F",
    Beverages: "\uD83E\uDD64",
    Bread: "\uD83C\uDF5E",
    Household: "\uD83E\uDDF9",
  };

  const emoji = $derived(emojiMap[product.category] || "\uD83D\uDED2");
</script>

<div class="card p-3 {oos ? 'opacity-50' : ''} relative">
  {#if onfavtoggle}
    <button
      class="absolute top-2 right-2 w-7 h-7 flex items-center justify-center rounded-full transition-colors
        {favourited ? 'text-red-500' : 'text-gray-300 hover:text-red-300'}"
      onclick={(e) => { e.stopPropagation(); onfavtoggle?.(); }}
    >
      <svg class="w-4 h-4" fill={favourited ? 'currentColor' : 'none'} stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
      </svg>
    </button>
  {/if}

  {#if product.image}
    <div class="h-20 flex items-center justify-center">
      <img src="/products/{product.image}" alt={product.name} class="h-full w-full object-contain rounded" />
    </div>
  {:else}
    <div class="h-14 flex items-center justify-center text-3xl">{emoji}</div>
  {/if}
  <h3 class="text-sm font-medium text-gray-800 leading-tight min-h-[2.5rem] mt-1">{product.name}</h3>
  <div class="flex items-center justify-between mt-1">
    <span class="text-price font-bold">&#8377;{product.price}</span>
    <span class="text-xs text-gray-400">/{product.unit}</span>
  </div>
  {#if needsConfirmation && !oos}
    <div class="text-xs text-amber-600 mt-1 font-medium">Needs confirmation</div>
  {/if}
  <div class="mt-2 h-9">
    {#if oos}
      <div class="h-full flex items-center justify-center text-red-500 text-xs font-medium">Out of Stock</div>
    {:else if quantity === 0}
      <Button variant="ghost" class="w-full h-full text-sm" onclick={onadd}>ADD</Button>
    {:else}
      <QtyStepper {quantity} onincrement={onadd} ondecrement={onremove} class="h-full" />
    {/if}
  </div>
</div>
