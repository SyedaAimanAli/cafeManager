-- ==============================================================================
-- RollUp Sales & Orders Simulation Seed Script for PostgreSQL
-- Seeds realistic historical orders over the past 7 days for rich analytics
-- ==============================================================================

DO $$
DECLARE
    v_tenant_id integer := 1;
    v_outlet_id integer := 1;
    v_order_id integer;
    v_day integer;
    v_order_num integer := 1001;
    v_customers text[] := ARRAY['Emma Watson', 'Liam Miller', 'Sophia Chen', 'Noah Davies', 'Olivia Taylor', 'Lucas Brown', 'Ava Martinez', 'Ethan Wright', 'Mia Anderson', 'Alexander Hall', 'Charlotte White', 'James Wilson', 'Isabella King', 'Benjamin Scott', 'Amelia Harris'];
    v_types text[] := ARRAY['DineIn', 'DineIn', 'DineIn', 'Takeaway', 'Takeaway'];
    v_tables text[] := ARRAY['Table 1', 'Table 2', 'Table 3', 'Table 4', 'Table 5', 'Patio-A', 'Patio-B', 'Counter 2'];
    v_cust_name text;
    v_type text;
    v_table text;
    v_created_at timestamptz;
    v_hour integer;
    v_minute integer;
    v_orders_per_day integer;
    v_i integer;
BEGIN
    -- Loop across the past 7 days (day 6 down to 0 = today)
    FOR v_day IN REVERSE 6..0 LOOP
        -- Generate between 15 to 25 orders per day
        v_orders_per_day := 16 + (v_day * 2) % 9;

        FOR v_i IN 1..v_orders_per_day LOOP
            -- Peak hours weighting (Morning 8-10, Lunch 12-14, Afternoon 15-18)
            IF v_i % 3 = 0 THEN
                v_hour := 8 + (v_i % 3);   -- 8, 9, 10 AM
            ELSIF v_i % 3 = 1 THEN
                v_hour := 12 + (v_i % 3);  -- 12, 13, 14 PM
            ELSE
                v_hour := 15 + (v_i % 4);  -- 15, 16, 17, 18 PM
            END IF;

            v_minute := (v_i * 7) % 60;
            v_created_at := (CURRENT_DATE - (v_day || ' days')::interval) + (v_hour || ' hours')::interval + (v_minute || ' minutes')::interval;

            v_cust_name := v_customers[1 + (v_order_num % array_length(v_customers, 1))];
            v_type := v_types[1 + (v_order_num % array_length(v_types, 1))];
            v_table := CASE WHEN v_type = 'DineIn' THEN v_tables[1 + (v_order_num % array_length(v_tables, 1))] ELSE '' END;

            -- Create Order
            INSERT INTO "Orders" (
                "OrderNumber", "CustomerName", "TableNumber", "Status", "Type", 
                "TenantId", "OutletId", "CreatedAt", "CompletedAt", "IsDeleted"
            ) VALUES (
                '#' || v_order_num, v_cust_name, v_table, 'Completed', v_type,
                v_tenant_id, v_outlet_id, v_created_at, v_created_at + interval '12 minutes', false
            ) RETURNING "Id" INTO v_order_id;

            -- Add 1 to 3 items per order
            IF v_order_num % 4 = 0 THEN
                -- Sourdough Batard + Butter Croissant + Flat White
                INSERT INTO "OrderItems" ("Quantity", "UnitPrice", "SelectedVariant", "SelectedAddons", "SpecialInstructions", "OrderId", "MenuItemId", "CreatedAt", "IsDeleted")
                VALUES 
                (1, 7.50, 'Standard', '', '', v_order_id, 1, v_created_at, false),
                (2, 4.25, 'Warm', '', '', v_order_id, 5, v_created_at, false),
                (1, 4.75, 'Oat Milk', '', '', v_order_id, 20, v_created_at, false);
            ELSIF v_order_num % 4 = 1 THEN
                -- Burrata Baguette + Iced Matcha Latte
                INSERT INTO "OrderItems" ("Quantity", "UnitPrice", "SelectedVariant", "SelectedAddons", "SpecialInstructions", "OrderId", "MenuItemId", "CreatedAt", "IsDeleted")
                VALUES 
                (1, 11.50, 'Standard', '', '', v_order_id, 13, v_created_at, false),
                (1, 5.50, 'Standard', '', '', v_order_id, 21, v_created_at, false);
            ELSIF v_order_num % 4 = 2 THEN
                -- Pain au Chocolat + Almond Croissant + Cardamom Latte
                INSERT INTO "OrderItems" ("Quantity", "UnitPrice", "SelectedVariant", "SelectedAddons", "SpecialInstructions", "OrderId", "MenuItemId", "CreatedAt", "IsDeleted")
                VALUES 
                (2, 4.95, 'Standard', '', '', v_order_id, 6, v_created_at, false),
                (1, 5.50, 'Standard', '', '', v_order_id, 7, v_created_at, false),
                (2, 5.75, 'Whole Milk', '', '', v_order_id, 22, v_created_at, false);
            ELSE
                -- Basque Cheesecake + Sea Salt Chocolate Cookie + Flat White
                INSERT INTO "OrderItems" ("Quantity", "UnitPrice", "SelectedVariant", "SelectedAddons", "SpecialInstructions", "OrderId", "MenuItemId", "CreatedAt", "IsDeleted")
                VALUES 
                (1, 6.50, 'Standard', '', '', v_order_id, 12, v_created_at, false),
                (3, 3.75, 'Standard', '', '', v_order_id, 17, v_created_at, false),
                (1, 4.75, 'Standard', '', '', v_order_id, 20, v_created_at, false);
            END IF;

            v_order_num := v_order_num + 1;
        END LOOP;
    END LOOP;

    RAISE NOTICE 'Seeded % historical orders for reports successfully!', (v_order_num - 1001);
END $$;
