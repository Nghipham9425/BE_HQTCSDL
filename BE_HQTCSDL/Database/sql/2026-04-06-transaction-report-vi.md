ú # Báo cáo Transaction cho hệ thống CARDGAME (Oracle)

## 1. Khái niệm Transaction
Transaction là một nhóm câu lệnh SQL được thực thi như một khối thống nhất, đảm bảo:
- Hoặc thành công toàn bộ (COMMIT)
- Hoặc hủy toàn bộ (ROLLBACK)

Hệ thống sử dụng transaction để giữ toàn vẹn dữ liệu cho các nghiệp vụ đặt hàng, thanh toán và tồn kho.

## 2. Môi trường và mô hình dữ liệu
- DBMS: Oracle
- Schema: CARDGAME
- Bảng liên quan chính:
  - ORDERS
  - ORDER_DETAILS
  - PAYMENTS
  - PRODUCTS
  - INVENTORY

Mô hình tồn kho đang áp dụng:
- QUANTITY: tồn kho thực tế
- RESERVED_QUANTITY: số lượng đã giữ cho đơn đang xử lý
- AVAILABLE = QUANTITY - RESERVED_QUANTITY

## 3. Các Transaction chính

### 3.1 Transaction đặt hàng (Place Order)
**Tên procedure:** SP_PLACE_ORDER_SINGLE

**Mục tiêu:**
- Khóa dòng PRODUCTS và INVENTORY để tránh race condition
- Kiểm tra sản phẩm active và đủ hàng
- Tạo ORDER + ORDER_DETAILS + PAYMENT
- Tăng RESERVED_QUANTITY

**Trình tự xử lý:**
1. SAVEPOINT
2. SELECT ... FOR UPDATE trên PRODUCTS
3. SELECT ... FOR UPDATE trên INVENTORY
4. Kiểm tra AVAILABLE >= số lượng đặt
5. INSERT ORDERS
6. INSERT ORDER_DETAILS
7. INSERT PAYMENTS (STATUS = PENDING)
8. UPDATE INVENTORY.RESERVED_QUANTITY
9. COMMIT
10. Nếu lỗi: ROLLBACK TO SAVEPOINT

**Ý nghĩa nghiệp vụ:**
- Khi vừa đặt hàng, hệ thống giữ chỗ hàng trước, chưa trừ kho thật.
- Đảm bảo 2 người đặt đồng thời không vượt quá số hàng khả dụng.

**Ví dụ lệnh SQL:**
```sql
BEGIN
    SP_PLACE_ORDER_SINGLE(1, 101, 2); -- UserID=1, ProductID=101, Quantity=2
END;
```

**Định nghĩa đầy đủ procedure:**
```sql
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
```

### 3.2 Transaction hủy đơn đang chờ (Cancel Pending Order)
**Tên procedure:** SP_CANCEL_PENDING_ORDER

**Mục tiêu:**
- Chỉ cho hủy đơn trạng thái PENDING
- Hoàn trả số lượng đã giữ chỗ
- Chuyển ORDER_STATUS thành CANCELLED
- Chuyển PAYMENT PENDING thành FAILED

**Trình tự xử lý:**
1. SAVEPOINT
2. Khóa dòng ORDER bằng FOR UPDATE
3. Kiểm tra status phải là PENDING
4. Duyệt ORDER_DETAILS theo từng PRODUCT_ID
5. Giảm INVENTORY.RESERVED_QUANTITY
6. UPDATE ORDERS.ORDER_STATUS = CANCELLED
7. UPDATE PAYMENTS.STATUS = FAILED (nếu đang PENDING)
8. COMMIT
9. Nếu lỗi: ROLLBACK TO SAVEPOINT

**Ý nghĩa nghiệp vụ:**
- Đơn bị hủy sẽ trả lại tồn khả dụng cho hệ thống.

**Ví dụ lệnh SQL:**
```sql
BEGIN
    SP_CANCEL_PENDING_ORDER(5001); -- OrderID=5001
END;
```

**Định nghĩa đầy đủ procedure:**
```sql
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
```

### 3.3 Transaction hoàn tất đơn (Complete Order)
**Tên procedure:** SP_COMPLETE_ORDER

**Mục tiêu:**
- Chốt đơn giao thành công
- Trừ kho thật từ QUANTITY
- Giảm RESERVED_QUANTITY tương ứng
- Chuyển ORDER_STATUS = DONE
- Chuyển PAYMENT.STATUS = SUCCESS

**Trình tự xử lý:**
1. SAVEPOINT
2. Khóa dòng ORDER bằng FOR UPDATE
3. Chặn nếu đơn đã ở trạng thái cuối (DONE/CANCELLED)
4. Duyệt ORDER_DETAILS theo từng PRODUCT_ID
5. UPDATE INVENTORY:
   - RESERVED_QUANTITY = RESERVED_QUANTITY - số lượng
   - QUANTITY = QUANTITY - số lượng
