<script lang="ts">
  type ToastType = 'success' | 'error' | 'info' | 'warning';

  interface Props {
    message: string;
    type?: ToastType;
    visible?: boolean;
    duration?: number;
    onclose?: () => void;
  }

  let {
    message,
    type = 'info',
    visible = $bindable(true),
    duration = 3000,
    onclose
  }: Props = $props();

  $effect(() => {
    if (visible && duration > 0) {
      const timer = setTimeout(() => {
        visible = false;
        onclose?.();
      }, duration);
      return () => clearTimeout(timer);
    }
  });

  const icons: Record<ToastType, string> = {
    success: '✓',
    error: '✕',
    info: 'ℹ',
    warning: '⚠'
  };
</script>

{#if visible}
  <div class="toast toast-{type}" role="alert">
    <span class="toast-icon">{icons[type]}</span>
    <span class="toast-message">{message}</span>
    <button class="toast-close" onclick={() => { visible = false; onclose?.(); }}>×</button>
  </div>
{/if}

<style>
  .toast {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3) var(--space-4);
    border-radius: var(--radius-lg);
    box-shadow: var(--shadow-lg);
    font-family: var(--font-family);
    font-size: var(--text-sm);
    animation: slideIn 0.2s ease;
    max-width: 400px;
    width: calc(100vw - 2rem);
  }

  .toast-success {
    background: var(--color-success-light);
    color: var(--color-success);
    border: 1px solid var(--color-success);
  }

  .toast-error {
    background: var(--color-danger-light);
    color: var(--color-danger);
    border: 1px solid var(--color-danger);
  }

  .toast-info {
    background: var(--color-info-light);
    color: var(--color-info);
    border: 1px solid var(--color-info);
  }

  .toast-warning {
    background: var(--color-warning-light);
    color: #92400E;
    border: 1px solid var(--color-warning);
  }

  .toast-icon {
    font-weight: var(--font-bold);
    font-size: var(--text-base);
    flex-shrink: 0;
  }

  .toast-message {
    flex: 1;
  }

  .toast-close {
    background: none;
    border: none;
    font-size: var(--text-lg);
    cursor: pointer;
    opacity: 0.7;
    padding: 0;
    line-height: 1;
    color: inherit;
  }

  .toast-close:hover {
    opacity: 1;
  }

  @keyframes slideIn {
    from {
      transform: translateY(-1rem);
      opacity: 0;
    }
    to {
      transform: translateY(0);
      opacity: 1;
    }
  }
</style>
