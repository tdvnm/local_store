<script lang="ts">
	import { page } from '$app/stores';
	const shopId = $derived($page.params.shopId);
</script>

<div class="shop-page">
	<h1>Shop</h1>
	<p>Shop ID: {shopId}</p>
	<p>Product listing coming soon.</p>
</div>

<style>
	.shop-page { padding: var(--space-4); max-width: 600px; margin: 0 auto; }
</style>
