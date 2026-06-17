using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "org_unit",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Path = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_unit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_org_unit_org_unit_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "identity",
                        principalTable: "org_unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message",
                schema: "identity",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_message", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_org_unit_Code",
                schema: "identity",
                table: "org_unit",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_org_unit_ParentId",
                schema: "identity",
                table: "org_unit",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_org_unit_Path",
                schema: "identity",
                table: "org_unit",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_ProcessedOnUtc",
                schema: "identity",
                table: "outbox_message",
                column: "ProcessedOnUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "org_unit",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "outbox_message",
                schema: "identity");
        }
    }
}
