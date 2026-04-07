--------------------------------------------------------
--  File created - Monday-April-06-2026
--  Purpose: Oracle transaction procedures for order lifecycle
--  Schema: CARDGAME
--------------------------------------------------------

--------------------------------------------------------
-- Optional: align payment status constraint with backend
-- Backend uses: PENDING | SUCCESS | FAILED | REFUNDED
--------------------------------------------------------
BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE "CARDGAME"."PAYMENTS" DROP CONSTRAINT "CHK_PAYMENTS_STATUS"';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -2443 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'ALTER TABLE "CARDGAME"."PAYMENTS" DROP CONSTRAINT "CHK_PAYMENT_STATUS"';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -2443 THEN
            RAISE;
        END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE '
        ALTER TABLE "CARDGAME"."PAYMENTS"
        ADD CONSTRAINT "CHK_PAYMENTS_STATUS"
        CHECK ("STATUS" IN (''PENDING'', ''SUCCESS'', ''FAILED'', ''REFUNDED''))
    ';
EXCEPTION
    WHEN OTHERS THEN
        IF SQLCODE != -2264 THEN
            RAISE;
        END IF;
END;
/

--------------------------------------------------------
-- Optional: align legacy triggers/procedure in old dump
-- Old dump uses payment status PAID/CANCELLED, while backend uses SUCCESS/FAILED/REFUNDED
--------------------------------------------------------
CREATE OR REPLACE TRIGGER "CARDGAME"."TRG_PAYMENTS_SET_PAID_AT"
BEFORE INSERT OR UPDATE OF "STATUS" ON "CARDGAME"."PAYMENTS"
FOR EACH ROW
BEGIN
    IF :NEW."STATUS" = 'SUCCESS' AND :NEW."PAID_AT" IS NULL THEN
        :NEW."PAID_AT" := SYSDATE;
    END IF;
END;
/

CREATE OR REPLACE TRIGGER "CARDGAME"."TRG_PAYMENTS_CONFIRM_ORDER"
AFTER INSERT OR UPDATE OF "STATUS" ON "CARDGAME"."PAYMENTS"
FOR EACH ROW
BEGIN
    IF :NEW."STATUS" = 'SUCCESS' THEN
        UPDATE "CARDGAME"."ORDERS"
           SET "ORDER_STATUS" = 'CONFIRMED'
         WHERE "ID" = :NEW."ORDER_ID"
           AND "ORDER_STATUS" = 'PENDING';
    ELSIF :NEW."STATUS" IN ('FAILED', 'REFUNDED') THEN
        UPDATE "CARDGAME"."ORDERS"
           SET "ORDER_STATUS" = 'CANCELLED'
         WHERE "ID" = :NEW."ORDER_ID"
           AND "ORDER_STATUS" = 'PENDING';
    END IF;
END;
/

CREATE OR REPLACE PROCEDURE "CARDGAME"."SP_UPDATE_ORDER_STATUS"
(
    p_order_id      IN  NUMBER,
    p_new_status    IN  VARCHAR2,
    p_success       OUT NUMBER,
    p_error_message OUT VARCHAR2
)
AS
    v_current_status VARCHAR2(20);
