import db from "./db";

const products = [
  // Dairy
  { name: "Amul Taaza Milk 500ml", price: 29, category: "Dairy", unit: "pack", image: "amul_taza_tonedmilk.avif" },
  { name: "Amul Cow Milk 500ml", price: 28, category: "Dairy", unit: "pack", image: "amulcowmilk.avif" },
  { name: "Amul Gold Milk 500ml", price: 34, category: "Dairy", unit: "pack", image: "amul_gold_full_cream.avif" },
  { name: "Amul Buffalo A2 Milk 500ml", price: 35, category: "Dairy", unit: "pack", image: "amulbuffaloA2.avif" },
  { name: "Mother Dairy Toned Milk 500ml", price: 25, category: "Dairy", unit: "pack", image: "motehrdairytonedmilk.avif" },
  { name: "Mother Dairy Cow Milk 500ml", price: 27, category: "Dairy", unit: "pack", image: "motherdaircowmilk.avif" },
  { name: "Mother Dairy Full Cream 500ml", price: 32, category: "Dairy", unit: "pack", image: "motherdairyfullcream.avif" },
  { name: "Mother Dairy Toned Cow Milk 500ml", price: 26, category: "Dairy", unit: "pack", image: "motherdairytonedcowmilk.avif" },
  { name: "Amul Salted Butter 100g", price: 56, category: "Dairy", unit: "pack", image: "amulsaltedbutter100g.avif" },
  { name: "Amul Salted Butter 200g", price: 105, category: "Dairy", unit: "pack", image: "amulsaledbutter200gs.avif" },
  { name: "Amul Unsalted Butter 100g", price: 56, category: "Dairy", unit: "pack", image: "amulunslatedbutter100g.avif" },
  { name: "Amul Garlic & Herbs Butter 100g", price: 60, category: "Dairy", unit: "pack", image: "amulgarlicandherbsbutter100gs.avif" },
  { name: "Mother Dairy Salted Butter 100g", price: 52, category: "Dairy", unit: "pack", image: "motherdairysaltedbutter100g.avif" },
  { name: "Mother Dairy Salted Butter 500g", price: 245, category: "Dairy", unit: "pack", image: "motherdairysaltedutter500g.avif" },
  { name: "Amul Masti Dahi 390g", price: 35, category: "Dairy", unit: "pack", image: "amulmastipouchcurd390gs.avif" },
  { name: "Amul Masti Dahi 1kg", price: 80, category: "Dairy", unit: "pack", image: "amulmastipouchcurd1kg.avif" },
  { name: "Mother Dairy Classic Curd 400g", price: 35, category: "Dairy", unit: "pack", image: "otherdairyclassiccurdcup400gs.avif" },
  { name: "Country Delight Low Fat Dahi 400g", price: 42, category: "Dairy", unit: "pack", image: "countrydelightlowfatpouchdahi400g.avif" },
  { name: "Paneer 200g", price: 80, category: "Dairy", unit: "pack" },

  // Eggs
  { name: "Eggs (6 pack)", price: 42, category: "Eggs", unit: "pack" },
  { name: "Eggs (12 pack)", price: 78, category: "Eggs", unit: "pack" },

  // Bread & Bakery
  { name: "English Oven White Bread", price: 45, category: "Bread", unit: "pack", image: "englishovenwhitebread.png" },
  { name: "English Oven Atta Bread 400g", price: 50, category: "Bread", unit: "pack", image: "englishover100percentatta400gs.avif" },
  { name: "Harvest Gold White Bread 350g", price: 40, category: "Bread", unit: "pack", image: "harvestgoldwhite350gs.avif" },
  { name: "Harvest Gold Brown Bread 450g", price: 50, category: "Bread", unit: "pack", image: "harvestgoldheartybrownbread450gs.avif" },
  { name: "Harvest Gold Atta Bread 400g", price: 45, category: "Bread", unit: "pack", image: "harvestgoldatta400gs.avif" },
  { name: "Britannia Atta No Maida 450g", price: 35, category: "Bread", unit: "pack", image: "brittaniaattanomaida450gs.avif" },
  { name: "Pav (4 pack)", price: 20, category: "Bread", unit: "pack", image: "englishovenpav.avif" },
  { name: "Bonn Extra Soft Pav Bread", price: 30, category: "Bread", unit: "pack", image: "bonnextrasoftpavbread.avif" },
  { name: "Bonn High Fibre Brown Bread", price: 55, category: "Bread", unit: "pack", image: "bonnhighfobrebrownbread.avif" },
  { name: "Bonn Zero Maida Atta Bread", price: 50, category: "Bread", unit: "pack", image: "bonnzeromaida100percentatta.avif" },
  { name: "Britannia Brown Bread", price: 45, category: "Bread", unit: "pack", image: "britanniabrownbread.avif" },
  { name: "English Oven Fruit Bread", price: 65, category: "Bread", unit: "pack", image: "englishovenfruitbread.avif" },
  { name: "English Oven Premium White Bread", price: 55, category: "Bread", unit: "pack", image: "englishovenpremiumwhite.avif" },
  { name: "English Oven Sandwich Bread 400g", price: 50, category: "Bread", unit: "pack", image: "englishovensandwhichwhitebread400gs.avif" },
  { name: "English Oven Milk Bread 400g", price: 55, category: "Bread", unit: "pack", image: "englishovermilkbreak400gs.avif" },
  { name: "Harvest Gold Bombay Pao", price: 25, category: "Bread", unit: "pack", image: "harvestgoldbombaypao.avif" },
  { name: "Harvest Gold Kulcha", price: 30, category: "Bread", unit: "pack", image: "harvestgoldkulcha.avif" },
  { name: "Harvest Gold Multigrain Bread 450g", price: 55, category: "Bread", unit: "pack", image: "harvestgoldmultigrainbread450gs.avif" },

  // Atta & Rice
  { name: "Aashirvaad Atta 5kg", price: 270, category: "Atta & Rice", unit: "bag" },
  { name: "Fortune Chakki Atta 5kg", price: 255, category: "Atta & Rice", unit: "bag" },
  { name: "India Gate Basmati Rice 1kg", price: 160, category: "Atta & Rice", unit: "bag" },
  { name: "Daawat Rozana Rice 5kg", price: 350, category: "Atta & Rice", unit: "bag" },

  // Dal & Pulses
  { name: "Toor Dal 1kg", price: 145, category: "Dal & Pulses", unit: "bag" },
  { name: "Moong Dal 1kg", price: 130, category: "Dal & Pulses", unit: "bag" },
  { name: "Chana Dal 1kg", price: 95, category: "Dal & Pulses", unit: "bag" },
  { name: "Rajma 500g", price: 85, category: "Dal & Pulses", unit: "bag" },

  // Oil
  { name: "Fortune Sunflower Oil 1L", price: 140, category: "Oil & Ghee", unit: "bottle" },
  { name: "Saffola Gold Oil 1L", price: 185, category: "Oil & Ghee", unit: "bottle" },
  { name: "Amul Ghee 500ml", price: 290, category: "Oil & Ghee", unit: "jar" },

  // Spices & Masala
  { name: "MDH Chana Masala 100g", price: 65, category: "Spices", unit: "pack" },
  { name: "Everest Garam Masala 100g", price: 72, category: "Spices", unit: "pack" },
  { name: "Haldi Powder 100g", price: 30, category: "Spices", unit: "pack" },
  { name: "Red Chilli Powder 100g", price: 35, category: "Spices", unit: "pack" },
  { name: "Salt 1kg", price: 22, category: "Spices", unit: "bag" },
  { name: "Sugar 1kg", price: 45, category: "Spices", unit: "bag" },

  // Snacks
  { name: "Lays Classic Salted 52g", price: 20, category: "Snacks", unit: "pack" },
  { name: "Kurkure Masala Munch 75g", price: 20, category: "Snacks", unit: "pack" },
  { name: "Parle-G Biscuits 250g", price: 25, category: "Snacks", unit: "pack" },
  { name: "Britannia Good Day 75g", price: 30, category: "Snacks", unit: "pack" },
  { name: "Maggi Noodles 4-pack", price: 56, category: "Snacks", unit: "pack" },

  // Beverages
  { name: "Coca Cola 750ml", price: 38, category: "Beverages", unit: "bottle" },
  { name: "Pepsi 750ml", price: 38, category: "Beverages", unit: "bottle" },
  { name: "Tata Tea Gold 250g", price: 120, category: "Beverages", unit: "pack" },
  { name: "Nescafe Classic 50g", price: 155, category: "Beverages", unit: "jar" },
  { name: "Bisleri Water 1L", price: 20, category: "Beverages", unit: "bottle" },

  // Household
  { name: "Surf Excel 1kg", price: 125, category: "Household", unit: "pack" },
  { name: "Vim Bar 200g", price: 18, category: "Household", unit: "bar" },
  { name: "Lizol Floor Cleaner 500ml", price: 99, category: "Household", unit: "bottle" },
];

