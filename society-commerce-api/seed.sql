-- Seed data for end-to-end buyer flow

-- 1. Society
INSERT INTO "Societies" ("Id", "Name", "Address", "City", "PinCode", "HouseholdCap", "Settings", "CreatedAt")
VALUES ('a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'Green Valley Society', 'Sector 45, Gurugram', 'Gurugram', '122003', 4, '{"allowCod": true, "maxOrderValue": 500000}', NOW())
ON CONFLICT DO NOTHING;

-- 2. Flat
INSERT INTO "Flats" ("Id", "SocietyId", "FlatNumber", "Block", "Floor", "IsActive", "CreatedAt")
VALUES ('f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'A-101', 'A', 1, true, NOW())
ON CONFLICT DO NOTHING;

-- 3. Link buyer user to flat (HouseholdMembership)
INSERT INTO "HouseholdMemberships" ("Id", "UserId", "FlatId", "Role", "InvitedBy", "JoinedAt")
VALUES ('11111111-1111-4111-8111-111111111111', '7447e5c4-5994-42a7-af18-0b63e836a655', 'f1a2b3c4-d5e6-4f7a-8b9c-0d1e2f3a4b5c', 0, NULL, NOW())
ON CONFLICT DO NOTHING;

-- 4. Buyer role for test user (RoleType 1 = FlatOwner)
INSERT INTO "UserRoles" ("Id", "UserId", "RoleType", "ScopeId", "GrantedBy", "GrantedAt")
VALUES ('22222222-2222-4222-8222-222222222222', '7447e5c4-5994-42a7-af18-0b63e836a655', 1, NULL, NULL, NOW())
ON CONFLICT DO NOTHING;

-- 5. Seller user (shop owner)
INSERT INTO "Users" ("Id", "Phone", "Name", "PreferredLanguage", "IsActive", "ApprovalStatus", "CreatedAt")
VALUES ('b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', '9876543210', 'Rajesh Kumar', 'en', true, 1, NOW())
ON CONFLICT ("Phone") DO NOTHING;

-- 6. Seller role (RoleType 3 = SellerOwner)
INSERT INTO "UserRoles" ("Id", "UserId", "RoleType", "ScopeId", "GrantedBy", "GrantedAt")
VALUES ('33333333-3333-4333-8333-333333333333', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 3, 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', NULL, NOW())
ON CONFLICT DO NOTHING;

-- 6b. Admin user
INSERT INTO "Users" ("Id", "Phone", "Name", "PreferredLanguage", "IsActive", "ApprovalStatus", "CreatedAt")
VALUES ('a0000001-0000-4000-8000-000000000001', '9560018536', 'Admin User', 'en', true, 1, NOW())
ON CONFLICT ("Phone") DO NOTHING;

-- 6c. Admin role (RoleType 6 = Admin)
INSERT INTO "UserRoles" ("Id", "UserId", "RoleType", "ScopeId", "GrantedBy", "GrantedAt")
VALUES ('44444444-4444-4444-8444-444444444444', 'a0000001-0000-4000-8000-000000000001', 6, NULL, NULL, NOW())
ON CONFLICT DO NOTHING;

-- 7. Shop (ApprovalStatus 1 = Approved)
INSERT INTO "Shops" ("Id", "SocietyId", "OwnerId", "Name", "Category", "Description", "IsActive", "ApprovalStatus", "ApprovedAt", "CreatedAt")
VALUES ('c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d', 'b2c3d4e5-f6a7-4b8c-9d0e-1f2a3b4c5d6e', 'Lucky Store', 'Grocery', 'Your neighbourhood grocery and dairy store', true, 1, NOW(), NOW())
ON CONFLICT DO NOTHING;

-- 8. Products (InventoryType: 1=Finite, 2=Abundant)
INSERT INTO "Products" ("Id", "ShopId", "Name", "Category", "PricePaise", "InventoryType", "StockQuantity", "IsAvailable", "IsRegulated", "LowStockThreshold", "CreatedAt", "UpdatedAt") VALUES
('d0000001-0000-4000-8000-000000000001', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Amul Taaza Milk 500ml', 'Dairy', 2900, 2, NULL, true, false, 0, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000002', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Amul Gold Milk 500ml', 'Dairy', 3400, 2, NULL, true, false, 0, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000003', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Mother Dairy Full Cream 500ml', 'Dairy', 3200, 2, NULL, true, false, 0, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000004', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Amul Salted Butter 100g', 'Dairy', 5600, 1, 15, true, false, 3, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000005', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Amul Masti Dahi 400g', 'Dairy', 3500, 1, 10, true, false, 2, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000006', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Harvest Gold White Bread 350g', 'Bread', 4000, 1, 8, true, false, 2, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000007', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'English Oven Atta Bread 400g', 'Bread', 5000, 1, 6, true, false, 2, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000008', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Pav (4 pack)', 'Bread', 2000, 2, NULL, true, false, 0, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000009', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Eggs (6 pack)', 'Eggs', 4200, 1, 20, true, false, 5, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000010', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Eggs (12 pack)', 'Eggs', 7800, 1, 12, true, false, 3, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000011', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Aashirvaad Atta 5kg', 'Atta & Rice', 27000, 1, 5, true, false, 2, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000012', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'India Gate Basmati Rice 1kg', 'Atta & Rice', 16000, 1, 8, true, false, 2, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000013', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Toor Dal 1kg', 'Dal & Pulses', 14500, 1, 10, true, false, 3, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000014', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Maggi Noodles 4-pack', 'Snacks', 5600, 1, 25, true, false, 5, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000015', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Lays Classic Salted 52g', 'Snacks', 2000, 1, 30, true, false, 5, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000016', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Coca Cola 750ml', 'Beverages', 3800, 1, 15, true, false, 3, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000017', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Tata Tea Gold 250g', 'Beverages', 12000, 1, 10, true, false, 2, NOW(), NOW()),
('d0000001-0000-4000-8000-000000000018', 'c3d4e5f6-a7b8-4c9d-0e1f-2a3b4c5d6e7f', 'Surf Excel 1kg', 'Household', 12500, 1, 8, true, false, 2, NOW(), NOW())
ON CONFLICT DO NOTHING;