BEGIN
    p_success := 0;
    p_error_message := NULL;

    BEGIN
        SELECT "ORDER_STATUS" INTO v_current_status
        FROM "CARDGAME"."ORDERS"
        WHERE "ID" = p_order_id
        FOR UPDATE;
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            p_error_message := 'Order not found';
            RETURN;
    END;

    IF v_current_status IN ('DONE', 'CANCELLED') THEN
        p_error_message := 'Cannot update completed/cancelled order';
        RETURN;
    END IF;

    IF UPPER(p_new_status) = 'CANCELLED' THEN
        FOR detail IN (
            SELECT "PRODUCT_ID", "QUANTITY"
            FROM "CARDGAME"."ORDER_DETAILS"
            WHERE "ORDER_ID" = p_order_id
        ) LOOP
            UPDATE "CARDGAME"."INVENTORY"
            SET "RESERVED_QUANTITY" = GREATEST(0, "RESERVED_QUANTITY" - detail."QUANTITY")
            WHERE "PRODUCT_ID" = detail."PRODUCT_ID";
        END LOOP;
    END IF;

    IF UPPER(p_new_status) = 'DONE' THEN
        FOR detail IN (
            SELECT "PRODUCT_ID", "QUANTITY"
            FROM "CARDGAME"."ORDER_DETAILS"
            WHERE "ORDER_ID" = p_order_id
        ) LOOP
            UPDATE "CARDGAME"."INVENTORY"
            SET "QUANTITY" = GREATEST(0, "QUANTITY" - detail."QUANTITY"),
                "RESERVED_QUANTITY" = GREATEST(0, "RESERVED_QUANTITY" - detail."QUANTITY")
            WHERE "PRODUCT_ID" = detail."PRODUCT_ID";
        END LOOP;

        UPDATE "CARDGAME"."PAYMENTS"
        SET "STATUS" = 'SUCCESS', "PAID_AT" = NVL("PAID_AT", SYSDATE)
        WHERE "ORDER_ID" = p_order_id;
    END IF;

    UPDATE "CARDGAME"."ORDERS"
    SET "ORDER_STATUS" = UPPER(p_new_status)
    WHERE "ID" = p_order_id;

    COMMIT;
    p_success := 1;
EXCEPTION
    WHEN OTHERS THEN
        ROLLBACK;
        p_error_message := SQLERRM;
END;
/

--------------------------------------------------------
-- Procedure 1: Place order (single product line)
-- - Locks product + inventory row
-- - Validates active product and available stock
-- - Reserves stock (RESERVED_QUANTITY)
-- - Creates ORDERS, ORDER_DETAILS, PAYMENTS atomically
--------------------------------------------------------
CREATE OR REPLACE PROCEDURE "CARDGAME"."SP_PLACE_ORDER_SINGLE"
(
    p_customer_id        IN  NUMBER,
    p_product_id         IN  NUMBER,
    p_quantity           IN  NUMBER,
    p_shipping_address   IN  NVARCHAR2,
    p_payment_method_id  IN  NUMBER,
    p_order_email        IN  VARCHAR2 DEFAULT NULL,
    p_note               IN  NVARCHAR2 DEFAULT NULL,
    p_order_id           OUT NUMBER
)
AS
    v_price         NUMBER(12, 0);
    v_is_active     NUMBER(1, 0);
    v_qty           NUMBER(10, 0);
    v_reserved      NUMBER(10, 0);
    v_available     NUMBER(10, 0);
    v_now           DATE := SYSDATE;
