import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";

export const GET: RequestHandler = () => {
  const tabs = db.prepare(`
    SELECT
      u.id as user_id,
      u.name,
      u.flat_no,
      u.phone,
      COALESCE(SUM(CASE WHEN o.payment_method = 'tab' AND o.status != 'cancelled' THEN o.total ELSE 0 END), 0) as total_tab,
      COALESCE((SELECT SUM(tp.amount) FROM tab_payments tp WHERE tp.user_id = u.id), 0) as total_paid
    FROM users u
    LEFT JOIN orders o ON o.user_id = u.id
    GROUP BY u.id
    HAVING total_tab > 0 OR total_paid > 0
    ORDER BY u.flat_no
  `).all() as Record<string, number | string>[];

  const result = tabs.map((t) => ({
    ...t,
    balance: (t.total_tab as number) - (t.total_paid as number),
  }));

  return json(result);
};

export const POST: RequestHandler = async ({ request }) => {
  const body = await request.json();
  const { user_id, amount } = body;

  db.prepare("INSERT INTO tab_payments (user_id, amount) VALUES (?, ?)").run(user_id, amount);

  return json({ ok: true }, { status: 201 });
};
