namespace BE_HQTCSDL.Models;

public static class ChatSenderTypes
{
    public const string Customer = "CUSTOMER";
    public const string Staff = "STAFF";
    public const string Ai = "AI";
    public const string System = "SYSTEM";
}

public static class ConversationStatuses
{
    public const string AiActive = "AI_ACTIVE";
    public const string WaitingStaff = "WAITING_STAFF";
    public const string StaffActive = "STAFF_ACTIVE";
    public const string Closed = "CLOSED";
}

public static class ConversationTypes
{
    public const string GeneralSupport = "GENERAL_SUPPORT";
    public const string OrderSupport = "ORDER_SUPPORT";

    public static bool IsValid(string value) =>
        value is GeneralSupport or OrderSupport;
}
