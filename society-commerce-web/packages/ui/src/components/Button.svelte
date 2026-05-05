<script lang="ts">
  type Variant = 'primary' | 'secondary' | 'danger' | 'ghost';
  type Size = 'sm' | 'md' | 'lg';

  interface Props {
    variant?: Variant;
    size?: Size;
    disabled?: boolean;
    loading?: boolean;
    fullWidth?: boolean;
    type?: 'button' | 'submit' | 'reset';
    onclick?: (e: MouseEvent) => void;
    children: any;
  }

  let {
    variant = 'primary',
    size = 'md',
    disabled = false,
    loading = false,
    fullWidth = false,
    type = 'button',
    onclick,
    children
  }: Props = $props();
</script>

<button
  {type}
  class="btn btn-{variant} btn-{size}"
  class:full-width={fullWidth}
  class:loading
  disabled={disabled || loading}
  {onclick}
>
  {#if loading}
    <span class="spinner"></span>
  {/if}
  <span class="btn-content" class:invisible={loading}>
    {@render children()}
  </span>
</button>

<style>
  .btn {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-2);
    border: none;
    border-radius: var(--radius-md);
    font-family: var(--font-family);
    font-weight: var(--font-semibold);
    cursor: pointer;
    transition: all var(--transition-fast);
    position: relative;
    white-space: nowrap;
    text-decoration: none;
    line-height: 1;
  }

  .btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .btn:focus-visible {
    outline: 2px solid var(--color-primary);
    outline-offset: 2px;
  }

  /* Sizes */
  .btn-sm {
    padding: var(--space-2) var(--space-3);
    font-size: var(--text-xs);
    min-height: 32px;
  }

  .btn-md {
    padding: var(--space-3) var(--space-4);
    font-size: var(--text-sm);
    min-height: 40px;
  }

  .btn-lg {
    padding: var(--space-4) var(--space-6);
    font-size: var(--text-base);
    min-height: 48px;
  }

  /* Variants */
  .btn-primary {
    background: var(--color-primary);
    color: var(--color-text-inverse);
  }
  .btn-primary:hover:not(:disabled) {
    background: var(--color-primary-hover);
  }

  .btn-secondary {
    background: var(--color-surface);
    color: var(--color-text-primary);
    border: 1px solid var(--color-border-strong);
  }
  .btn-secondary:hover:not(:disabled) {
    background: var(--color-surface-hover);
  }

  .btn-danger {
    background: var(--color-danger);
    color: var(--color-text-inverse);
  }
  .btn-danger:hover:not(:disabled) {
    background: #B91C1C;
  }

  .btn-ghost {
    background: transparent;
    color: var(--color-primary);
  }
  .btn-ghost:hover:not(:disabled) {
    background: var(--color-primary-light);
  }

  .full-width {
    width: 100%;
  }

  .spinner {
    position: absolute;
    width: 16px;
    height: 16px;
    border: 2px solid transparent;
    border-top-color: currentColor;
    border-radius: 50%;
    animation: spin 0.6s linear infinite;
  }

  .invisible {
    visibility: hidden;
  }

  @keyframes spin {
    to { transform: rotate(360deg); }
  }
</style>
