import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";

export const GET: RequestHandler = ({ url }) => {
  const userId = url.searchParams.get("user_id");
  if (!userId) return json([]);

  const favs = db
    .prepare(
      `SELECT f.product_id, p.name, p.price, p.category, p.unit, p.stock_mode, p.in_stock
       FROM favourites f JOIN products p ON f.product_id = p.id
       WHERE f.user_id = ? ORDER BY f.created_at DESC`
    )
    .all(Number(userId));

  return json(favs);
};

export const POST: RequestHandler = async ({ request }) => {
  const { user_id, product_id } = await request.json();

  try {
    db.prepare(
      "INSERT INTO favourites (user_id, product_id) VALUES (?, ?)"
    ).run(user_id, product_id);
  } catch {
    /* duplicate — ignore */
  }

  return json({ ok: true }, { status: 201 });
};

export const DELETE: RequestHandler = async ({ request }) => {
  const { user_id, product_id } = await request.json();

  db.prepare(
    "DELETE FROM favourites WHERE user_id = ? AND product_id = ?"
  ).run(user_id, product_id);

  return json({ ok: true });
};
