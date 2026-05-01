module.exports = [
"[externals]/next/dist/compiled/next-server/app-route-turbo.runtime.dev.js [external] (next/dist/compiled/next-server/app-route-turbo.runtime.dev.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/compiled/next-server/app-route-turbo.runtime.dev.js", () => require("next/dist/compiled/next-server/app-route-turbo.runtime.dev.js"));

module.exports = mod;
}),
"[externals]/next/dist/compiled/@opentelemetry/api [external] (next/dist/compiled/@opentelemetry/api, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/compiled/@opentelemetry/api", () => require("next/dist/compiled/@opentelemetry/api"));

module.exports = mod;
}),
"[externals]/next/dist/compiled/next-server/app-page-turbo.runtime.dev.js [external] (next/dist/compiled/next-server/app-page-turbo.runtime.dev.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/compiled/next-server/app-page-turbo.runtime.dev.js", () => require("next/dist/compiled/next-server/app-page-turbo.runtime.dev.js"));

module.exports = mod;
}),
"[externals]/next/dist/server/app-render/work-unit-async-storage.external.js [external] (next/dist/server/app-render/work-unit-async-storage.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/server/app-render/work-unit-async-storage.external.js", () => require("next/dist/server/app-render/work-unit-async-storage.external.js"));

module.exports = mod;
}),
"[externals]/next/dist/server/app-render/work-async-storage.external.js [external] (next/dist/server/app-render/work-async-storage.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/server/app-render/work-async-storage.external.js", () => require("next/dist/server/app-render/work-async-storage.external.js"));

module.exports = mod;
}),
"[externals]/next/dist/shared/lib/no-fallback-error.external.js [external] (next/dist/shared/lib/no-fallback-error.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/shared/lib/no-fallback-error.external.js", () => require("next/dist/shared/lib/no-fallback-error.external.js"));

module.exports = mod;
}),
"[externals]/next/dist/server/app-render/after-task-async-storage.external.js [external] (next/dist/server/app-render/after-task-async-storage.external.js, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("next/dist/server/app-render/after-task-async-storage.external.js", () => require("next/dist/server/app-render/after-task-async-storage.external.js"));

