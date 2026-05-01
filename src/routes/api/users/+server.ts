import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";
import { seedUsers } from "$lib/seed";

let seeded = false;

export const GET: RequestHandler = ({ url }) => {
  if (!seeded) { seedUsers(); seeded = true; }
  const flatNo = url.searchParams.get("flat_no");

  if (flatNo) {
    const user = db.prepare("SELECT * FROM users WHERE flat_no = ?").get(flatNo);
    return json(user || null);
  }

  const users = db.prepare("SELECT * FROM users ORDER BY flat_no").all();
  return json(users);
};

export const POST: RequestHandler = async ({ request }) => {
  const body = await request.json();
  const { name, flat_no, phone } = body;

  const existing = db.prepare("SELECT * FROM users WHERE flat_no = ?").get(flat_no);
  if (existing) {
    return json(existing);
  }

  const result = db
    .prepare("INSERT INTO users (name, flat_no, phone) VALUES (?, ?, ?)")
    .run(name, flat_no, phone);

  const user = db.prepare("SELECT * FROM users WHERE id = ?").get(result.lastInsertRowid);
  return json(user, { status: 201 });
};
