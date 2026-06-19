using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustryTrade.Modules.Notifications.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NotificationRouting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Recipient",
                schema: "notifications",
                table: "notification",
                newName: "TargetPermission");

            migrationBuilder.RenameIndex(
                name: "IX_notification_Recipient_IsRead",
                schema: "notifications",
                table: "notification",
                newName: "IX_notification_TargetPermission_IsRead");

            migrationBuilder.AddColumn<Guid>(
                name: "OrgUnitId",
                schema: "notifications",
                table: "notification",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrgUnitId",
                schema: "notifications",
                table: "notification");

            migrationBuilder.RenameColumn(
                name: "TargetPermission",
                schema: "notifications",
                table: "notification",
                newName: "Recipient");

            migrationBuilder.RenameIndex(
                name: "IX_notification_TargetPermission_IsRead",
                schema: "notifications",
                table: "notification",
                newName: "IX_notification_Recipient_IsRead");
        }
    }
}
