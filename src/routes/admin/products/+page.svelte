<script lang="ts">
  import { onMount } from "svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import Input from "$lib/components/Input.svelte";
  import Badge from "$lib/components/Badge.svelte";

  type Product = { id: number; name: string; price: number; category: string; unit: string; in_stock: number };

  let products = $state<Product[]>([]);
  let search = $state("");
  let showAdd = $state(false);
  let newProduct = $state({ name: "", price: "", category: "", unit: "piece" });

  function fetchProducts() {
    fetch("/api/products").then((r) => r.json()).then((data) => (products = data));
  }

  onMount(fetchProducts);

  async function toggleStock(p: Product) {
    await fetch(`/api/products/${p.id}`, { method: "PATCH", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ in_stock: !p.in_stock }) });
    fetchProducts();
  }

  async function addProduct() {
    if (!newProduct.name || !newProduct.price || !newProduct.category) { alert("Fill in all fields"); return; }
    await fetch("/api/products", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ ...newProduct, price: parseFloat(newProduct.price) }) });
    newProduct = { name: "", price: "", category: "", unit: "piece" };
    showAdd = false;
    fetchProducts();
  }

  async function deleteProduct(id: number) {
    if (!confirm("Delete this product?")) return;
    await fetch(`/api/products/${id}`, { method: "DELETE" });
    fetchProducts();
  }

  const filtered = $derived(products.filter((p) => p.name.toLowerCase().includes(search.toLowerCase())));
  const categories = $derived([...new Set(products.map((p) => p.category))].sort());
</script>

<svelte:head>
  <title>Products - Lucky Store Admin</title>
</svelte:head>

<div>
  <div class="flex items-center justify-between mb-4">
    <h2 class="text-xl font-bold text-gray-800">Products ({products.length})</h2>
    <Button variant="primary" class="px-4 py-2" onclick={() => (showAdd = !showAdd)}>{showAdd ? "Cancel" : "+ Add Product"}</Button>
  </div>

  {#if showAdd}
    <Card class="p-4 mb-4">
      <h3 class="font-bold text-gray-800 mb-3">New Product</h3>
      <div class="grid grid-cols-2 gap-3">
        <Input type="text" placeholder="Product Name" bind:value={newProduct.name} class="col-span-2" />
        <Input type="number" placeholder="Price" bind:value={newProduct.price} />
        <select bind:value={newProduct.unit} class="input">
          {#each ["piece", "pack", "kg", "bottle", "bag", "jar", "bar"] as u (u)}
            <option value={u}>{u}</option>
          {/each}
        </select>
        <select bind:value={newProduct.category} class="input">
          <option value="">Select Category</option>
          {#each categories as c (c)}
            <option value={c}>{c}</option>
          {/each}
          <option value="Other">Other</option>
        </select>
        <Button variant="primary" class="py-2" onclick={addProduct}>Add Product</Button>
      </div>
    </Card>
  {/if}

  <Input type="text" placeholder="Search products..." bind:value={search} class="mb-4" />

  <Card class="divide-y divide-gray-100">
    {#each filtered as product (product.id)}
      <div class="flex items-center justify-between p-4">
        <div class="flex-1">
          <div class="flex items-center gap-2">
            <h3 class="text-sm font-medium text-gray-800">{product.name}</h3>
            {#if !product.in_stock}
              <Badge class="bg-red-100 text-red-600" label="Out of Stock" />
            {/if}
          </div>
          <div class="text-xs text-gray-400 mt-0.5">{product.category} &middot; &#8377;{product.price}/{product.unit}</div>
        </div>
        <div class="flex items-center gap-2">
          <Button
            variant={product.in_stock ? "ghost" : "danger"}
            class="text-xs px-3 py-1.5"
            onclick={() => toggleStock(product)}
          >
            {product.in_stock ? "In Stock" : "Out of Stock"}
          </Button>
          <Button variant="muted" class="text-xs px-3 py-1.5 hover:!bg-red-50 hover:!text-red-600" onclick={() => deleteProduct(product.id)}>
            Delete
          </Button>
        </div>
      </div>
    {/each}
  </Card>
</div>
