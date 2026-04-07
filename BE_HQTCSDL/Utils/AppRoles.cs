namespace BE_HQTCSDL.Utils
{
    public static class AppRoles
    {
        public const string User = "USER";
        public const string Admin = "ADMIN";
        public const string OrderManager = "ORDER_MANAGER";
        public const string InventoryManager = "INVENTORY_MANAGER";

        public const string AdminOrOrderManager = "ADMIN,ORDER_MANAGER";
        public const string AdminOrInventoryManager = "ADMIN,INVENTORY_MANAGER";
    }
}