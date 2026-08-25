using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalFormsSystem.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDamagedReportFollowUps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DamagedReportFollowUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DamagedReportId = table.Column<int>(type: "int", nullable: false),
                    FollowUpDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamagedReportFollowUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamagedReportFollowUps_DamagedReports_DamagedReportId",
                        column: x => x.DamagedReportId,
                        principalTable: "DamagedReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DamagedReportFollowUps_DamagedReportId",
                table: "DamagedReportFollowUps",
                column: "DamagedReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DamagedReportFollowUps");
        }
    }
}
