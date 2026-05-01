import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";

export const PATCH: RequestHandler = async ({ params, request }) => {
  const id = Number(params.id);
  const body = await request.json();

  const fields: string[] = [];
  const values: (string | number)[] = [];

  if (body.name !== undefined) { fields.push("name = ?"); values.push(body.name); }
  if (body.price !== undefined) { fields.push("price = ?"); values.push(body.price); }
  if (body.category !== undefined) { fields.push("category = ?"); values.push(body.category); }
  if (body.unit !== undefined) { fields.push("unit = ?"); values.push(body.unit); }
  if (body.in_stock !== undefined) { fields.push("in_stock = ?"); values.push(body.in_stock ? 1 : 0); }

  if (fields.length === 0) {
    return json({ error: "No fields to update" }, { status: 400 });
  }

  values.push(id);
  db.prepare(`UPDATE products SET ${fields.join(", ")} WHERE id = ?`).run(...values);

  const product = db.prepare("SELECT * FROM products WHERE id = ?").get(id);
  return json(product);
};

export const DELETE: RequestHandler = ({ params }) => {
  db.prepare("DELETE FROM products WHERE id = ?").run(Number(params.id));
  return json({ ok: true });
};
