using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace IndustryTrade.Modules.SectorData.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPetrolCommerceEcommerce : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "commerce_location",
                schema: "sector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Location = table.Column<Point>(type: "geometry (Point, 4326)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commerce_location", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ecommerce_participant",
                schema: "sector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TaxCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Platforms = table.Column<string[]>(type: "text[]", nullable: false),
                    MainGoods = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ecommerce_participant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "petroleum_station",
                schema: "sector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Location = table.Column<Point>(type: "geometry (Point, 4326)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_petroleum_station", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_commerce_location_Code",
                schema: "sector",
                table: "commerce_location",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commerce_location_Location",
                schema: "sector",
                table: "commerce_location",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_commerce_location_OrgUnitId_Type",
                schema: "sector",
                table: "commerce_location",
                columns: new[] { "OrgUnitId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_ecommerce_participant_OrgUnitId",
                schema: "sector",
                table: "ecommerce_participant",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ecommerce_participant_TaxCode",
                schema: "sector",
                table: "ecommerce_participant",
                column: "TaxCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_petroleum_station_Code",
                schema: "sector",
                table: "petroleum_station",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_petroleum_station_Location",
                schema: "sector",
                table: "petroleum_station",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_petroleum_station_OrgUnitId",
                schema: "sector",
                table: "petroleum_station",
                column: "OrgUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commerce_location",
                schema: "sector");

            migrationBuilder.DropTable(
                name: "ecommerce_participant",
                schema: "sector");

            migrationBuilder.DropTable(
                name: "petroleum_station",
                schema: "sector");
        }
    }
}
