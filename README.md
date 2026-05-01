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

## Tech

- SvelteKit + Svelte 5
- SQLite (better-sqlite3)
- Tailwind CSS v4
- Archia font (self-hosted)

---

## Features

### Buyer
- Browse & search products with category filters
- Favourites (heart toggle, filter by favourites)
- Cart with delivery mode: **Urgent** / **Scheduled** / **Pickup**
- Payment: **Cash on Delivery** or **Monthly Tab**
- Order history & live status tracking
- Recurring orders (Routines)
- Print receipts

### Seller
- Inventory management (name, price, company, category, stock mode)
- Stock modes: **known quantity** (auto-deducts) or **infinite** (requires confirmation)
- Order dashboard with status updates (packed, out for delivery, ready for pickup)
- Percentage discounts
- Role management (seller, delivery boy)
- Monthly tab tracking
