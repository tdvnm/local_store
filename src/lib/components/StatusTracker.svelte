<script lang="ts">
  interface Props {
    status: string;
    deliveryMode: string;
  }

  let { status, deliveryMode }: Props = $props();

  const deliverySteps = ["pending", "confirmed", "packed", "out_for_delivery", "delivered"];
  const pickupSteps = ["pending", "confirmed", "packed", "ready_for_pickup", "delivered"];

  const stepLabels: Record<string, string> = {
    pending: "Order Placed",
    confirmed: "Confirmed",
    packed: "Packed",
    out_for_delivery: "Out for Delivery",
    ready_for_pickup: "Ready for Pickup",
    delivered: "Delivered",
  };

  const steps = $derived(deliveryMode === "pickup" ? pickupSteps : deliverySteps);

  const currentIdx = $derived(
    status === "cancelled" ? -1 : steps.indexOf(status)
  );
</script>

{#if status === "cancelled"}
  <div class="flex items-center gap-3 p-4 bg-red-50 rounded-xl">
    <div class="w-8 h-8 rounded-full bg-red-100 flex items-center justify-center text-red-500 text-sm font-bold">X</div>
    <div>
      <div class="font-semibold text-red-700">Order Cancelled</div>
      <div class="text-xs text-red-400">This order has been cancelled</div>
    </div>
  </div>
{:else}
  <div class="space-y-0">
    {#each steps as step, i (step)}
      {@const done = i <= currentIdx}
      {@const current = i === currentIdx}
      {@const last = i === steps.length - 1}
      <div class="flex gap-3">
        <div class="flex flex-col items-center">
          <div
            class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0 transition-colors
              {done ? 'bg-gradient-to-b from-[#1a7a5e] to-[var(--c-emerald)] text-white shadow-sm' : 'bg-gray-100 text-gray-400'}"
          >
            {#if done && !current}
              <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="3">
                <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
              </svg>
            {:else}
              {i + 1}
            {/if}
          </div>
          {#if !last}
            <div class="w-0.5 h-6 {done ? 'bg-[var(--c-emerald)]' : 'bg-gray-200'}"></div>
          {/if}
        </div>
        <div class="pt-1">
          <div class="text-sm font-medium {done ? 'text-gray-800' : 'text-gray-400'}">
            {stepLabels[step]}
          </div>
          {#if current}
            <div class="text-xs text-[var(--c-emerald)] font-medium mt-0.5">Current</div>
          {/if}
        </div>
      </div>
    {/each}
  </div>
{/if}
