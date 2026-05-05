<script lang="ts">
  import { auth } from "$lib/stores/auth.svelte";
  import { productService, shopService } from "@society-commerce/api-client";
  import type { ProductResponse } from "@society-commerce/api-client";

  let products = $state<ProductResponse[]>([]);
  let categories = $state<string[]>([]);
  let selectedCategory = $state("All");
  let search = $state("");
  let editingProduct = $state<ProductResponse | null>(null);
  let showForm = $state(false);
  let loading = $state(true);

  let loaded = false;
  $effect(() => {
    if (!loaded && auth.shopId) { loaded = true; loadCatalog(); }
  });

  async function loadCatalog() {
    try {
      const [prods, cats] = await Promise.all([
        productService.list(auth.shopId!),
        shopService.getCategories(auth.shopId!),
      ]);
      products = prods;
      categories = cats;
    } finally { loading = false; }
  }

  const filtered = $derived.by(() => {
    let result = products;
    if (selectedCategory !== "All") result = result.filter(p => p.category === selectedCategory);
    if (search) {
      const q = search.toLowerCase();
      result = result.filter(p => p.name.toLowerCase().includes(q));
    }
    return result;
  });

  const lowStock = $derived(products.filter(p => p.inventoryType === 1 && (p.stockQuantity ?? 0) < 5));

  function formatPrice(paise: number) { return `₹${(paise / 100).toFixed(2)}`; }

  function editProduct(p: ProductResponse) {
    editingProduct = { ...p };
    showForm = true;
  }

  function closeForm() {
    editingProduct = null;
    showForm = false;
  }
</script>

