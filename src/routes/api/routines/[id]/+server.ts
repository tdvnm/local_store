import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";

export const PATCH: RequestHandler = async ({ params, request }) => {
  const id = Number(params.id);
  const body = await request.json();

  const fields: string[] = [];
  const values: (string | number)[] = [];

  if (body.quantity !== undefined) {
    fields.push("quantity = ?");
    values.push(body.quantity);
  }
  if (body.frequency !== undefined) {
    fields.push("frequency = ?");
    values.push(body.frequency);
  }
  if (body.active !== undefined) {
    fields.push("active = ?");
    values.push(body.active ? 1 : 0);
  }

  if (fields.length === 0) {
    return json({ error: "No fields to update" }, { status: 400 });
  }

  values.push(id);
  db.prepare(`UPDATE routines SET ${fields.join(", ")} WHERE id = ?`).run(
    ...values
  );

  const routine = db
    .prepare(
      `SELECT r.*, p.name as product_name, p.price, p.category, p.unit
       FROM routines r JOIN products p ON r.product_id = p.id
       WHERE r.id = ?`
    )
    .get(id);

  return json(routine);
};

export const DELETE: RequestHandler = ({ params }) => {
  db.prepare("DELETE FROM routines WHERE id = ?").run(Number(params.id));
  return json({ ok: true });
};