module.exports = mod;
}),
"[externals]/path [external] (path, cjs)", ((__turbopack_context__, module, exports) => {

const mod = __turbopack_context__.x("path", () => require("path"));

module.exports = mod;
}),
"[project]/src/lib/db.ts [app-route] (ecmascript)", ((__turbopack_context__) => {
"use strict";

__turbopack_context__.s([
    "default",
    ()=>__TURBOPACK__default__export__
]);
var __TURBOPACK__imported__module__$5b$externals$5d2f$better$2d$sqlite3__$5b$external$5d$__$28$better$2d$sqlite3$2c$__cjs$2c$__$5b$project$5d2f$node_modules$2f$better$2d$sqlite3$29$__ = __turbopack_context__.i("[externals]/better-sqlite3 [external] (better-sqlite3, cjs, [project]/node_modules/better-sqlite3)");
var __TURBOPACK__imported__module__$5b$externals$5d2f$path__$5b$external$5d$__$28$path$2c$__cjs$29$__ = __turbopack_context__.i("[externals]/path [external] (path, cjs)");
;
;
const dbPath = __TURBOPACK__imported__module__$5b$externals$5d2f$path__$5b$external$5d$__$28$path$2c$__cjs$29$__["default"].join(process.cwd(), "lucky_store.db");
const db = new __TURBOPACK__imported__module__$5b$externals$5d2f$better$2d$sqlite3__$5b$external$5d$__$28$better$2d$sqlite3$2c$__cjs$2c$__$5b$project$5d2f$node_modules$2f$better$2d$sqlite3$29$__["default"](dbPath);
db.pragma("journal_mode = WAL");
db.pragma("foreign_keys = ON");
db.exec(`
  CREATE TABLE IF NOT EXISTS products (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    price REAL NOT NULL,
    category TEXT NOT NULL,
    unit TEXT NOT NULL DEFAULT 'piece',
    in_stock INTEGER NOT NULL DEFAULT 1,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
  );

  CREATE TABLE IF NOT EXISTS users (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    name TEXT NOT NULL,
    flat_no TEXT NOT NULL UNIQUE,
    phone TEXT NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now'))
  );

  CREATE TABLE IF NOT EXISTS orders (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    status TEXT NOT NULL DEFAULT 'pending',
    payment_method TEXT NOT NULL DEFAULT 'cod',
    total REAL NOT NULL DEFAULT 0,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (user_id) REFERENCES users(id)
  );

  CREATE TABLE IF NOT EXISTS order_items (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    order_id INTEGER NOT NULL,
    product_id INTEGER NOT NULL,
    quantity INTEGER NOT NULL DEFAULT 1,
    price REAL NOT NULL,
    FOREIGN KEY (order_id) REFERENCES orders(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
  );

  CREATE TABLE IF NOT EXISTS tab_payments (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    user_id INTEGER NOT NULL,
    amount REAL NOT NULL,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (user_id) REFERENCES users(id)
  );
`);
const __TURBOPACK__default__export__ = db;
}),
"[project]/src/lib/seed.ts [app-route] (ecmascript)", ((__turbopack_context__) => {
"use strict";

__turbopack_context__.s([
    "seedProducts",
    ()=>seedProducts,
    "seedUsers",
    ()=>seedUsers
]);
var __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/src/lib/db.ts [app-route] (ecmascript)");
;
const products = [
    // Dairy
    {
        name: "Amul Taaza Milk 500ml",
        price: 29,
        category: "Dairy",
        unit: "pack"
    },
    {
        name: "Mother Dairy Milk 500ml",
        price: 30,
        category: "Dairy",
        unit: "pack"
    },
    {
        name: "Amul Gold Milk 500ml",
        price: 34,
        category: "Dairy",
        unit: "pack"
    },
    {
        name: "Amul Butter 100g",
        price: 56,
        category: "Dairy",
        unit: "pack"
    },
    {
        name: "Amul Curd 400g",
        price: 35,
        category: "Dairy",
        unit: "pack"
    },
    {
        name: "Mother Dairy Dahi 400g",
        price: 35,
        category: "Dairy",
        unit: "pack"
    },
    {
        name: "Paneer 200g",
        price: 80,
        category: "Dairy",
        unit: "pack"
    },
    // Eggs
    {
        name: "Eggs (6 pack)",
        price: 42,
        category: "Eggs",
        unit: "pack"
    },
    {
        name: "Eggs (12 pack)",
        price: 78,
        category: "Eggs",
        unit: "pack"
    },
    // Atta & Rice
    {
        name: "Aashirvaad Atta 5kg",
        price: 270,
        category: "Atta & Rice",
        unit: "bag"
    },
    {
        name: "Fortune Chakki Atta 5kg",
        price: 255,
        category: "Atta & Rice",
        unit: "bag"
    },
    {
        name: "India Gate Basmati Rice 1kg",
        price: 160,
        category: "Atta & Rice",
        unit: "bag"
    },
    {
        name: "Daawat Rozana Rice 5kg",
        price: 350,
        category: "Atta & Rice",
        unit: "bag"
    },
    // Dal & Pulses
    {
        name: "Toor Dal 1kg",
        price: 145,
        category: "Dal & Pulses",
        unit: "bag"
    },
    {
        name: "Moong Dal 1kg",
        price: 130,
        category: "Dal & Pulses",
        unit: "bag"
    },
    {
        name: "Chana Dal 1kg",
        price: 95,
        category: "Dal & Pulses",
        unit: "bag"
    },
    {
        name: "Rajma 500g",
        price: 85,
        category: "Dal & Pulses",
        unit: "bag"
    },
    // Oil
    {
        name: "Fortune Sunflower Oil 1L",
        price: 140,
        category: "Oil & Ghee",
        unit: "bottle"
    },
    {
        name: "Saffola Gold Oil 1L",
        price: 185,
        category: "Oil & Ghee",
        unit: "bottle"
    },
    {
        name: "Amul Ghee 500ml",
        price: 290,
        category: "Oil & Ghee",
        unit: "jar"
    },
    // Spices & Masala
    {
        name: "MDH Chana Masala 100g",
        price: 65,
        category: "Spices",
        unit: "pack"
    },
    {
        name: "Everest Garam Masala 100g",
        price: 72,
        category: "Spices",
        unit: "pack"
    },
    {
        name: "Haldi Powder 100g",
        price: 30,
        category: "Spices",
        unit: "pack"
    },
    {
        name: "Red Chilli Powder 100g",
        price: 35,
        category: "Spices",
        unit: "pack"
    },
    {
        name: "Salt 1kg",
        price: 22,
        category: "Spices",
        unit: "bag"
    },
    {
        name: "Sugar 1kg",
        price: 45,
        category: "Spices",
        unit: "bag"
    },
    // Snacks
    {
        name: "Lays Classic Salted 52g",
        price: 20,
        category: "Snacks",
        unit: "pack"
    },
    {
        name: "Kurkure Masala Munch 75g",
        price: 20,
        category: "Snacks",
        unit: "pack"
    },
    {
        name: "Parle-G Biscuits 250g",
        price: 25,
        category: "Snacks",
        unit: "pack"
    },
    {
        name: "Britannia Good Day 75g",
        price: 30,
        category: "Snacks",
        unit: "pack"
    },
    {
        name: "Maggi Noodles 4-pack",
        price: 56,
        category: "Snacks",
        unit: "pack"
    },
    // Beverages
    {
        name: "Coca Cola 750ml",
        price: 38,
        category: "Beverages",
        unit: "bottle"
    },
    {
        name: "Pepsi 750ml",
        price: 38,
        category: "Beverages",
        unit: "bottle"
    },
    {
        name: "Tata Tea Gold 250g",
        price: 120,
        category: "Beverages",
        unit: "pack"
    },
    {
        name: "Nescafe Classic 50g",
        price: 155,
        category: "Beverages",
        unit: "jar"
    },
    {
        name: "Bisleri Water 1L",
        price: 20,
        category: "Beverages",
        unit: "bottle"
    },
    // Bread & Bakery
    {
        name: "Britannia Bread",
        price: 40,
        category: "Bread",
        unit: "pack"
    },
    {
        name: "Pav (4 pack)",
        price: 20,
        category: "Bread",
        unit: "pack"
    },
    // Personal Care
    {
        name: "Surf Excel 1kg",
        price: 125,
        category: "Household",
        unit: "pack"
    },
    {
        name: "Vim Bar 200g",
        price: 18,
        category: "Household",
        unit: "bar"
    },
    {
        name: "Lizol Floor Cleaner 500ml",
        price: 99,
        category: "Household",
        unit: "bottle"
    }
];
function seedProducts() {
    const existing = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("SELECT COUNT(*) as count FROM products").get();
    if (existing.count > 0) return;
    const insert = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("INSERT INTO products (name, price, category, unit) VALUES (?, ?, ?, ?)");
    const insertMany = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].transaction(()=>{
        for (const p of products){
            insert.run(p.name, p.price, p.category, p.unit);
        }
    });
    insertMany();
}
function seedUsers() {
    const existing = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("SELECT COUNT(*) as count FROM users").get();
    if (existing.count > 0) return;
    const insert = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("INSERT INTO users (name, flat_no, phone) VALUES (?, ?, ?)");
    const users = [
        {
            name: "Rahul Sharma",
            flat_no: "A-101",
            phone: "9876543210"
        },
        {
            name: "Priya Gupta",
            flat_no: "B-204",
            phone: "9876543211"
        },
        {
            name: "Amit Patel",
            flat_no: "C-302",
            phone: "9876543212"
        },
        {
            name: "Neha Singh",
            flat_no: "A-405",
            phone: "9876543213"
        }
    ];
    const insertMany = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].transaction(()=>{
        for (const u of users){
            insert.run(u.name, u.flat_no, u.phone);
        }
    });
    insertMany();
}
}),
"[project]/src/app/api/products/route.ts [app-route] (ecmascript)", ((__turbopack_context__) => {
"use strict";

__turbopack_context__.s([
    "GET",
    ()=>GET,
    "POST",
    ()=>POST
]);
var __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$server$2e$js__$5b$app$2d$route$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/node_modules/next/server.js [app-route] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/src/lib/db.ts [app-route] (ecmascript)");
var __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$seed$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__ = __turbopack_context__.i("[project]/src/lib/seed.ts [app-route] (ecmascript)");
;
;
;
(0, __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$seed$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["seedProducts"])();
async function GET(request) {
    const { searchParams } = new URL(request.url);
    const category = searchParams.get("category");
    const search = searchParams.get("search");
    let products;
    if (search) {
        products = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("SELECT * FROM products WHERE name LIKE ? ORDER BY category, name").all(`%${search}%`);
    } else if (category) {
        products = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("SELECT * FROM products WHERE category = ? ORDER BY name").all(category);
    } else {
        products = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("SELECT * FROM products ORDER BY category, name").all();
    }
    return __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$server$2e$js__$5b$app$2d$route$5d$__$28$ecmascript$29$__["NextResponse"].json(products);
}
async function POST(request) {
    const body = await request.json();
    const { name, price, category, unit } = body;
    const result = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("INSERT INTO products (name, price, category, unit) VALUES (?, ?, ?, ?)").run(name, price, category, unit || "piece");
    const product = __TURBOPACK__imported__module__$5b$project$5d2f$src$2f$lib$2f$db$2e$ts__$5b$app$2d$route$5d$__$28$ecmascript$29$__["default"].prepare("SELECT * FROM products WHERE id = ?").get(result.lastInsertRowid);
    return __TURBOPACK__imported__module__$5b$project$5d2f$node_modules$2f$next$2f$server$2e$js__$5b$app$2d$route$5d$__$28$ecmascript$29$__["NextResponse"].json(product, {
        status: 201
    });
}
}),
];

//# sourceMappingURL=%5Broot-of-the-server%5D__0udue~t._.js.map