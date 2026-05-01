<script lang="ts">
  import Badge from "./Badge.svelte";
  import Button from "./Button.svelte";

  interface OrderItem {
    id: number;
    product_name: string;
    quantity: number;
    price: number;
    category: string;
  }

  interface Order {
    id: number;
    user_name: string;
    flat_no: string;
    status: string;
    payment_method: string;
    total: number;
    created_at: string;
    items: OrderItem[];
  }

  interface Props {
    order: Order;
    onupdatestatus: (id: number, status: string) => void;
  }

  let { order, onupdatestatus }: Props = $props();

  const statusColors: Record<string, string> = {
    pending: "bg-yellow-100 text-yellow-800",
    confirmed: "bg-blue-100 text-blue-800",
    ready: "bg-purple-100 text-purple-800",
    delivered: "bg-[#eef1ff] text-[var(--c-emerald)]",
    cancelled: "bg-red-100 text-red-800",
  };

  const nextStatus: Record<string, string> = { pending: "confirmed", confirmed: "ready", ready: "delivered" };
  const statusAction: Record<string, string> = { pending: "Confirm", confirmed: "Mark Ready", ready: "Mark Delivered" };
</script>

<div class="card overflow-hidden animate-in">
  <div class="p-4 flex items-start justify-between">
    <div>
      <div class="flex items-center gap-2">
        <span class="font-bold text-gray-800">#{order.id}</span>
        <Badge class={statusColors[order.status]} label={order.status} />
        <Badge
          class={order.payment_method === "tab" ? "bg-orange-100 text-orange-700" : "bg-gray-100 text-gray-600"}
          label={order.payment_method === "tab" ? "Monthly Tab" : "COD"}
        />
      </div>
      <div class="text-sm text-gray-500 mt-1">{order.user_name} - {order.flat_no}</div>
      <div class="text-xs text-gray-400 mt-0.5">{new Date(order.created_at).toLocaleString()}</div>
    </div>
    <div class="text-lg font-bold text-gray-800">&#8377;{order.total}</div>
  </div>
  <div class="px-4 py-2 border-t border-gray-50">
    {#each order.items as item (item.id)}
      <div class="flex justify-between py-1 text-sm">
        <span class="text-gray-700">{item.product_name} x{item.quantity}</span>
        <span class="text-gray-500">&#8377;{item.price * item.quantity}</span>
      </div>
    {/each}
  </div>
  {#if nextStatus[order.status]}
    <div class="px-4 py-3 bg-gray-50 flex gap-2">
      <Button variant="primary" class="flex-1 py-2.5" onclick={() => onupdatestatus(order.id, nextStatus[order.status])}>
        {statusAction[order.status]}
      </Button>
      {#if order.status === "pending"}
        <Button variant="danger" class="px-4 py-2.5" onclick={() => onupdatestatus(order.id, "cancelled")}>
          Cancel
        </Button>
      {/if}
    </div>
  {/if}
</div>
