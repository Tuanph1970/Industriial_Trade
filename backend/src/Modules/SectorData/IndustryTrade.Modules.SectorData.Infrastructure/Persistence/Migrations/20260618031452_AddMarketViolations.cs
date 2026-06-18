using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustryTrade.Modules.SectorData.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketViolations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "market_violation_case",
                schema: "sector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CaseNo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Group = table.Column<int>(type: "integer", nullable: false),
                    OrgUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                    BusinessName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    InspectedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ViolationContent = table.Column<string>(type: "text", nullable: false),
                    SanctionContent = table.Column<string>(type: "text", nullable: true),
                    FineAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    ModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_violation_case", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_market_violation_case_CaseNo",
                schema: "sector",
                table: "market_violation_case",
                column: "CaseNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_market_violation_case_OrgUnitId_Group",
                schema: "sector",
                table: "market_violation_case",
                columns: new[] { "OrgUnitId", "Group" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "market_violation_case",
                schema: "sector");
        }
    }
}
