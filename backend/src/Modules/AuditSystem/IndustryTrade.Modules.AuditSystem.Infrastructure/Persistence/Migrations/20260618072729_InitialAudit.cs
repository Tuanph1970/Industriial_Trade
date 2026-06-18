using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustryTrade.Modules.AuditSystem.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.CreateTable(
                name: "log_entry",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Action = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    AtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_log_entry", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_log_entry_Action",
                schema: "audit",
                table: "log_entry",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_log_entry_Actor",
                schema: "audit",
                table: "log_entry",
                column: "Actor");

            migrationBuilder.CreateIndex(
                name: "IX_log_entry_AtUtc",
                schema: "audit",
                table: "log_entry",
                column: "AtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "log_entry",
                schema: "audit");
        }
    }
}
