using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalFormsSystem.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddPlainTextPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlainTextPassword",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlainTextPassword",
                table: "Employees");
        }
    }
}
