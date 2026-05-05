<script lang="ts">
  type StatusType = 'confirmed' | 'pending' | 'partial' | 'rejected' | 'auto_rejected' | 'substitution' | 'sub_accepted' | 'sub_rejected'
    | 'preparing' | 'ready' | 'out_for_delivery' | 'completed' | 'cancelled';

  interface Props {
    status: StatusType;
    label?: string;
    size?: 'sm' | 'md';
  }

  let { status, label, size = 'sm' }: Props = $props();

  const statusConfig: Record<StatusType, { color: string; bg: string; defaultLabel: string }> = {
    confirmed: { color: 'var(--color-success)', bg: 'var(--color-success-light)', defaultLabel: 'Confirmed' },
    pending: { color: 'var(--color-warning)', bg: 'var(--color-warning-light)', defaultLabel: 'Awaiting confirmation' },
    partial: { color: '#EA580C', bg: '#FFF7ED', defaultLabel: 'Partially confirmed' },
    rejected: { color: 'var(--color-danger)', bg: 'var(--color-danger-light)', defaultLabel: 'Unavailable' },
    auto_rejected: { color: 'var(--color-danger)', bg: 'var(--color-danger-light)', defaultLabel: 'Seller didn\'t respond' },
    substitution: { color: 'var(--color-info)', bg: 'var(--color-info-light)', defaultLabel: 'Substitute offered' },
    sub_accepted: { color: 'var(--color-success)', bg: 'var(--color-success-light)', defaultLabel: 'Substitute accepted' },
    sub_rejected: { color: 'var(--color-danger)', bg: 'var(--color-danger-light)', defaultLabel: 'Substitute rejected' },
    preparing: { color: 'var(--color-info)', bg: 'var(--color-info-light)', defaultLabel: 'Preparing' },
    ready: { color: 'var(--color-success)', bg: 'var(--color-success-light)', defaultLabel: 'Ready for pickup' },
    out_for_delivery: { color: 'var(--color-info)', bg: 'var(--color-info-light)', defaultLabel: 'Out for delivery' },
    completed: { color: 'var(--color-success)', bg: 'var(--color-success-light)', defaultLabel: 'Completed' },
    cancelled: { color: 'var(--color-text-muted)', bg: 'var(--color-surface-muted)', defaultLabel: 'Cancelled' }
  };

  const config = $derived(statusConfig[status] ?? statusConfig.pending);
  const displayLabel = $derived(label ?? config.defaultLabel);
</script>

<span
  class="chip chip-{size}"
  style:color={config.color}
  style:background={config.bg}
>
  <span class="dot" style:background={config.color}></span>
  {displayLabel}
</span>

<style>
  .chip {
    display: inline-flex;
    align-items: center;
    gap: var(--space-1);
    border-radius: var(--radius-full);
    font-family: var(--font-family);
    font-weight: var(--font-medium);
    white-space: nowrap;
  }

  .chip-sm {
    padding: 2px var(--space-2);
    font-size: var(--text-xs);
  }

  .chip-md {
    padding: var(--space-1) var(--space-3);
    font-size: var(--text-sm);
  }

  .dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
    flex-shrink: 0;
  }
</style>
