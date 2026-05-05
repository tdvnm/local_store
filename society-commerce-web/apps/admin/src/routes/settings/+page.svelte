<script lang="ts">
  import { auth } from '$lib/stores/auth.svelte';

  let confirmationWindow = $state(5);
  let maxOrderItems = $state(20);
  let deliveryRadius = $state(2);
  let maintenanceMode = $state(false);
</script>

<div class="p-6 max-w-2xl">
  <h1 class="text-2xl font-bold text-gray-900 mb-6">Platform Settings</h1>

  <div class="space-y-6">
    <div class="bg-white rounded-xl p-5 border border-gray-100">
      <h3 class="font-semibold text-gray-900 mb-4">Order Configuration</h3>
      <div class="space-y-4">
        <div class="flex justify-between items-center">
          <div>
            <p class="text-sm font-medium text-gray-700">Confirmation Window</p>
            <p class="text-xs text-gray-400">Minutes sellers have to confirm items</p>
          </div>
          <div class="flex items-center gap-2">
            <input type="number" bind:value={confirmationWindow} min="1" max="30" class="w-16 px-2 py-1.5 border border-gray-200 rounded-lg text-sm text-center" />
            <span class="text-sm text-gray-500">min</span>
          </div>
        </div>
        <div class="flex justify-between items-center">
          <div>
            <p class="text-sm font-medium text-gray-700">Max Items Per Order</p>
            <p class="text-xs text-gray-400">Limit on items in a single order</p>
          </div>
          <input type="number" bind:value={maxOrderItems} min="5" max="50" class="w-16 px-2 py-1.5 border border-gray-200 rounded-lg text-sm text-center" />
        </div>
        <div class="flex justify-between items-center">
          <div>
            <p class="text-sm font-medium text-gray-700">Delivery Radius</p>
            <p class="text-xs text-gray-400">Max km for delivery orders</p>
          </div>
          <div class="flex items-center gap-2">
            <input type="number" bind:value={deliveryRadius} min="1" max="10" class="w-16 px-2 py-1.5 border border-gray-200 rounded-lg text-sm text-center" />
            <span class="text-sm text-gray-500">km</span>
          </div>
        </div>
      </div>
    </div>

    <div class="bg-white rounded-xl p-5 border border-gray-100">
      <div class="flex items-center justify-between">
        <div>
          <h3 class="font-semibold text-gray-900">Maintenance Mode</h3>
          <p class="text-sm text-gray-500 mt-0.5">Disable all buyer-facing operations</p>
        </div>
        <button
          onclick={() => maintenanceMode = !maintenanceMode}
          class="px-4 py-2 rounded-full text-sm font-medium transition-colors {maintenanceMode ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'}"
        >
          {maintenanceMode ? 'Enabled' : 'Disabled'}
        </button>
      </div>
    </div>

    <div class="bg-white rounded-xl p-5 border border-gray-100">
      <h3 class="font-semibold text-gray-900 mb-2">Admin Account</h3>
      <div class="space-y-2 text-sm">
        <div class="flex justify-between">
          <span class="text-gray-500">Name</span>
          <span class="text-gray-900 font-medium">{auth.user?.name}</span>
        </div>
        <div class="flex justify-between">
          <span class="text-gray-500">Phone</span>
          <span class="text-gray-900">{auth.user?.phone}</span>
        </div>
      </div>
    </div>
  </div>
</div>
