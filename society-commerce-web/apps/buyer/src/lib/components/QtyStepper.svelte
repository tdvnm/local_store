<script lang="ts">
  interface Props {
    quantity: number;
    onincrement: () => void;
    ondecrement: () => void;
    class?: string;
  }

  let { quantity, onincrement, ondecrement, class: className = "" }: Props = $props();
</script>

<div class="qty-stepper {className}">
  <button onclick={ondecrement}>-</button>
  <span>{quantity}</span>
  <button onclick={onincrement}>+</button>
</div>
