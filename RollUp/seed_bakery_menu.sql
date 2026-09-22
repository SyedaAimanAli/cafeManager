-- ==============================================================================
-- RollUp Bakery & Roastery - Database Seed Script for PostgreSQL
-- Seeds rich bakery categories, artisan menu items with photos, and branding.
-- ==============================================================================

DO $$
DECLARE
    v_tenant_id integer;
    v_outlet_id integer;
    v_cat_breads integer;
    v_cat_pastries integer;
    v_cat_cakes integer;
    v_cat_savory integer;
    v_cat_cookies integer;
    v_cat_coffee integer;
BEGIN
    -- 1. Ensure Tenant Exists and update Branding
    SELECT "Id" INTO v_tenant_id FROM "Tenants" WHERE "Slug" = 'default' LIMIT 1;
    
    IF v_tenant_id IS NULL THEN
        INSERT INTO "Tenants" (
            "Name", "Slug", "Country", "City", "Currency", 
            "ContactEmail", "ContactPhone", "Address", "Tagline", 
            "IsActive", "ThemeTemplate", "ColorScheme", "FontFamily", 
            "CreatedAt", "IsDeleted"
        ) VALUES (
            'RollUp Artisan Bakery & Roastery', 'default', 'United States', 'San Francisco', 'USD',
            'bakery@rollup.com', '+1 (415) 890-2144', '482 Baker & Artisan Way, San Francisco, CA', 
            'Wild Sourdough, French Viennoiserie & Small-Batch Roastery',
            true, 'bistro', 'espresso', 'playfair',
            NOW(), false
        ) RETURNING "Id" INTO v_tenant_id;
    ELSE
        UPDATE "Tenants" SET
            "Name" = 'RollUp Artisan Bakery & Roastery',
            "Tagline" = 'Wild Sourdough, French Viennoiserie & Small-Batch Roastery',
            "ThemeTemplate" = 'bistro',
            "ColorScheme" = 'espresso',
            "FontFamily" = 'playfair',
            "Address" = '482 Baker & Artisan Way, San Francisco, CA',
            "ContactPhone" = '+1 (415) 890-2144',
            "ContactEmail" = 'bakery@rollup.com',
            "UpdatedAt" = NOW()
        WHERE "Id" = v_tenant_id;
    END IF;

    -- 2. Ensure Outlet Exists
    SELECT "Id" INTO v_outlet_id FROM "Outlets" WHERE "TenantId" = v_tenant_id LIMIT 1;
    IF v_outlet_id IS NULL THEN
        INSERT INTO "Outlets" (
            "Name", "Address", "Phone", "IsActive", "TenantId", "CreatedAt", "IsDeleted"
        ) VALUES (
            'Flagship Bakery & Cafe', '482 Baker & Artisan Way, San Francisco, CA', '+1 (415) 890-2144', true, v_tenant_id, NOW(), false
        ) RETURNING "Id" INTO v_outlet_id;
    END IF;

    -- 3. Clean existing Menu Items & Categories for clean bakery demo
    DELETE FROM "OrderItems";
    DELETE FROM "MenuItems" WHERE "TenantId" = v_tenant_id;
    DELETE FROM "Categories" WHERE "TenantId" = v_tenant_id;

    -- 4. Insert Bakery Categories
    INSERT INTO "Categories" ("Name", "Description", "SortOrder", "TenantId", "CreatedAt", "IsDeleted")
    VALUES ('Artisan Breads & Sourdough', 'Naturally leavened wild sourdough, baguettes and daily hearth bakes', 1, v_tenant_id, NOW(), false)
    RETURNING "Id" INTO v_cat_breads;

    INSERT INTO "Categories" ("Name", "Description", "SortOrder", "TenantId", "CreatedAt", "IsDeleted")
    VALUES ('French Viennoiserie & Pastries', '72-layer French butter croissants, danishes and kouign-amanns', 2, v_tenant_id, NOW(), false)
    RETURNING "Id" INTO v_cat_pastries;

    INSERT INTO "Categories" ("Name", "Description", "SortOrder", "TenantId", "CreatedAt", "IsDeleted")
    VALUES ('Cakes, Tarts & Patisserie', 'Individual French tarts, Basque cheesecakes, eclairs and delicate mille-feuilles', 3, v_tenant_id, NOW(), false)
    RETURNING "Id" INTO v_cat_cakes;

    INSERT INTO "Categories" ("Name", "Description", "SortOrder", "TenantId", "CreatedAt", "IsDeleted")
    VALUES ('Artisan Sandwiches & Savory', 'Warm gourmet quiches, tartines, burrata baguettes and brioche melts', 4, v_tenant_id, NOW(), false)
    RETURNING "Id" INTO v_cat_savory;

    INSERT INTO "Categories" ("Name", "Description", "SortOrder", "TenantId", "CreatedAt", "IsDeleted")
    VALUES ('Cookies, Scones & Tea Treats', 'Sea salt chocolate chunk cookies, Earl Grey scones and pistachio bakes', 5, v_tenant_id, NOW(), false)
    RETURNING "Id" INTO v_cat_cookies;

    INSERT INTO "Categories" ("Name", "Description", "SortOrder", "TenantId", "CreatedAt", "IsDeleted")
    VALUES ('Specialty Coffee & Beverages', 'Single-origin espresso, velvet flat whites, and ceremonial iced matcha', 6, v_tenant_id, NOW(), false)
    RETURNING "Id" INTO v_cat_coffee;

    -- 5. Insert Bakery Menu Items with Photography URLs
    
    -- Category 1: Artisan Breads
    INSERT INTO "MenuItems" ("Name", "Description", "Price", "ImageUrl", "IsAvailable", "IsPopular", "Tags", "TenantId", "CategoryId", "OutletId", "CreatedAt", "IsDeleted")
    VALUES 
    (
        'Rustic Country Sourdough Batard',
        'Naturally leavened with wild yeast starter, 36-hour cold fermented for blistered dark crust and airy, custard crumb.',
        7.50,
        'https://images.unsplash.com/photo-1589367920969-ab8e050bbb04?w=600&auto=format&fit=crop&q=80',
        true, true, 'Vegan,Sourdough,Organic', v_tenant_id, v_cat_breads, v_outlet_id, NOW(), false
    ),
    (
        'Seeded Multigrain Sourdough',
        'Packed with toasted sesame, golden flax, toasted pumpkin and sunflower seeds for deep nutty aroma and crunch.',
        8.25,
        'https://images.unsplash.com/photo-1509440159596-0249088772ff?w=600&auto=format&fit=crop&q=80',
        true, false, 'High Fiber,Sourdough,Seeds', v_tenant_id, v_cat_breads, v_outlet_id, NOW(), false
    ),
    (
        'Traditional French Baguette',
        'Classic crispy golden crust, open honeycomb crumb, baked fresh four times daily from unbleached French flour.',
        4.50,
        'https://images.unsplash.com/photo-1549931319-a545dcf3bc73?w=600&auto=format&fit=crop&q=80',
        true, true, 'French,Crispy,Daily Bake', v_tenant_id, v_cat_breads, v_outlet_id, NOW(), false
    ),
    (
        'Roasted Garlic & Rosemary Focaccia',
        'Cold-pressed olive oil-rich Italian dough dimpled with sweet garlic confit, garden rosemary, and Maldon flaky sea salt.',
        6.00,
        'https://images.unsplash.com/photo-1616428678947-f370d04961be?w=600&auto=format&fit=crop&q=80',
        true, false, 'Italian,Vegan,Garlic Confit', v_tenant_id, v_cat_breads, v_outlet_id, NOW(), false
    );

    -- Category 2: French Viennoiserie
    INSERT INTO "MenuItems" ("Name", "Description", "Price", "ImageUrl", "IsAvailable", "IsPopular", "Tags", "TenantId", "CategoryId", "OutletId", "CreatedAt", "IsDeleted")
    VALUES 
    (
        'Classic Butter Croissant',
        '72-layer laminated French butter pastry, honeycomb interior, shatteringly crisp exterior, and meltingly rich.',
        4.25,
        'https://images.unsplash.com/photo-1555507036-ab1f4038808a?w=600&auto=format&fit=crop&q=80',
        true, true, 'French Butter,Bestseller', v_tenant_id, v_cat_pastries, v_outlet_id, NOW(), false
    ),
    (
        'Pain au Chocolat',
        'Double batons of Valrhona 64% dark chocolate enveloped in buttery flaky croissant dough.',
        4.95,
        'https://images.unsplash.com/photo-1608198093002-ad4e005484ec?w=600&auto=format&fit=crop&q=80',
        true, true, 'Valrhona Chocolate,Popular', v_tenant_id, v_cat_pastries, v_outlet_id, NOW(), false
    ),
    (
        'Almond Frangipane Croissant',
        'Twice-baked croissant infused with vanilla rum syrup, filled with rich almond cream and topped with toasted sliced almonds.',
        5.50,
        'https://images.unsplash.com/photo-1623334044303-241021148842?w=600&auto=format&fit=crop&q=80',
        true, true, 'Contains Nuts,Bestseller', v_tenant_id, v_cat_pastries, v_outlet_id, NOW(), false
    ),
    (
        'Cardamom Kouign-Amann',
        'Breton laminated pastry caramelized with salted butter and turbinado sugar, spiced with fragrant green cardamom.',
        5.25,
        'https://images.unsplash.com/photo-1530610476181-d83430b64dcd?w=600&auto=format&fit=crop&q=80',
        true, false, 'Spiced,Caramelized,Signature', v_tenant_id, v_cat_pastries, v_outlet_id, NOW(), false
    );

    -- Category 3: Cakes, Tarts & Patisserie
    INSERT INTO "MenuItems" ("Name", "Description", "Price", "ImageUrl", "IsAvailable", "IsPopular", "Tags", "TenantId", "CategoryId", "OutletId", "CreatedAt", "IsDeleted")
    VALUES 
    (
        'Wild Berry Frangipane Tart',
        'Crisp sweet shortcrust filled with baked almond frangipane, topped with fresh organic raspberries, blackberries & glaze.',
        6.75,
        'https://images.unsplash.com/photo-1519869325930-281384150729?w=600&auto=format&fit=crop&q=80',
        true, true, 'Fresh Fruit,Vegetarian,Patisserie', v_tenant_id, v_cat_cakes, v_outlet_id, NOW(), false
    ),
    (
        'Belgian Dark Chocolate Eclair',
        'Crisp choux pastry stuffed with silky 70% dark chocolate pastry cream, dipped in mirror gloss chocolate glaze.',
        5.95,
        'https://images.unsplash.com/photo-1587314168485-3236d6710814?w=600&auto=format&fit=crop&q=80',
        true, false, 'Belgian Chocolate,Choux', v_tenant_id, v_cat_cakes, v_outlet_id, NOW(), false
    ),
    (
        'Lemon Meringue Sable Tart',
        'Zesty Meyer lemon curd with toasted Swiss meringue peaks in crisp French butter sable shell.',
        6.25,
        'https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=600&auto=format&fit=crop&q=80',
        true, true, 'Citrus,Tart,Signature', v_tenant_id, v_cat_cakes, v_outlet_id, NOW(), false
    ),
    (
        'Basque Burnt Cheesecake Slice',
        'Crustless Spanish cheesecake baked at high heat for deeply caramelized top with molten, custardy center.',
        6.50,
        'https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=600&auto=format&fit=crop&q=80',
        true, true, 'Gluten-Free,Bestseller', v_tenant_id, v_cat_cakes, v_outlet_id, NOW(), false
    );

    -- Category 4: Artisan Sandwiches & Savory
    INSERT INTO "MenuItems" ("Name", "Description", "Price", "ImageUrl", "IsAvailable", "IsPopular", "Tags", "TenantId", "CategoryId", "OutletId", "CreatedAt", "IsDeleted")
    VALUES 
    (
        'Prosciutto & Whipped Burrata Baguette',
        '24-month aged Prosciutto di Parma, creamy Italian burrata, baby arugula, and wild fig glaze on crusty artisan baguette.',
        11.50,
        'https://images.unsplash.com/photo-1528735602780-2552fd46c7af?w=600&auto=format&fit=crop&q=80',
        true, true, 'Savory,Gourmet,Popular', v_tenant_id, v_cat_savory, v_outlet_id, NOW(), false
    ),
    (
        'Smoked Salmon & Dill Cream Cheese Brioche',
        'Norwegian smoked salmon, whipped caper cream cheese, pickled shallots, and fresh dill on toasted brioche bun.',
        12.00,
        'https://images.unsplash.com/photo-1550547660-d9450f859349?w=600&auto=format&fit=crop&q=80',
        true, false, 'Seafood,Chef Special', v_tenant_id, v_cat_savory, v_outlet_id, NOW(), false
    ),
    (
        'Heirloom Tomato & Ricotta Tartine',
        'Thick toasted country sourdough, whipped lemon ricotta, colorful heirloom tomatoes, basil walnut pesto, and balsamic glaze.',
        9.50,
        'https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop&q=80',
        true, false, 'Vegetarian,Organic,Tartine', v_tenant_id, v_cat_savory, v_outlet_id, NOW(), false
    ),
    (
        'Caramelized Onion & Gruyère Quiche',
        'Rich baked egg custard with slow-cooked sweet shallots and cave-aged Swiss Gruyère in flaky butter crust.',
        8.50,
        'https://images.unsplash.com/photo-1627308595229-7830a5c91f9f?w=600&auto=format&fit=crop&q=80',
        true, false, 'Warm Bake,French,Vegetarian', v_tenant_id, v_cat_savory, v_outlet_id, NOW(), false
    );

    -- Category 5: Cookies & Scones
    INSERT INTO "MenuItems" ("Name", "Description", "Price", "ImageUrl", "IsAvailable", "IsPopular", "Tags", "TenantId", "CategoryId", "OutletId", "CreatedAt", "IsDeleted")
    VALUES 
    (
        'Valrhona Sea Salt Chocolate Chunk Cookie',
        'Crisp edges with gooey molten center, loaded with 70% dark chocolate pools and topped with crunchy Maldon sea salt.',
        3.75,
        'https://images.unsplash.com/photo-1499636136210-6f4ee915583e?w=600&auto=format&fit=crop&q=80',
        true, true, 'Bestseller,Valrhona Chocolate', v_tenant_id, v_cat_cookies, v_outlet_id, NOW(), false
    ),
    (
        'Earl Grey & Wild Blueberry Scone',
        'Tender buttermilk scone infused with bergamot Earl Grey tea, studded with wild Maine blueberries, finished with vanilla glaze.',
        4.50,
        'https://images.unsplash.com/photo-1586985289688-ca3cf47d3e6e?w=600&auto=format&fit=crop&q=80',
        true, false, 'Afternoon Tea,Blueberry', v_tenant_id, v_cat_cookies, v_outlet_id, NOW(), false
    ),
    (
        'Cinnamon Brown Sugar Brioche Roll',
        'Soft fluffy brioche spiraled with Ceylon cinnamon and brown sugar, smothered in whipped Madagascar vanilla bean cream cheese.',
        5.00,
        'https://images.unsplash.com/photo-1509365465985-25d11c17e812?w=600&auto=format&fit=crop&q=80',
        true, true, 'Comfort Food,Popular,Warm', v_tenant_id, v_cat_cookies, v_outlet_id, NOW(), false
    );

    -- Category 6: Specialty Coffee & Beverages
    INSERT INTO "MenuItems" ("Name", "Description", "Price", "ImageUrl", "IsAvailable", "IsPopular", "Tags", "TenantId", "CategoryId", "OutletId", "CreatedAt", "IsDeleted")
    VALUES 
    (
        'Velvet Flat White',
        'Double shot of Ethiopian single-origin espresso with silky steamed microfoam milk.',
        4.75,
        'https://images.unsplash.com/photo-1577968897966-3d4325b36b61?w=600&auto=format&fit=crop&q=80',
        true, true, 'Espresso,Barista Pick', v_tenant_id, v_cat_coffee, v_outlet_id, NOW(), false
    ),
    (
        'Ceremonial Iced Matcha Latte',
        'Stone-ground Uji ceremonial grade matcha whisked with oat milk and touch of organic wildflower honey.',
        5.50,
        'https://images.unsplash.com/photo-1536256263959-770b48d82b0a?w=600&auto=format&fit=crop&q=80',
        true, true, 'Matcha,Organic,Antioxidant', v_tenant_id, v_cat_coffee, v_outlet_id, NOW(), false
    ),
    (
        'Vanilla Bean Cardamom Latte',
        'House-infused Madagascar vanilla syrup, ground cardamom spice, double espresso, and textured whole milk.',
        5.75,
        'https://images.unsplash.com/photo-1541167760496-1628856ab772?w=600&auto=format&fit=crop&q=80',
        true, false, 'Spiced,Signature,Latte', v_tenant_id, v_cat_coffee, v_outlet_id, NOW(), false
    );

    RAISE NOTICE 'Bakery Menu Seed Completed Successfully! Tenant ID: %, Items Added: 22', v_tenant_id;
END $$;
