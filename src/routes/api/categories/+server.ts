import { json } from "@sveltejs/kit";
import type { RequestHandler } from "./$types";
import db from "$lib/db";
import { seedProducts } from "$lib/seed";

let seeded = false;

export const GET: RequestHandler = () => {
  if (!seeded) { seedProducts(); seeded = true; }
  const categories = db
    .prepare("SELECT DISTINCT category FROM products ORDER BY category")
    .all() as { category: string }[];

  return json(categories.map((c) => c.category));
};
