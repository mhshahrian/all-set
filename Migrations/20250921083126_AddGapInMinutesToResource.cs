using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllSet.Migrations
{
    /// <inheritdoc />
    public partial class AddGapInMinutesToResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GapInMinutes",
                table: "Resources",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GapInMinutes",
                table: "Resources");
        }
    }
}
