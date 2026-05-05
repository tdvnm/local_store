<script lang="ts">
  import { auth } from "$lib/stores/auth.svelte";
  import { shopService } from "@society-commerce/api-client";
  import type { ShopResponse } from "@society-commerce/api-client";

  let shop = $state<ShopResponse | null>(null);
  let isOpen = $state(true);
  let loading = $state(true);

  let loaded = false;
  $effect(() => {
    if (!loaded && auth.shopId) { loaded = true; loadShop(); }
  });

  async function loadShop() {
    try {
      shop = await shopService.get(auth.shopId!);
      if (shop) isOpen = shop.isActive;
    } finally { loading = false; }
  }

  function toggleOpen() {
    isOpen = !isOpen;
    // In a real app, this would call an API
  }
</script>

<div class="p-6 max-w-2xl">
  <h1 class="text-2xl font-bold text-gray-900 mb-6">Shop Settings</h1>

  {#if loading}
    <div class="flex justify-center py-12">
      <div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
    </div>
  {:else if shop}
    <div class="space-y-6">
      <!-- Shop status -->
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="font-semibold text-gray-900">Shop Status</h3>
            <p class="text-sm text-gray-500 mt-0.5">Control whether your shop is accepting orders</p>
          </div>
          <button
            onclick={toggleOpen}
            class="px-4 py-2 rounded-full text-sm font-medium transition-all {isOpen ? 'bg-green-100 text-green-700 hover:bg-green-200' : 'bg-red-100 text-red-700 hover:bg-red-200'}"
          >
            {isOpen ? '● Open' : '○ Closed'}
          </button>
        </div>
      </div>

      <!-- Shop profile -->
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <h3 class="font-semibold text-gray-900 mb-4">Shop Profile</h3>
        <div class="space-y-4">
          <div>
            <label class="text-xs font-medium text-gray-600">Shop Name</label>
            <input value={shop.name} readonly class="w-full mt-1 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm" />
          </div>
          <div>
            <label class="text-xs font-medium text-gray-600">Category</label>
            <input value={shop.category} readonly class="w-full mt-1 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm" />
          </div>
          <div>
            <label class="text-xs font-medium text-gray-600">Description</label>
            <textarea value={shop.description ?? ''} readonly rows="3" class="w-full mt-1 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg text-sm resize-none"></textarea>
          </div>
        </div>
      </div>

      <!-- Operating hours placeholder -->
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <h3 class="font-semibold text-gray-900 mb-2">Operating Hours</h3>
        <p class="text-sm text-gray-500">Configure your shop's opening hours and days of operation.</p>
        <div class="mt-4 grid grid-cols-2 gap-3">
          <div>
            <label class="text-xs font-medium text-gray-600">Opens at</label>
            <input type="time" value="08:00" class="w-full mt-1 px-3 py-2 border border-gray-200 rounded-lg text-sm" />
          </div>
          <div>
            <label class="text-xs font-medium text-gray-600">Closes at</label>
            <input type="time" value="21:00" class="w-full mt-1 px-3 py-2 border border-gray-200 rounded-lg text-sm" />
          </div>
        </div>
      </div>

      <!-- Seller info -->
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <h3 class="font-semibold text-gray-900 mb-2">Account</h3>
        <div class="space-y-2 text-sm">
          <div class="flex justify-between">
            <span class="text-gray-500">Name</span>
            <span class="text-gray-900 font-medium">{auth.user?.name}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Phone</span>
            <span class="text-gray-900">{auth.user?.phone}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Role</span>
            <span class="text-gray-900">{auth.user?.roles.join(', ').replace(/_/g, ' ')}</span>
          </div>
        </div>
      </div>
    </div>
  {/if}
</div>
