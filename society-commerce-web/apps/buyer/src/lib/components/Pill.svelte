<script lang="ts">
  interface Props {
    active?: boolean;
    onclick?: () => void;
    label: string;
  }

  let { active = false, onclick, label }: Props = $props();
</script>

<button class="pill {active ? 'pill-active' : 'pill-inactive'}" {onclick}>
  {label}
</button>
