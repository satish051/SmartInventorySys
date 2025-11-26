using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartInventorySys.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentsToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "Orders");
        }
    }
}