6. UPDATE ORDERS.ORDER_STATUS = DONE
7. UPDATE PAYMENTS.STATUS = SUCCESS, PAID_AT
8. COMMIT
9. Nếu lỗi: ROLLBACK TO SAVEPOINT

**Ý nghĩa nghiệp vụ:**
- Lúc này mới trừ kho thực tế, phù hợp mô hình reserve trước, xuất kho sau.

**Ví dụ lệnh SQL:**
```sql
BEGIN
    SP_COMPLETE_ORDER(5001); -- OrderID=5001
END;
```

**Định nghĩa đầy đủ procedure:**
```sql
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
```

### 3.4 Transaction xác nhận thanh toán (Payment Confirmation)
**Tên procedure:** SP_CONFIRM_PAYMENT

**Mục tiêu:**
- Sau khi thanh toán thành công (qua VNPay hoặc phương thức khác), cập nhật trạng thái
- Liên kết với hệ thống thanh toán bên ngoài
- Đảm bảo đơn hàng chỉ được xử lý sau khi thanh toán

**Trình tự xử lý:**
1. SAVEPOINT
2. Khóa dòng ORDER bằng FOR UPDATE
3. Kiểm tra đơn hàng có STATUS = 'PENDING' không
4. Cập nhật ORDERS.STATUS = 'PAID' (hoặc tương ứng)
5. Cập nhật PAYMENTS.STATUS = 'SUCCESS', ghi TRANSACTION_ID
6. Ghi log thanh toán (PAYMENT_TRANSACTIONS nếu cần)
7. COMMIT
8. Nếu thất bại: ROLLBACK TO SAVEPOINT

**Ý nghĩa nghiệp vụ:**
- Đảm bảo tính toàn vẹn giữa đặt hàng và thanh toán, tránh lừa đảo.

**Ví dụ lệnh SQL:**
```sql
BEGIN
    SP_CONFIRM_PAYMENT(5001, 'VNPAY', 'TXN123456'); -- OrderID=5001, Method='VNPAY', TransactionID='TXN123456'
END;
```

**Lưu ý:** Trong file SQL hiện tại, việc xác nhận thanh toán được xử lý tự động qua trigger `TRG_PAYMENTS_CONFIRM_ORDER`. Nếu cần procedure riêng, có thể tạo thêm. Dưới đây là trigger xử lý:

**Trigger xác nhận thanh toán:**
```sql
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
```

**Trigger set PAID_AT:**
```sql
CREATE OR REPLACE TRIGGER "CARDGAME"."TRG_PAYMENTS_SET_PAID_AT"
BEFORE INSERT OR UPDATE OF "STATUS" ON "CARDGAME"."PAYMENTS"
FOR EACH ROW
BEGIN
    IF :NEW."STATUS" = 'SUCCESS' AND :NEW."PAID_AT" IS NULL THEN
        :NEW."PAID_AT" := SYSDATE;
    END IF;
END;
```

### 3.4 Transaction xac nhan thanh toan
Nguon xu ly: logic xac nhan webhook thanh toan

Muc tieu:
- Danh dau payment thanh cong
- Chuyen don tu PENDING sang CONFIRMED

Trinh tu tong quat:
1. Bat dau transaction
2. Khoa va kiem tra ORDER
3. Kiem tra amount/transaction id
4. UPDATE PAYMENTS.STATUS = SUCCESS
5. Neu ORDER dang PENDING -> UPDATE ORDER_STATUS = CONFIRMED
6. COMMIT
7. Neu loi: ROLLBACK

Y nghia nghiep vu:
- Dong bo trang thai giua thanh toan va don hang trong cung 1 transaction.

## 4. Tinh chat ACID duoc dam bao
- Atomicity: Moi nghiep vu chinh duoc commit/huy tron khoi
- Consistency: Rang buoc ton kho, status va khoa ngoai duoc giu dung
- Isolation: FOR UPDATE giam xung dot ghi dong thoi
- Durability: Sau COMMIT, du lieu duoc luu ben vung

## 5. Ly do thiet ke reserve stock
Viec tach QUANTITY va RESERVED_QUANTITY giup:
- Tranh over-selling khi nhieu khach dat cung luc
- Cho phep huy don de tra lai hang de dang
- Don gian hoa quy trinh xac nhan giao hang va tru kho that

## 6. Ket luan
He thong da xay dung day du cac transaction cot loi cho vong doi don hang:
- Dat don
- Huy don
- Hoan tat don
- Xac nhan thanh toan

Thiet ke nay phu hop voi he thong ban hang thuc te, dam bao toan ven du lieu va kha nang mo rong trong moi truong Oracle.
