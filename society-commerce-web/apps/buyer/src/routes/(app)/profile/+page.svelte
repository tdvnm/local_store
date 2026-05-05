<script lang="ts">
  import { goto } from "$app/navigation";
  import { auth } from "$lib/stores/auth.svelte";
  import { householdService, notificationService, supportService } from "$lib/api";
  import Header from "$lib/components/Header.svelte";
  import Card from "$lib/components/Card.svelte";
  import Button from "$lib/components/Button.svelte";
  import Badge from "$lib/components/Badge.svelte";
  import type { NotificationResponse, SupportTicket } from "@society-commerce/api-client";

  let household = $state<{ id: string; flatNumber: string; members: { id: string; name: string; phone: string; role: string }[] } | null>(null);
  let notifications = $state<NotificationResponse[]>([]);
  let showSupport = $state(false);
  let ticketSubject = $state("");
  let ticketDesc = $state("");
  let ticketType = $state<'general' | 'dispute' | 'bug'>('general');
  let submitting = $state(false);
  let loading = $state(true);

  let loaded = false;
  $effect(() => {
    if (!loaded && auth.user) { loaded = true; loadData(); }
  });

  async function loadData() {
    try {
      const [hh, notifs] = await Promise.all([
        householdService.getForUser(auth.user!.id),
        notificationService.list(),
      ]);
      if (hh) {
        const members = await householdService.getMembers(hh.id);
        household = { id: hh.id, flatNumber: hh.flatNumber, members };
      }
      notifications = notifs;
    } finally { loading = false; }
  }

  async function markNotifRead(id: string) {
    await notificationService.markRead(id);
    notifications = notifications.map(n => n.id === id ? { ...n, isRead: true } : n);
  }

  async function submitTicket() {
    if (!ticketSubject.trim() || !ticketDesc.trim()) return;
    submitting = true;
    try {
      await supportService.create({ type: ticketType, subject: ticketSubject, description: ticketDesc });
      showSupport = false;
      ticketSubject = "";
      ticketDesc = "";
    } finally { submitting = false; }
  }

  function handleLogout() {
    auth.logout();
    goto("/login");
  }
</script>

<svelte:head><title>Profile - Society Commerce</title></svelte:head>

<div class="min-h-screen pb-24">
  <Header>
    <div class="max-w-lg mx-auto px-4 py-3">
      <h1 class="text-xl font-bold">Profile</h1>
    </div>
  </Header>

  <main class="max-w-lg mx-auto px-4 mt-4 space-y-4">
    {#if loading}
      <div class="flex justify-center py-12">
        <div class="w-8 h-8 border-3 border-[var(--c-emerald)] border-t-transparent rounded-full animate-spin"></div>
      </div>
    {:else if auth.user}
      <!-- User info -->
      <div class="card p-4">
        <div class="flex items-center gap-4">
          <div class="w-14 h-14 rounded-full bg-emerald-100 flex items-center justify-center text-emerald-700 font-bold text-xl">
            {auth.user.name.split(' ').map(n => n[0]).join('')}
          </div>
          <div>
            <h2 class="font-bold text-gray-900 text-lg">{auth.user.name}</h2>
            <p class="text-sm text-gray-500">{auth.user.phone}</p>
            <div class="flex gap-2 mt-1">
              {#each auth.user.roles as role}
                <Badge color="blue">{role.replace('_', ' ')}</Badge>
              {/each}
            </div>
          </div>
        </div>
      </div>

      <!-- Household -->
      {#if household}
        <div class="card p-4">
          <h3 class="font-semibold text-gray-800 mb-3 flex items-center gap-2">
            <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" /></svg>
            Flat {household.flatNumber}
          </h3>
          <div class="space-y-2">
            {#each household.members as member}
              <div class="flex items-center justify-between py-1.5">
                <div class="flex items-center gap-3">
                  <div class="w-8 h-8 rounded-full bg-gray-100 flex items-center justify-center text-gray-600 text-xs font-semibold">
                    {member.name.split(' ').map(n => n[0]).join('')}
                  </div>
                  <div>
                    <p class="text-sm font-medium text-gray-900">{member.name}</p>
                    <p class="text-xs text-gray-400">{member.phone}</p>
                  </div>
                </div>
                <Badge color={member.role === 'flat_owner' ? 'blue' : 'gray'}>
                  {member.role === 'flat_owner' ? 'Owner' : 'Member'}
                </Badge>
              </div>
            {/each}
          </div>
        </div>
      {/if}

      <!-- Notifications -->
      <div class="card p-4">
        <h3 class="font-semibold text-gray-800 mb-3">Recent Notifications</h3>
        {#if notifications.length === 0}
          <p class="text-sm text-gray-400">No notifications</p>
        {:else}
          <div class="space-y-2 max-h-60 overflow-y-auto">
            {#each notifications.slice(0, 10) as notif (notif.id)}
              <button onclick={() => markNotifRead(notif.id)} class="w-full text-left p-2 rounded-lg transition-colors {notif.isRead ? 'bg-white' : 'bg-blue-50'}">
                <p class="text-sm {notif.isRead ? 'text-gray-600' : 'font-medium text-gray-900'}">{notif.titleKey.split('.').pop()?.replace(/_/g, ' ')}</p>
                <p class="text-xs text-gray-400 mt-0.5">{new Date(notif.createdAt).toLocaleString()}</p>
              </button>
            {/each}
          </div>
        {/if}
      </div>

      <!-- Support -->
      <div class="card p-4">
        <h3 class="font-semibold text-gray-800 mb-3">Support & Help</h3>
        {#if !showSupport}
          <Button variant="secondary" class="w-full" onclick={() => showSupport = true}>Report Issue / Get Help</Button>
        {:else}
          <div class="space-y-3">
            <select bind:value={ticketType} class="w-full p-2 border border-gray-200 rounded-lg text-sm">
              <option value="general">General Question</option>
              <option value="dispute">Order Dispute</option>
              <option value="bug">Bug Report</option>
            </select>
            <input bind:value={ticketSubject} placeholder="Subject" class="w-full p-2 border border-gray-200 rounded-lg text-sm" />
            <textarea bind:value={ticketDesc} placeholder="Describe your issue..." rows="3" class="w-full p-2 border border-gray-200 rounded-lg text-sm resize-none"></textarea>
            <div class="flex gap-2">
              <Button variant="primary" size="sm" onclick={submitTicket} loading={submitting}>Submit</Button>
              <Button variant="ghost" size="sm" onclick={() => showSupport = false}>Cancel</Button>
            </div>
          </div>
        {/if}
      </div>

      <!-- Logout -->
      <button onclick={handleLogout} class="w-full py-3 text-sm text-red-500 hover:text-red-700 border border-red-100 rounded-xl hover:bg-red-50 transition-colors">
        Logout
      </button>
    {/if}
  </main>
</div>
