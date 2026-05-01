import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";

export const PATCH: RequestHandler = async ({ params, request }) => {
  const id = Number(params.id);
  const body = await request.json();
  const { status } = body;

  const validStatuses = ["pending", "confirmed", "packed", "out_for_delivery", "ready_for_pickup", "delivered", "cancelled"];
  if (!validStatuses.includes(status)) {
    return json({ error: "Invalid status" }, { status: 400 });
  }

  db.prepare("UPDATE orders SET status = ? WHERE id = ?").run(status, id);

  const order = db.prepare("SELECT * FROM orders WHERE id = ?").get(id);
  return json(order);
};
