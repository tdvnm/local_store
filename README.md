# Lucky Store

A grocery store platform connecting buyers and sellers with real-time order management, flexible delivery options, and monthly tab billing.

## Setup

```bash
npm install
npm run dev
```

Open the URL shown in the terminal (usually `http://localhost:5173`).

The database (SQLite) and seed data are created automatically on first run — no setup needed.

## Usage

1. You'll land on the **login page** — enter any flat number (e.g. `A-101`) to log in with a seeded user, or enter a new one to register
2. Browse products, add to cart, place orders
3. Use the bottom nav to access **Routines**, **Orders**, and **Tab**
4. Admin dashboard is at `/admin`

## Build & Deploy

```bash
npm run build
node build/index.js
```

Set `PORT` env var to change the default port (3000):

```bash
PORT=8080 node build/index.js
```

## Quick Hosting (for testing)

The easiest way to share it with others for testing:

### ngrok (expose localhost)

```bash
npm run dev
# In another terminal:
ngrok http 5173
```

Gives you a public URL like `https://xxxx.ngrok-free.app` — share it with anyone.

Install ngrok: `nix-shell -p ngrok` or from [ngrok.com](https://ngrok.com)

### Cloudflare Tunnel (no account needed)

```bash
npm run dev
# In another terminal:
cloudflared tunnel --url http://localhost:5173
```

Gives you a temporary public `https://xxxx.trycloudflare.com` URL.

Install: `nix-shell -p cloudflared`

### Local network

```bash
npm run dev -- --host
```

This exposes the server on your local network. Other devices on the same WiFi can access it via your IP (shown in the terminal), e.g. `http://192.168.1.5:5173`.

## Tech

- SvelteKit + Svelte 5
- SQLite (better-sqlite3)
- Tailwind CSS v4
- Archia font (self-hosted)

---

## Roles

### Buyer

#### Shopping
- Browse and search for grocery products
- Add items to cart
- Mark items as favourites for quick access

#### Routines (Recurring Orders)
- Set up recurring/scheduled orders (e.g., milk every morning)
- Manage and edit active routines

#### Ordering & Delivery Options
When placing an order, the buyer can choose from:
- **Urgent** — ASAP delivery
- **Scheduled** — Deliver before a specified time (e.g., before 6 PM)
- **Pickup** — Seller packs the order; buyer picks it up from the shop

#### Order Tracking
- Track the status of current orders in real-time
- View full order history

#### Monthly Tab
- Orders accumulate on a monthly tab instead of paying per-order
- View current tab total and breakdown at any time

#### Stock Confirmation
- Items with known stock are available immediately
- Items marked as "infinite" stock by the seller require confirmation — the buyer is notified that their order is waiting for seller confirmation before processing

#### Other
- Login / Logout
- Print orders or receipts
- Username format: `B3_804`

---

### Seller

#### Shop Setup
- Register a new shop on the platform
- Manage shop profile

#### Inventory Management
- Add, edit, and remove products using reactive forms
- Product fields: **name**, **price**, **company**, **classification**
- Classify and categorise products into groups
- **Stock modes:**
  - **Known quantity** — Seller specifies the count (e.g., 100 units of milk); stock deducts automatically with each order
  - **Infinite** — For items the seller can't easily track (e.g., coffee sachets, juice); orders for these items require manual seller confirmation before processing
- Apply discounts using percentage

#### Orders
- View all incoming orders sorted in a dashboard
- Full order tracking with status updates
- Send notifications to buyers at each stage:
  - Packed
  - Out for delivery
  - Ready for pickup
- Delivery confirmation upon completion

#### Roles & Staff
- Assign roles: **Main Seller**, **Delivery Boy**
- Manage staff and their permissions

#### Database
- Full data management for products, orders, and staff
- Everything is manageable: roles, inventory, pricing
