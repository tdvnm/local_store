<script lang="ts">
  interface Props {
    id?: string;
    label?: string;
    type?: 'text' | 'tel' | 'email' | 'password' | 'number' | 'search';
    placeholder?: string;
    value?: string;
    error?: string;
    disabled?: boolean;
    required?: boolean;
    maxlength?: number;
    oninput?: (e: Event) => void;
  }

  let {
    id,
    label,
    type = 'text',
    placeholder,
    value = $bindable(''),
    error,
    disabled = false,
    required = false,
    maxlength,
    oninput
  }: Props = $props();
</script>

<div class="input-group" class:has-error={!!error}>
  {#if label}
    <label for={id} class="input-label">
      {label}
      {#if required}<span class="required">*</span>{/if}
    </label>
  {/if}
  <input
    {id}
    {type}
    {placeholder}
    {disabled}
    {required}
    {maxlength}
    class="input"
    bind:value
    {oninput}
  />
  {#if error}
    <span class="error-text">{error}</span>
  {/if}
</div>

<style>
  .input-group {
    display: flex;
    flex-direction: column;
    gap: var(--space-1);
  }

  .input-label {
    font-size: var(--text-sm);
    font-weight: var(--font-medium);
    color: var(--color-text-primary);
  }

  .required {
    color: var(--color-danger);
  }

  .input {
    width: 100%;
    padding: var(--space-3) var(--space-4);
    font-size: var(--text-base);
    font-family: var(--font-family);
    color: var(--color-text-primary);
    background: var(--color-surface);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    transition: border-color var(--transition-fast);
    outline: none;
    min-height: 44px;
  }

  .input::placeholder {
    color: var(--color-text-muted);
  }

  .input:focus {
    border-color: var(--color-primary);
    box-shadow: 0 0 0 3px var(--color-primary-light);
  }

  .input:disabled {
    opacity: 0.5;
    cursor: not-allowed;
    background: var(--color-surface-muted);
  }

  .has-error .input {
    border-color: var(--color-danger);
  }

  .has-error .input:focus {
    box-shadow: 0 0 0 3px var(--color-danger-light);
  }

  .error-text {
    font-size: var(--text-xs);
    color: var(--color-danger);
  }
</style>
