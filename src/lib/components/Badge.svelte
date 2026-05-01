<script lang="ts">
  interface Props {
    class?: string;
    label: string;
  }

  let { class: className = "", label }: Props = $props();
</script>

<span class="badge {className}">{label}</span>
