<script lang="ts">
  import type { Snippet } from "svelte";
  import type { HTMLButtonAttributes } from "svelte/elements";

  interface Props extends HTMLButtonAttributes {
    variant?: "primary" | "ghost" | "danger" | "muted";
    children: Snippet;
  }

  let { variant = "primary", children, class: className = "", ...rest }: Props = $props();

  const variantClass = $derived(
    variant === "primary" ? "btn-primary"
    : variant === "ghost" ? "btn-ghost"
    : variant === "danger" ? "btn-danger"
    : "btn-muted"
  );
</script>

<button class="btn {variantClass} {className}" {...rest}>
  {@render children()}
</button>
