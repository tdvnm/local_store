<script lang="ts">
  interface Props {
    name: string;
    price: number; // in paise
    imageUrl?: string;
    inventoryType: 'finite' | 'abundant';
    stockQuantity?: number;
    isAvailable: boolean;
    quantity?: number; // current cart quantity
    onAdd?: () => void;
    onIncrement?: () => void;
    onDecrement?: () => void;
  }

  let {
    name,
    price,
    imageUrl,
    inventoryType,
    stockQuantity,
    isAvailable,
    quantity = 0,
    onAdd,
    onIncrement,
    onDecrement
  }: Props = $props();

  const priceDisplay = $derived(`₹${(price / 100).toFixed(0)}`);
  const availabilityText = $derived(
    inventoryType === 'finite'
      ? `In stock${stockQuantity !== undefined ? ` (${stockQuantity})` : ''}`
      : 'Usually available'
  );
  const availabilityClass = $derived(
    inventoryType === 'finite' ? 'stock-finite' : 'stock-abundant'
  );
</script>

<div class="product-card" class:unavailable={!isAvailable}>
  <div class="product-image">
    {#if imageUrl}
      <img src={imageUrl} alt={name} loading="lazy" />
    {:else}
      <div class="image-placeholder">📦</div>
    {/if}
  </div>
  <div class="product-info">
    <h3 class="product-name">{name}</h3>
    <span class="product-price">{priceDisplay}</span>
    <span class="availability {availabilityClass}">
      {#if isAvailable}
        {availabilityText}
      {:else}
        Out of stock
      {/if}
    </span>
  </div>
  <div class="product-action">
    {#if !isAvailable}
      <span class="unavailable-label">Unavailable</span>
    {:else if quantity === 0}
      <button class="add-btn" onclick={onAdd}>Add</button>
    {:else}
      <div class="qty-control">
        <button class="qty-btn" onclick={onDecrement}>−</button>
        <span class="qty-value">{quantity}</span>
        <button class="qty-btn" onclick={onIncrement}>+</button>
      </div>
    {/if}
  </div>
</div>

<style>
  .product-card {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-3);
    border-radius: var(--radius-lg);
    background: var(--color-surface);
    border: 1px solid var(--color-border);
    transition: box-shadow var(--transition-fast);
  }

  .product-card:hover {
    box-shadow: var(--shadow-sm);
  }

  .product-card.unavailable {
    opacity: 0.6;
  }

  .product-image {
    width: 64px;
    height: 64px;
    border-radius: var(--radius-md);
    overflow: hidden;
    flex-shrink: 0;
    background: var(--color-surface-muted);
  }

  .product-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .image-placeholder {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
  }

  .product-info {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 2px;
    min-width: 0;
  }

  .product-name {
    font-size: var(--text-sm);
    font-weight: var(--font-medium);
    color: var(--color-text-primary);
    margin: 0;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .product-price {
    font-size: var(--text-sm);
    font-weight: var(--font-bold);
    color: var(--color-text-primary);
  }

  .availability {
    font-size: var(--text-xs);
    font-weight: var(--font-medium);
  }

  .stock-finite {
    color: var(--color-success);
  }

  .stock-abundant {
    color: var(--color-text-secondary);
  }

  .product-action {
    flex-shrink: 0;
  }

  .add-btn {
    padding: var(--space-2) var(--space-4);
    background: var(--color-primary-light);
    color: var(--color-primary);
    border: 1px solid var(--color-primary);
    border-radius: var(--radius-md);
    font-size: var(--text-sm);
    font-weight: var(--font-semibold);
    cursor: pointer;
    transition: background var(--transition-fast);
    min-height: 36px;
  }

  .add-btn:hover {
    background: var(--color-primary);
    color: var(--color-text-inverse);
  }

  .qty-control {
    display: flex;
    align-items: center;
    gap: 0;
    border: 1px solid var(--color-primary);
    border-radius: var(--radius-md);
    overflow: hidden;
  }

  .qty-btn {
    width: 32px;
    height: 36px;
    border: none;
    background: var(--color-primary-light);
    color: var(--color-primary);
    font-size: var(--text-base);
    font-weight: var(--font-bold);
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .qty-btn:hover {
    background: var(--color-primary);
    color: var(--color-text-inverse);
  }

  .qty-value {
    width: 32px;
    text-align: center;
    font-size: var(--text-sm);
    font-weight: var(--font-semibold);
    color: var(--color-primary);
  }

  .unavailable-label {
    font-size: var(--text-xs);
    color: var(--color-text-muted);
  }
</style>
