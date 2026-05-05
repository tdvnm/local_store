<script lang="ts">
  import Badge from "./Badge.svelte";
  import Button from "./Button.svelte";

  interface OrderItem {
    id: string;
    productName: string;
    quantity: number;
    unitPricePaise: number;
  }

  interface Order {
    id: string;
    orderNumber: string;
    status: string;
    totalPaise: number;
    createdAt: string;
    items: OrderItem[];
  }

  interface Props {
    order: Order;
  }

  let { order }: Props = $props();

  const statusColors: Record<string, string> = {
    pending: "bg-yellow-100 text-yellow-800",
    awaiting_confirmation: "bg-amber-100 text-amber-800",
    confirmed: "bg-blue-100 text-blue-800",
    preparing: "bg-indigo-100 text-indigo-800",
    ready_for_pickup: "bg-purple-100 text-purple-800",
    out_for_delivery: "bg-purple-100 text-purple-800",
    completed: "bg-[#eef1ff] text-[var(--c-emerald)]",
    cancelled: "bg-red-100 text-red-800",
  };

  const displayTotal = $derived(order.totalPaise / 100);
</script>

<div class="card overflow-hidden animate-in">
  <div class="p-4 flex items-start justify-between">
    <div>
      <div class="flex items-center gap-2">
        <span class="font-bold text-gray-800">#{order.orderNumber}</span>
        <Badge class={statusColors[order.status] || "bg-gray-100 text-gray-600"} label={order.status.replace(/_/g, " ")} />
      </div>
      <div class="text-xs text-gray-400 mt-0.5">{new Date(order.createdAt).toLocaleString()}</div>
    </div>
    <div class="text-lg font-bold text-gray-800">&#8377;{displayTotal}</div>
  </div>
  <div class="px-4 py-2 border-t border-gray-50">
    {#each order.items.slice(0, 3) as item (item.id)}
      <div class="flex justify-between py-1 text-sm">
        <span class="text-gray-700">{item.productName} x{item.quantity}</span>
        <span class="text-gray-500">&#8377;{(item.unitPricePaise * item.quantity) / 100}</span>
      </div>
    {/each}
    {#if order.items.length > 3}
      <div class="text-xs text-gray-400 py-1">+{order.items.length - 3} more items</div>
    {/if}
  </div>
</div>
