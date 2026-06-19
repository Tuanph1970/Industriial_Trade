using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IndustryTrade.Modules.Reporting.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                schema: "reporting",
                table: "report_submission",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "report_line",
                schema: "reporting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IndicatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    IndicatorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Label = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    RowOrder = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: true),
                    ValueText = table.Column<string>(type: "text", nullable: true),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_line", x => x.Id);
                    table.ForeignKey(
                        name: "FK_report_line_report_submission_SubmissionId",
                        column: x => x.SubmissionId,
                        principalSchema: "reporting",
                        principalTable: "report_submission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_line_SubmissionId",
                schema: "reporting",
                table: "report_line",
                column: "SubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "report_line",
                schema: "reporting");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                schema: "reporting",
                table: "report_submission");
        }
    }
}