BEGIN
    IF p_customer_id IS NULL OR p_customer_id <= 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Invalid customer id');
    END IF;

    IF p_product_id IS NULL OR p_product_id <= 0 THEN
        RAISE_APPLICATION_ERROR(-20002, 'Invalid product id');
    END IF;

    IF p_quantity IS NULL OR p_quantity <= 0 THEN
        RAISE_APPLICATION_ERROR(-20003, 'Invalid quantity');
    END IF;

    SAVEPOINT sp_before_place;

    SELECT p."PRICE", p."IS_ACTIVE"
      INTO v_price, v_is_active
      FROM "CARDGAME"."PRODUCTS" p
     WHERE p."ID" = p_product_id
       FOR UPDATE;

    IF v_is_active <> 1 THEN
        RAISE_APPLICATION_ERROR(-20004, 'Product is inactive');
    END IF;

    IF v_price IS NULL OR v_price <= 0 THEN
        RAISE_APPLICATION_ERROR(-20005, 'Product price is invalid');
    END IF;

    SELECT i."QUANTITY", i."RESERVED_QUANTITY"
      INTO v_qty, v_reserved
      FROM "CARDGAME"."INVENTORY" i
     WHERE i."PRODUCT_ID" = p_product_id
       FOR UPDATE;

    v_available := NVL(v_qty, 0) - NVL(v_reserved, 0);

    IF v_available < p_quantity THEN
        RAISE_APPLICATION_ERROR(-20006, 'Not enough stock');
    END IF;

    INSERT INTO "CARDGAME"."ORDERS"
    (
        "CUSTOMER_ID",
        "VOUCHER_ID",
        "AMOUNT",
        "DISCOUNT_AMOUNT",
        "SHIPPING_ADDRESS",
        "ORDER_EMAIL",
        "NOTE",
        "PAYMENT_METHOD_ID",
        "ORDER_DATE",
        "ORDER_STATUS"
    )
    VALUES
    (
        p_customer_id,
        NULL,
        v_price * p_quantity,
        0,
        p_shipping_address,
        p_order_email,
        p_note,
        p_payment_method_id,
        v_now,
        'PENDING'
    )
    RETURNING "ID" INTO p_order_id;

    INSERT INTO "CARDGAME"."ORDER_DETAILS"
    (
        "ORDER_ID",
        "PRODUCT_ID",
        "SKU",
        "PRICE",
        "QUANTITY"
    )
    SELECT
        p_order_id,
        p."ID",
        p."SKU",
        v_price,
        p_quantity
    FROM "CARDGAME"."PRODUCTS" p
    WHERE p."ID" = p_product_id;

    INSERT INTO "CARDGAME"."PAYMENTS"
    (
        "ORDER_ID",
        "PAYMENT_METHOD_ID",
        "AMOUNT",
        "STATUS",
        "TRANSACTION_ID",
        "PAID_AT",
        "CREATED_AT"
    )
    VALUES
    (
        p_order_id,
        p_payment_method_id,
        v_price * p_quantity,
        'PENDING',
        NULL,
        NULL,
        v_now
    );

    UPDATE "CARDGAME"."INVENTORY"
       SET "RESERVED_QUANTITY" = NVL("RESERVED_QUANTITY", 0) + p_quantity,
           "UPDATED_AT" = v_now
     WHERE "PRODUCT_ID" = p_product_id;

    COMMIT;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK TO sp_before_place;
        RAISE_APPLICATION_ERROR(-20007, 'Product or inventory not found');
    WHEN OTHERS THEN
        ROLLBACK TO sp_before_place;
        RAISE;
END;
/

--------------------------------------------------------
-- Procedure 2: Cancel pending order
-- - Locks order row
-- - Only allows PENDING
-- - Releases reserved stock
-- - Sets order status CANCELLED atomically
--------------------------------------------------------
CREATE OR REPLACE PROCEDURE "CARDGAME"."SP_CANCEL_PENDING_ORDER"
(
    p_order_id     IN NUMBER,
    p_customer_id  IN NUMBER DEFAULT NULL
)
AS
    v_status   VARCHAR2(20);
BEGIN
    IF p_order_id IS NULL OR p_order_id <= 0 THEN
        RAISE_APPLICATION_ERROR(-20101, 'Invalid order id');
    END IF;

    SAVEPOINT sp_before_cancel;

    IF p_customer_id IS NULL THEN
        SELECT o."ORDER_STATUS"
          INTO v_status
          FROM "CARDGAME"."ORDERS" o
         WHERE o."ID" = p_order_id
           FOR UPDATE;
    ELSE
        SELECT o."ORDER_STATUS"
          INTO v_status
          FROM "CARDGAME"."ORDERS" o
         WHERE o."ID" = p_order_id
           AND o."CUSTOMER_ID" = p_customer_id
           FOR UPDATE;
    END IF;

    IF UPPER(TRIM(v_status)) <> 'PENDING' THEN
        RAISE_APPLICATION_ERROR(-20102, 'Only PENDING orders can be cancelled');
    END IF;

    FOR rec IN (
        SELECT d."PRODUCT_ID" AS product_id, SUM(d."QUANTITY") AS total_qty
          FROM "CARDGAME"."ORDER_DETAILS" d
         WHERE d."ORDER_ID" = p_order_id
         GROUP BY d."PRODUCT_ID"
    ) LOOP
        UPDATE "CARDGAME"."INVENTORY" i
           SET i."RESERVED_QUANTITY" = GREATEST(0, NVL(i."RESERVED_QUANTITY", 0) - rec.total_qty),
               i."UPDATED_AT" = SYSDATE
         WHERE i."PRODUCT_ID" = rec.product_id;
    END LOOP;

    UPDATE "CARDGAME"."ORDERS"
       SET "ORDER_STATUS" = 'CANCELLED'
     WHERE "ID" = p_order_id;

    UPDATE "CARDGAME"."PAYMENTS"
       SET "STATUS" = 'FAILED'
     WHERE "ORDER_ID" = p_order_id
       AND UPPER(NVL("STATUS", 'PENDING')) = 'PENDING';

    COMMIT;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK TO sp_before_cancel;
        RAISE_APPLICATION_ERROR(-20103, 'Order not found or not owned by customer');
    WHEN OTHERS THEN
        ROLLBACK TO sp_before_cancel;
        RAISE;
