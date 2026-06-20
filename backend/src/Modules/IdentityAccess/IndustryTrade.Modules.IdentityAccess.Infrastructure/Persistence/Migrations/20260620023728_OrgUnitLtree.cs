using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustryTrade.Modules.IdentityAccess.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OrgUnitLtree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_org_unit_Path",
                schema: "identity",
                table: "org_unit");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:ltree", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            // text → ltree needs an explicit USING cast (no implicit cast exists); existing dot-paths
            // are already valid ltree labels (org-unit codes are sanitized to [A-Za-z0-9_]).
            migrationBuilder.Sql(
                @"ALTER TABLE identity.org_unit ALTER COLUMN ""Path"" TYPE ltree USING ""Path""::ltree;");

            migrationBuilder.CreateIndex(
                name: "IX_org_unit_Path",
                schema: "identity",
                table: "org_unit",
                column: "Path")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_org_unit_Path",
                schema: "identity",
                table: "org_unit");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:ltree", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.Sql(
                @"ALTER TABLE identity.org_unit ALTER COLUMN ""Path"" TYPE text USING ""Path""::text;");

            migrationBuilder.CreateIndex(
                name: "IX_org_unit_Path",
                schema: "identity",
                table: "org_unit",
                column: "Path");
        }
    }
}
