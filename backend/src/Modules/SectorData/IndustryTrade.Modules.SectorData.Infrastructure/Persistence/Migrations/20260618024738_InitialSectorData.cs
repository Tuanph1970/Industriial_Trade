using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace IndustryTrade.Modules.SectorData.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSectorData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sector");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "indicator_observation",
                schema: "sector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: true),
                    Value = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    ValueText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Source = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_indicator_observation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "industrial_cluster",
                schema: "sector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    AreaHa = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    Location = table.Column<Point>(type: "geometry (Point, 4326)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_industrial_cluster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message",
                schema: "sector",
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
                name: "IX_indicator_observation_IndicatorId_OrgUnitId_PeriodYear",
                schema: "sector",
                table: "indicator_observation",
                columns: new[] { "IndicatorId", "OrgUnitId", "PeriodYear" });

            migrationBuilder.CreateIndex(
                name: "IX_indicator_observation_OrgUnitId",
                schema: "sector",
                table: "indicator_observation",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_industrial_cluster_Code",
                schema: "sector",
                table: "industrial_cluster",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_industrial_cluster_Location",
                schema: "sector",
                table: "industrial_cluster",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_industrial_cluster_OrgUnitId",
                schema: "sector",
                table: "industrial_cluster",
                column: "OrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_message_ProcessedOnUtc",
                schema: "sector",
                table: "outbox_message",
                column: "ProcessedOnUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "indicator_observation",
                schema: "sector");

            migrationBuilder.DropTable(
                name: "industrial_cluster",
                schema: "sector");

            migrationBuilder.DropTable(
                name: "outbox_message",
                schema: "sector");
        }
    }
}
