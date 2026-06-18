using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustryTrade.Modules.Integration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration");

            migrationBuilder.CreateTable(
                name: "connection_status_check",
                schema: "integration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Component = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Healthy = table.Column<bool>(type: "boolean", nullable: false),
                    Detail = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CheckedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connection_status_check", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "data_sharing_service",
                schema: "integration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    EndpointUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_sharing_service", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_connection_status_check_CheckedAtUtc",
                schema: "integration",
                table: "connection_status_check",
                column: "CheckedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_data_sharing_service_Code",
                schema: "integration",
                table: "data_sharing_service",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "connection_status_check",
                schema: "integration");

            migrationBuilder.DropTable(
                name: "data_sharing_service",
                schema: "integration");
        }
    }
}
