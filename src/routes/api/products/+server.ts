import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";
import { seedProducts } from "$lib/seed";

let seeded = false;

export const GET: RequestHandler = ({ url }) => {
  if (!seeded) { seedProducts(); seeded = true; }
  const category = url.searchParams.get("category");
  const search = url.searchParams.get("search");

  let products;
  if (search) {
    products = db
      .prepare("SELECT * FROM products WHERE name LIKE ? ORDER BY category, name")
      .all(`%${search}%`);
  } else if (category) {
    products = db
      .prepare("SELECT * FROM products WHERE category = ? ORDER BY name")
      .all(category);
  } else {
    products = db.prepare("SELECT * FROM products ORDER BY category, name").all();
  }

  return json(products);
};

export const POST: RequestHandler = async ({ request }) => {
  const body = await request.json();
  const { name, price, category, unit } = body;

  const result = db
    .prepare("INSERT INTO products (name, price, category, unit) VALUES (?, ?, ?, ?)")
    .run(name, price, category, unit || "piece");

  const product = db.prepare("SELECT * FROM products WHERE id = ?").get(result.lastInsertRowid);
  return json(product, { status: 201 });
};