END;
/

--------------------------------------------------------
-- Procedure 3: Complete order (ship delivered)
-- - Locks order row
-- - Converts reserved stock to real deduction (QUANTITY)
-- - Updates order status DONE and payment SUCCESS atomically
--------------------------------------------------------
CREATE OR REPLACE PROCEDURE "CARDGAME"."SP_COMPLETE_ORDER"
(
    p_order_id IN NUMBER
)
AS
    v_status VARCHAR2(20);
BEGIN
    IF p_order_id IS NULL OR p_order_id <= 0 THEN
        RAISE_APPLICATION_ERROR(-20201, 'Invalid order id');
    END IF;

    SAVEPOINT sp_before_complete;

    SELECT o."ORDER_STATUS"
      INTO v_status
      FROM "CARDGAME"."ORDERS" o
     WHERE o."ID" = p_order_id
       FOR UPDATE;

    IF UPPER(TRIM(v_status)) IN ('DONE', 'CANCELLED') THEN
        RAISE_APPLICATION_ERROR(-20202, 'Order is already final state');
    END IF;

    FOR rec IN (
        SELECT d."PRODUCT_ID" AS product_id, SUM(d."QUANTITY") AS total_qty
          FROM "CARDGAME"."ORDER_DETAILS" d
         WHERE d."ORDER_ID" = p_order_id
         GROUP BY d."PRODUCT_ID"
    ) LOOP
        UPDATE "CARDGAME"."INVENTORY" i
           SET i."RESERVED_QUANTITY" = GREATEST(0, NVL(i."RESERVED_QUANTITY", 0) - rec.total_qty),
               i."QUANTITY" = GREATEST(0, NVL(i."QUANTITY", 0) - rec.total_qty),
               i."UPDATED_AT" = SYSDATE
         WHERE i."PRODUCT_ID" = rec.product_id;

        IF SQL%ROWCOUNT = 0 THEN
            RAISE_APPLICATION_ERROR(-20203, 'Inventory row not found for product');
        END IF;
    END LOOP;

    UPDATE "CARDGAME"."ORDERS"
       SET "ORDER_STATUS" = 'DONE'
     WHERE "ID" = p_order_id;

    UPDATE "CARDGAME"."PAYMENTS"
       SET "STATUS" = 'SUCCESS',
           "PAID_AT" = NVL("PAID_AT", SYSDATE)
     WHERE "ORDER_ID" = p_order_id;

    COMMIT;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK TO sp_before_complete;
        RAISE_APPLICATION_ERROR(-20204, 'Order not found');
    WHEN OTHERS THEN
        ROLLBACK TO sp_before_complete;
        RAISE;
END;
/

--------------------------------------------------------
-- Usage examples
--------------------------------------------------------
-- DECLARE
--   v_new_order_id NUMBER;
-- BEGIN
--   "CARDGAME"."SP_PLACE_ORDER_SINGLE"(
--       p_customer_id       => 1,
--       p_product_id        => 10,
--       p_quantity          => 2,
--       p_shipping_address  => '123 Tran Hung Dao, Q1, HCM',
--       p_payment_method_id => 1,
--       p_order_email       => 'user@demo.com',
--       p_note              => 'Giao gio hanh chinh',
--       p_order_id          => v_new_order_id
--   );
--   DBMS_OUTPUT.PUT_LINE('New order id = ' || v_new_order_id);
-- END;
-- /
--
-- BEGIN
--   "CARDGAME"."SP_CANCEL_PENDING_ORDER"(p_order_id => 1001, p_customer_id => 1);
-- END;
-- /
--
-- BEGIN
--   "CARDGAME"."SP_COMPLETE_ORDER"(p_order_id => 1001);
-- END;
-- /
