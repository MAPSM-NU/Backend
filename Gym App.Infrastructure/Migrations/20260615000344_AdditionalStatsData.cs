using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class AdditionalStatsData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KCaloriesBurned",
                table: "WorkoutSets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "totalWorkoutCompleted",
                table: "UserStatsDaily",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KCaloriesBurned",
                table: "WorkoutSets");

            migrationBuilder.DropColumn(
                name: "totalWorkoutCompleted",
                table: "UserStatsDaily");
        }
    }
}
