<script lang="ts">
  interface Props {
    count: number;
    total: number;
    href?: string;
  }

  let { count, total, href = "/cart" }: Props = $props();
</script>

{#if count > 0}
  <div class="fixed bottom-14 left-0 right-0 z-50 animate-slide-up">
    <div class="frosted-dark">
      <a {href} class="block">
        <div class="max-w-lg mx-auto px-4 py-4 flex items-center justify-between text-white">
          <div>
            <span class="font-bold">{count} item{count > 1 ? "s" : ""}</span>
            <span class="mx-2 opacity-50">|</span>
            <span class="font-bold">&#8377;{total}</span>
          </div>
          <span class="font-semibold">View Cart &rarr;</span>
        </div>
      </a>
    </div>
  </div>
{/if}
