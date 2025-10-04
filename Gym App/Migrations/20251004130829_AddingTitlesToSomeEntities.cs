using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class AddingTitlesToSomeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Feedbacks",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Challenges",
                type: "varchar(100)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Title",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Challenges");
        }
    }
}