export function seedProducts() {
  const existing = db.prepare("SELECT COUNT(*) as count FROM products").get() as { count: number };
  if (existing.count > 0) return;

  const insert = db.prepare(
    "INSERT INTO products (name, price, category, unit, image) VALUES (?, ?, ?, ?, ?)"
  );

  const insertMany = db.transaction(() => {
    for (const p of products) {
      insert.run(p.name, p.price, p.category, p.unit, p.image || null);
    }
  });

  insertMany();
}

// Seed some dummy users
export function seedUsers() {
  const existing = db.prepare("SELECT COUNT(*) as count FROM users").get() as { count: number };
  if (existing.count > 0) return;

  const insert = db.prepare(
    "INSERT INTO users (name, flat_no, phone) VALUES (?, ?, ?)"
  );

  const users = [
    { name: "Rahul Sharma", flat_no: "A-101", phone: "9876543210" },
    { name: "Priya Gupta", flat_no: "B-204", phone: "9876543211" },
    { name: "Amit Patel", flat_no: "C-302", phone: "9876543212" },
    { name: "Neha Singh", flat_no: "A-405", phone: "9876543213" },
  ];

  const insertMany = db.transaction(() => {
    for (const u of users) {
      insert.run(u.name, u.flat_no, u.phone);
    }
  });

  insertMany();
}
