<script lang="ts">
	import '../app.css';
	import { page } from '$app/state';
	import { auth } from '$lib/stores/auth.svelte';
	import { goto } from '$app/navigation';
	let { children } = $props();

	const navItems = [
		{ href: '/', label: 'Dashboard' },
		{ href: '/approvals', label: 'Approvals' },
		{ href: '/orders', label: 'Orders' },
		{ href: '/tickets', label: 'Tickets' },
		{ href: '/audit', label: 'Audit Log' },
		{ href: '/reports', label: 'Reports' },
		{ href: '/announcements', label: 'Announcements' },
		{ href: '/settings', label: 'Settings' },
	];

	const isLoginPage = $derived(page.url.pathname === '/login');

	$effect(() => {
		if (auth.ready && !auth.loggedIn && !isLoginPage) goto('/login');
	});

	function logout() {
		auth.logout();
		goto('/login');
	}
</script>

{#if isLoginPage || !auth.loggedIn}
	{@render children()}
{:else}
	<div class="flex min-h-screen bg-gray-50">
		<nav class="w-56 bg-gray-900 text-white flex flex-col">
			<div class="px-5 py-5 border-b border-gray-700">
				<h1 class="text-lg font-bold">SC Admin</h1>
			</div>
			<div class="flex-1 py-3 px-3 space-y-1">
				{#each navItems as item}
					<a
						href={item.href}
						class="block px-3 py-2 rounded-lg text-sm transition-colors {page.url.pathname === item.href ? 'bg-purple-600 text-white font-medium' : 'text-gray-300 hover:bg-gray-800 hover:text-white'}"
					>{item.label}</a>
				{/each}
			</div>
			<div class="px-4 py-4 border-t border-gray-700">
				<p class="text-xs text-gray-400 mb-2">{auth.user?.name}</p>
				<button onclick={logout} class="text-xs text-red-400 hover:text-red-300">Sign out</button>
			</div>
		</nav>
		<main class="flex-1 overflow-y-auto">
			{@render children()}
		</main>
	</div>
{/if}
