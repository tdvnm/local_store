import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";

export const GET: RequestHandler = ({ url }) => {
  const userId = url.searchParams.get("user_id");
  if (!userId) return json([]);

  const routines = db
    .prepare(
      `SELECT r.*, p.name as product_name, p.price, p.category, p.unit
       FROM routines r JOIN products p ON r.product_id = p.id
       WHERE r.user_id = ? ORDER BY r.created_at DESC`
    )
    .all(Number(userId));

  return json(routines);
};

export const POST: RequestHandler = async ({ request }) => {
  const { user_id, product_id, quantity, frequency } = await request.json();

  const result = db
    .prepare(
      "INSERT INTO routines (user_id, product_id, quantity, frequency) VALUES (?, ?, ?, ?)"
    )
    .run(user_id, product_id, quantity || 1, frequency || "daily");

  const routine = db
    .prepare(
      `SELECT r.*, p.name as product_name, p.price, p.category, p.unit
       FROM routines r JOIN products p ON r.product_id = p.id
       WHERE r.id = ?`
    )
    .get(result.lastInsertRowid);

  return json(routine, { status: 201 });
};
