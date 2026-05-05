<script lang="ts">
  let announcements = $state([
    { id: '1', title: 'Maintenance Window', body: 'Platform will be under maintenance on Sunday 6am-8am.', createdAt: '2026-05-03T10:00:00Z', active: true },
    { id: '2', title: 'New Shop: Fresh Bakes', body: 'A new bakery has been approved and is now accepting orders!', createdAt: '2026-04-30T12:00:00Z', active: true },
  ]);
  let showForm = $state(false);
  let newTitle = $state('');
  let newBody = $state('');

  function addAnnouncement() {
    if (!newTitle.trim() || !newBody.trim()) return;
    announcements = [{
      id: crypto.randomUUID(),
      title: newTitle,
      body: newBody,
      createdAt: new Date().toISOString(),
      active: true,
    }, ...announcements];
    newTitle = '';
    newBody = '';
    showForm = false;
  }
</script>

<div class="p-6">
  <div class="flex items-center justify-between mb-6">
    <h1 class="text-2xl font-bold text-gray-900">Announcements</h1>
    <button onclick={() => showForm = !showForm} class="px-4 py-2 bg-purple-600 text-white text-sm rounded-lg hover:bg-purple-700">+ New</button>
  </div>

  {#if showForm}
    <div class="bg-white rounded-xl p-5 border border-gray-100 mb-6">
      <div class="space-y-3">
        <input bind:value={newTitle} placeholder="Title" class="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm" />
        <textarea bind:value={newBody} placeholder="Announcement body..." rows="3" class="w-full px-3 py-2 border border-gray-200 rounded-lg text-sm resize-none"></textarea>
        <div class="flex gap-2">
          <button onclick={addAnnouncement} class="px-4 py-2 bg-purple-600 text-white text-sm rounded-lg">Publish</button>
          <button onclick={() => showForm = false} class="px-4 py-2 text-gray-500 text-sm">Cancel</button>
        </div>
      </div>
    </div>
  {/if}

  <div class="space-y-3">
    {#each announcements as ann (ann.id)}
      <div class="bg-white rounded-xl p-5 border border-gray-100">
        <div class="flex justify-between items-start">
          <div>
            <h3 class="font-semibold text-gray-900">{ann.title}</h3>
            <p class="text-sm text-gray-600 mt-1">{ann.body}</p>
            <p class="text-xs text-gray-400 mt-2">{new Date(ann.createdAt).toLocaleDateString()}</p>
          </div>
          <span class="px-2 py-0.5 rounded-full text-xs {ann.active ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}">{ann.active ? 'Active' : 'Archived'}</span>
        </div>
      </div>
    {/each}
  </div>
</div>
