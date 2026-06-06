using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library_Management_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class columnadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "country",
                table: "AspNetUsers");
        }
    }
}