<div class="p-6">
  <div class="flex items-center justify-between mb-6">
    <h1 class="text-2xl font-bold text-gray-900">Catalog</h1>
    <span class="text-sm text-gray-400">{products.length} products</span>
  </div>

  {#if loading}
    <div class="flex justify-center py-12">
      <div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
    </div>
  {:else}
    <!-- Low stock alert -->
    {#if lowStock.length > 0}
      <div class="bg-amber-50 border border-amber-200 rounded-xl p-4 mb-6">
        <p class="text-sm font-medium text-amber-800 mb-2">⚠ {lowStock.length} items low on stock</p>
        <div class="flex flex-wrap gap-2">
          {#each lowStock as p}
            <span class="px-2 py-1 bg-white border border-amber-200 rounded-lg text-xs text-amber-700">
              {p.name} ({p.stockQuantity} left)
            </span>
          {/each}
        </div>
      </div>
    {/if}

    <!-- Filters -->
    <div class="flex gap-3 mb-4">
      <input bind:value={search} placeholder="Search products..." class="flex-1 px-3 py-2 border border-gray-200 rounded-lg text-sm" />
      <select bind:value={selectedCategory} class="px-3 py-2 border border-gray-200 rounded-lg text-sm">
        <option value="All">All Categories</option>
        {#each categories as cat}
          <option value={cat}>{cat}</option>
        {/each}
      </select>
    </div>

    <!-- Product table -->
    <div class="bg-white rounded-xl border border-gray-100 overflow-hidden">
      <table class="w-full text-sm">
        <thead class="bg-gray-50">
          <tr>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Product</th>
            <th class="text-left px-4 py-3 font-medium text-gray-600">Category</th>
            <th class="text-right px-4 py-3 font-medium text-gray-600">Price</th>
            <th class="text-center px-4 py-3 font-medium text-gray-600">Inventory</th>
            <th class="text-center px-4 py-3 font-medium text-gray-600">Stock</th>
            <th class="text-center px-4 py-3 font-medium text-gray-600">Status</th>
            <th class="px-4 py-3"></th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-50">
          {#each filtered as product (product.id)}
            <tr class="hover:bg-gray-50 transition-colors">
              <td class="px-4 py-3">
                <div class="flex items-center gap-3">
                  {#if product.imageUrl}
                    <img src={product.imageUrl} alt="" class="w-8 h-8 rounded-lg object-cover" />
                  {:else}
                    <div class="w-8 h-8 rounded-lg bg-gray-100 flex items-center justify-center text-gray-400 text-xs">N/A</div>
                  {/if}
                  <div>
                    <p class="font-medium text-gray-900">{product.name}</p>
                    {#if product.isRegulated}
                      <span class="text-xs text-red-500">⚕ Regulated</span>
                    {/if}
                  </div>
                </div>
              </td>
              <td class="px-4 py-3 text-gray-600">{product.category}</td>
              <td class="px-4 py-3 text-right font-medium">{formatPrice(product.pricePaise)}</td>
              <td class="px-4 py-3 text-center">
                <span class="px-2 py-0.5 rounded-full text-xs {product.inventoryType === 1 ? 'bg-blue-100 text-blue-700' : 'bg-green-100 text-green-700'}">
                  {product.inventoryType === 1 ? 'Finite' : 'Abundant'}
                </span>
              </td>
              <td class="px-4 py-3 text-center">
                {#if product.inventoryType === 1}
                  <span class="{(product.stockQuantity ?? 0) < 5 ? 'text-red-600 font-semibold' : 'text-gray-700'}">
                    {product.stockQuantity ?? 0}
                  </span>
                {:else}
                  <span class="text-gray-400">∞</span>
                {/if}
              </td>
              <td class="px-4 py-3 text-center">
                <span class="w-2 h-2 rounded-full inline-block {product.isAvailable ? 'bg-green-500' : 'bg-red-500'}"></span>
              </td>
              <td class="px-4 py-3 text-right">
                <button onclick={() => editProduct(product)} class="text-xs text-purple-600 hover:text-purple-800 font-medium">Edit</button>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>

    <!-- Edit modal -->
    {#if showForm && editingProduct}
      <div class="fixed inset-0 bg-black/30 z-50 flex items-center justify-center p-4" role="dialog">
        <div class="bg-white rounded-2xl p-6 w-full max-w-md max-h-[80vh] overflow-y-auto">
          <h3 class="text-lg font-bold text-gray-900 mb-4">Edit Product</h3>
          <div class="space-y-4">
            <div>
              <label class="text-xs font-medium text-gray-600">Name</label>
              <input bind:value={editingProduct.name} class="w-full mt-1 px-3 py-2 border border-gray-200 rounded-lg text-sm" />
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="text-xs font-medium text-gray-600">Price (paise)</label>
                <input type="number" bind:value={editingProduct.pricePaise} class="w-full mt-1 px-3 py-2 border border-gray-200 rounded-lg text-sm" />
              </div>
              <div>
                <label class="text-xs font-medium text-gray-600">Category</label>
                <select bind:value={editingProduct.category} class="w-full mt-1 px-3 py-2 border border-gray-200 rounded-lg text-sm">
                  {#each categories as cat}
                    <option value={cat}>{cat}</option>
                  {/each}
                </select>
              </div>
            </div>
            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="text-xs font-medium text-gray-600">Inventory Mode</label>
                <select bind:value={editingProduct.inventoryType} class="w-full mt-1 px-3 py-2 border border-gray-200 rounded-lg text-sm">
                  <option value={1}>Finite (tracked)</option>
                  <option value={2}>Abundant (always available)</option>
                </select>
              </div>
              {#if editingProduct.inventoryType === 1}
                <div>
                  <label class="text-xs font-medium text-gray-600">Stock Qty</label>
                  <input type="number" bind:value={editingProduct.stockQuantity} class="w-full mt-1 px-3 py-2 border border-gray-200 rounded-lg text-sm" />
                </div>
              {/if}
            </div>
            <div class="flex items-center gap-4">
              <label class="flex items-center gap-2 text-sm">
                <input type="checkbox" bind:checked={editingProduct.isAvailable} class="rounded" />
                Available
              </label>
              <label class="flex items-center gap-2 text-sm">
                <input type="checkbox" bind:checked={editingProduct.isRegulated} class="rounded" />
                Regulated Item
              </label>
            </div>
          </div>
          <div class="flex gap-2 mt-6">
            <button onclick={closeForm} class="flex-1 py-2.5 border border-gray-200 rounded-lg text-sm text-gray-600 hover:bg-gray-50">Cancel</button>
            <button onclick={closeForm} class="flex-1 py-2.5 bg-purple-600 text-white rounded-lg text-sm font-medium hover:bg-purple-700">Save Changes</button>
          </div>
        </div>
      </div>
    {/if}
  {/if}
</div>
