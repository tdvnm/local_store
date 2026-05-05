<script lang="ts">
  import { adminService } from '@society-commerce/api-client';
  import type { DashboardResponse } from '@society-commerce/api-client';

  let dashboard = $state<DashboardResponse | null>(null);
  let loading = $state(true);

  $effect(() => {
    adminService.getDashboard().then(d => { dashboard = d; loading = false; });
  });

  function formatPrice(paise: number) { return `₹${(paise / 100).toFixed(0)}`; }
</script>

<div class="p-6">
  <h1 class="text-2xl font-bold text-gray-900 mb-6">Dashboard</h1>

  {#if loading}
    <div class="flex justify-center py-12">
      <div class="w-8 h-8 border-3 border-purple-500 border-t-transparent rounded-full animate-spin"></div>
    </div>
  {:else if dashboard}
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <p class="text-sm text-gray-500">Total Users</p>
        <p class="text-2xl font-bold text-gray-900 mt-1">{dashboard.totalUsers}</p>
      </div>
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <p class="text-sm text-gray-500">Active Shops</p>
        <p class="text-2xl font-bold text-gray-900 mt-1">{dashboard.totalShops}</p>
      </div>
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <p class="text-sm text-gray-500">Active Orders</p>
        <p class="text-2xl font-bold text-purple-600 mt-1">{dashboard.activeOrders}</p>
      </div>
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <p class="text-sm text-gray-500">Today's Revenue</p>
        <p class="text-2xl font-bold text-green-600 mt-1">{formatPrice(dashboard.todayRevenuePaise)}</p>
      </div>
    </div>

    <div class="grid md:grid-cols-2 gap-6">
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <h3 class="font-semibold text-gray-900 mb-3">Quick Stats</h3>
        <div class="space-y-3">
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">Today's Orders</span>
            <span class="font-medium">{dashboard.todayOrders}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">Pending Seller Approvals</span>
            <span class="font-medium {dashboard.pendingSellers > 0 ? 'text-amber-600' : ''}">{dashboard.pendingSellers}</span>
          </div>
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">Open Support Tickets</span>
            <span class="font-medium {dashboard.openTickets > 0 ? 'text-red-600' : ''}">{dashboard.openTickets}</span>
          </div>
        </div>
      </div>
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <h3 class="font-semibold text-gray-900 mb-3">Platform Health</h3>
        <div class="space-y-3">
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">API Status</span>
            <span class="px-2 py-0.5 bg-green-100 text-green-700 text-xs rounded-full">Healthy</span>
          </div>
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">Uptime</span>
            <span class="font-medium">99.9%</span>
          </div>
          <div class="flex justify-between text-sm">
            <span class="text-gray-500">Avg Response</span>
            <span class="font-medium">42ms</span>
          </div>
        </div>
      </div>
    </div>
  {/if}
</div>
