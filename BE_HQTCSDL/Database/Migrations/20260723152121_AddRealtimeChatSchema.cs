using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BE_HQTCSDL.Database.Migrations;

public partial class AddRealtimeChatSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "CONVERSATIONS",
            columns: table => new
            {
                ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                    .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                CUSTOMER_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                ASSIGNED_STAFF_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                ORDER_ID = table.Column<long>(type: "NUMBER(19)", nullable: true),
                TYPE = table.Column<string>(type: "NVARCHAR2(30)", maxLength: 30, nullable: false),
                STATUS = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CONVERSATIONS", x => x.ID);
                table.ForeignKey(
                    name: "FK_CONVERSATIONS_ORDERS_ORDER_ID",
                    column: x => x.ORDER_ID,
                    principalTable: "ORDERS",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CONVERSATIONS_USERS_ASSIGNED_STAFF_ID",
                    column: x => x.ASSIGNED_STAFF_ID,
                    principalTable: "USERS",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_CONVERSATIONS_USERS_CUSTOMER_ID",
                    column: x => x.CUSTOMER_ID,
                    principalTable: "USERS",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "CHAT_MESSAGES",
            columns: table => new
            {
                ID = table.Column<long>(type: "NUMBER(19)", nullable: false)
                    .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                CONVERSATION_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                SENDER_ID = table.Column<long>(type: "NUMBER(19)", nullable: false),
                CONTENT = table.Column<string>(type: "NVARCHAR2(2000)", maxLength: 2000, nullable: false),
                IS_READ = table.Column<bool>(type: "BOOLEAN", nullable: false),
                CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CHAT_MESSAGES", x => x.ID);
                table.ForeignKey(
                    name: "FK_CHAT_MESSAGES_CONVERSATIONS_CONVERSATION_ID",
                    column: x => x.CONVERSATION_ID,
                    principalTable: "CONVERSATIONS",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_CHAT_MESSAGES_USERS_SENDER_ID",
                    column: x => x.SENDER_ID,
                    principalTable: "USERS",
                    principalColumn: "ID",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_MESSAGES_CONVERSATION_ID",
            table: "CHAT_MESSAGES",
            column: "CONVERSATION_ID");

        migrationBuilder.CreateIndex(
            name: "IX_CHAT_MESSAGES_SENDER_ID",
            table: "CHAT_MESSAGES",
            column: "SENDER_ID");

        migrationBuilder.CreateIndex(
            name: "IX_CONVERSATIONS_ASSIGNED_STAFF_ID",
            table: "CONVERSATIONS",
            column: "ASSIGNED_STAFF_ID");

        migrationBuilder.CreateIndex(
            name: "IX_CONVERSATIONS_CUSTOMER_ID",
            table: "CONVERSATIONS",
            column: "CUSTOMER_ID");

        migrationBuilder.CreateIndex(
            name: "IX_CONVERSATIONS_ORDER_ID",
            table: "CONVERSATIONS",
            column: "ORDER_ID");

        migrationBuilder.CreateIndex(
            name: "IX_CONVERSATIONS_UPDATED_AT",
            table: "CONVERSATIONS",
            column: "UPDATED_AT");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CHAT_MESSAGES");
        migrationBuilder.DropTable(name: "CONVERSATIONS");
    }
}
