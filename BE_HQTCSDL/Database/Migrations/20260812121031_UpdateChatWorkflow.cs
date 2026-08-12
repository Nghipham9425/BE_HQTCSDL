using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_HQTCSDL.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChatWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "SENDER_ID",
                table: "CHAT_MESSAGES",
                type: "NUMBER(19)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)");

            migrationBuilder.AddColumn<string>(
                name: "SENDER_TYPE",
                table: "CHAT_MESSAGES",
                type: "NVARCHAR2(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "CUSTOMER");

            migrationBuilder.Sql(
                "UPDATE \"CONVERSATIONS\" SET \"STATUS\" = 'AI_ACTIVE' WHERE \"STATUS\" = 'OPEN'");

            migrationBuilder.CreateIndex(
                name: "IX_CONVERSATIONS_STATUS",
                table: "CONVERSATIONS",
                column: "STATUS");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CONVERSATIONS_STATUS",
                table: "CONVERSATIONS",
                sql: "\"STATUS\" IN ('AI_ACTIVE', 'WAITING_STAFF', 'STAFF_ACTIVE', 'CLOSED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CONVERSATIONS_TYPE",
                table: "CONVERSATIONS",
                sql: "\"TYPE\" IN ('GENERAL_SUPPORT', 'ORDER_SUPPORT')");

            migrationBuilder.CreateIndex(
                name: "IX_CHAT_MESSAGES_CONVERSATION_ID_CREATED_AT",
                table: "CHAT_MESSAGES",
                columns: new[] { "CONVERSATION_ID", "CREATED_AT" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_CHAT_MESSAGES_SENDER_ID",
                table: "CHAT_MESSAGES",
                sql: "((\"SENDER_TYPE\" IN ('CUSTOMER', 'STAFF') AND \"SENDER_ID\" IS NOT NULL) OR (\"SENDER_TYPE\" IN ('AI', 'SYSTEM') AND \"SENDER_ID\" IS NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CHAT_MESSAGES_SENDER_TYPE",
                table: "CHAT_MESSAGES",
                sql: "\"SENDER_TYPE\" IN ('CUSTOMER', 'STAFF', 'AI', 'SYSTEM')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONVERSATIONS_STATUS",
                table: "CONVERSATIONS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CONVERSATIONS_STATUS",
                table: "CONVERSATIONS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CONVERSATIONS_TYPE",
                table: "CONVERSATIONS");

            migrationBuilder.DropIndex(
                name: "IX_CHAT_MESSAGES_CONVERSATION_ID_CREATED_AT",
                table: "CHAT_MESSAGES");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CHAT_MESSAGES_SENDER_ID",
                table: "CHAT_MESSAGES");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CHAT_MESSAGES_SENDER_TYPE",
                table: "CHAT_MESSAGES");

            migrationBuilder.Sql(
                "DELETE FROM \"CHAT_MESSAGES\" WHERE \"SENDER_ID\" IS NULL");

            migrationBuilder.Sql(
                "UPDATE \"CONVERSATIONS\" SET \"STATUS\" = 'OPEN' WHERE \"STATUS\" <> 'CLOSED'");

            migrationBuilder.DropColumn(
                name: "SENDER_TYPE",
                table: "CHAT_MESSAGES");

            migrationBuilder.AlterColumn<long>(
                name: "SENDER_ID",
                table: "CHAT_MESSAGES",
                type: "NUMBER(19)",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "NUMBER(19)",
                oldNullable: true);
        }
    }
}
