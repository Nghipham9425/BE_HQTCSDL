-- Seed initial accounts for RBAC testing/operation.
-- This script is idempotent: it can be executed multiple times.
-- Prerequisite: run 2026-04-04-rbac-user-roles.sql first.

-- Ensure ROLE column can store ORDER_MANAGER / INVENTORY_MANAGER.
ALTER TABLE "CARDGAME"."USERS" MODIFY ("ROLE" VARCHAR2(30 BYTE));
/

MERGE INTO "CARDGAME"."USERS" u
USING (
    SELECT 'admin@cardgame.com' AS email,
           'Admin@123' AS password,
           'System Admin' AS full_name,
           'ADMIN' AS role_name
    FROM dual
) s
ON (UPPER(u."EMAIL") = UPPER(s.email))
WHEN MATCHED THEN
    UPDATE SET u."FULL_NAME" = s.full_name,
               u."ROLE" = s.role_name
WHEN NOT MATCHED THEN
    INSERT ("EMAIL", "PASSWORD", "FULL_NAME", "ROLE", "CREATED_AT")
    VALUES (s.email, s.password, s.full_name, s.role_name, SYSDATE);

MERGE INTO "CARDGAME"."USERS" u
USING (
    SELECT 'order@cardgame.com' AS email,
           'Order@123' AS password,
           'Order Manager' AS full_name,
           'ORDER_MANAGER' AS role_name
    FROM dual
) s
ON (UPPER(u."EMAIL") = UPPER(s.email))
WHEN MATCHED THEN
    UPDATE SET u."FULL_NAME" = s.full_name,
               u."ROLE" = s.role_name
WHEN NOT MATCHED THEN
    INSERT ("EMAIL", "PASSWORD", "FULL_NAME", "ROLE", "CREATED_AT")
    VALUES (s.email, s.password, s.full_name, s.role_name, SYSDATE);

MERGE INTO "CARDGAME"."USERS" u
USING (
    SELECT 'warehouse@cardgame.com' AS email,
           'Warehouse@123' AS password,
           'Inventory Manager' AS full_name,
           'INVENTORY_MANAGER' AS role_name
    FROM dual
) s
ON (UPPER(u."EMAIL") = UPPER(s.email))
WHEN MATCHED THEN
    UPDATE SET u."FULL_NAME" = s.full_name,
               u."ROLE" = s.role_name
WHEN NOT MATCHED THEN
    INSERT ("EMAIL", "PASSWORD", "FULL_NAME", "ROLE", "CREATED_AT")
    VALUES (s.email, s.password, s.full_name, s.role_name, SYSDATE);

MERGE INTO "CARDGAME"."USERS" u
USING (
    SELECT 'user@cardgame.com' AS email,
           'User@123' AS password,
           'Demo User' AS full_name,
           'USER' AS role_name
    FROM dual
) s
ON (UPPER(u."EMAIL") = UPPER(s.email))
WHEN MATCHED THEN
    UPDATE SET u."FULL_NAME" = s.full_name,
               u."ROLE" = s.role_name
WHEN NOT MATCHED THEN
    INSERT ("EMAIL", "PASSWORD", "FULL_NAME", "ROLE", "CREATED_AT")
    VALUES (s.email, s.password, s.full_name, s.role_name, SYSDATE);

COMMIT;
