<script lang="ts">
  import { goto } from "$app/navigation";
  import { auth } from "$lib/stores/auth.svelte";
  import Card from "$lib/components/Card.svelte";
  import Input from "$lib/components/Input.svelte";
  import Button from "$lib/components/Button.svelte";

  let flatNo = $state("");
  let name = $state("");
  let phone = $state("");
  let step = $state<"login" | "register">("login");
  let loading = $state(false);
  let error = $state("");

  async function handleLogin() {
    const val = flatNo.trim();
    if (!val) {
      error = "Please enter your flat number";
      return;
    }
    loading = true;
    error = "";

    try {
      const res = await fetch(`/api/users?flat_no=${encodeURIComponent(val)}`);
      const user = await res.json();

      if (user && user.id) {
        auth.login(user);
        goto("/");
      } else {
        step = "register";
      }
    } catch {
      error = "Something went wrong. Please try again.";
    } finally {
      loading = false;
    }
  }

  async function handleRegister() {
    if (!name.trim() || !phone.trim()) {
      error = "Please fill in all fields";
      return;
    }
    loading = true;
    error = "";

    try {
      const res = await fetch("/api/users", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name: name.trim(), flat_no: flatNo.trim(), phone: phone.trim() }),
      });
      const user = await res.json();
      auth.login(user);
      goto("/");
    } catch {
      error = "Something went wrong. Please try again.";
    } finally {
      loading = false;
    }
  }
</script>

<svelte:head>
  <title>Login - Lucky Store</title>
</svelte:head>

<div class="min-h-screen flex items-center justify-center px-4">
  <div class="w-full max-w-sm">
    <div class="text-center mb-8 animate-in">
      <div class="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-to-b from-[#1a7a5e] to-[var(--c-emerald)] text-white text-2xl font-bold mb-4 shadow-lg">
        L
      </div>
      <h1 class="text-2xl font-bold text-gray-800">Lucky Store</h1>
      <p class="text-gray-400 text-sm mt-1">Your neighbourhood grocery store</p>
    </div>

    <Card class="p-6 animate-in">
      {#if step === "login"}
        <h2 class="font-bold text-gray-800 mb-1">Welcome</h2>
        <p class="text-sm text-gray-400 mb-4">Enter your flat number to continue</p>

        <form onsubmit={(e) => { e.preventDefault(); handleLogin(); }}>
          <div class="space-y-3">
            <Input type="text" placeholder="Flat No. (e.g. B3-804)" bind:value={flatNo} />
          </div>

          {#if error}
            <p class="text-red-500 text-sm mt-2">{error}</p>
          {/if}

          <Button variant="primary" class="w-full py-3 mt-4 rounded-xl" disabled={loading}>
            {loading ? "Checking..." : "Continue"}
          </Button>
        </form>
      {:else}
        <h2 class="font-bold text-gray-800 mb-1">New here?</h2>
        <p class="text-sm text-gray-400 mb-4">Set up your account for <span class="font-semibold text-gray-600">{flatNo}</span></p>

        <form onsubmit={(e) => { e.preventDefault(); handleRegister(); }}>
          <div class="space-y-3">
            <Input type="text" placeholder="Your Name" bind:value={name} />
            <Input type="tel" placeholder="Phone Number" bind:value={phone} />
          </div>

          {#if error}
            <p class="text-red-500 text-sm mt-2">{error}</p>
          {/if}

          <Button variant="primary" class="w-full py-3 mt-4 rounded-xl" disabled={loading}>
            {loading ? "Creating Account..." : "Get Started"}
          </Button>
        </form>

        <button class="w-full text-center text-sm text-gray-400 mt-3 hover:text-gray-600" onclick={() => { step = 'login'; error = ''; }}>
          Back
        </button>
      {/if}
    </Card>
  </div>
</div>
