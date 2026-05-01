import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";

export const GET: RequestHandler = ({ url }) => {
  const userId = url.searchParams.get("user_id");
  const status = url.searchParams.get("status");

  let query = `
    SELECT o.*, u.name as user_name, u.flat_no
    FROM orders o
    JOIN users u ON o.user_id = u.id
  `;
  const conditions: string[] = [];
  const values: (string | number)[] = [];

  if (userId) { conditions.push("o.user_id = ?"); values.push(Number(userId)); }
  if (status) { conditions.push("o.status = ?"); values.push(status); }

  if (conditions.length > 0) {
    query += " WHERE " + conditions.join(" AND ");
  }
  query += " ORDER BY o.created_at DESC";

  const orders = db.prepare(query).all(...values) as Record<string, unknown>[];

  const itemsStmt = db.prepare(`
    SELECT oi.*, p.name as product_name, p.category
    FROM order_items oi
    JOIN products p ON oi.product_id = p.id
    WHERE oi.order_id = ?
  `);

  for (const order of orders) {
    order.items = itemsStmt.all(order.id);
  }

  return json(orders);
};

export const POST: RequestHandler = async ({ request }) => {
  const body = await request.json();
  const { user_id, payment_method, delivery_mode, scheduled_time, items } = body;

  let total = 0;
  const productPrices: { product_id: number; quantity: number; price: number }[] = [];

  for (const item of items) {
    const product = db.prepare("SELECT * FROM products WHERE id = ?").get(item.product_id) as { price: number } | undefined;
    if (!product) continue;
    const price = product.price * item.quantity;
    total += price;
    productPrices.push({ product_id: item.product_id, quantity: item.quantity, price: product.price });
  }

  const result = db
    .prepare("INSERT INTO orders (user_id, payment_method, delivery_mode, scheduled_time, total, status) VALUES (?, ?, ?, ?, ?, 'pending')")
    .run(user_id, payment_method || "cod", delivery_mode || "delivery", scheduled_time || null, total);

  const orderId = result.lastInsertRowid;

  const insertItem = db.prepare(
    "INSERT INTO order_items (order_id, product_id, quantity, price) VALUES (?, ?, ?, ?)"
  );

  const insertAll = db.transaction(() => {
    for (const item of productPrices) {
      insertItem.run(orderId, item.product_id, item.quantity, item.price);
    }
  });
  insertAll();

  return json({ id: orderId, total, status: "pending" }, { status: 201 });
};
