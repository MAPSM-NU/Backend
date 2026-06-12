using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class UserStatsNamesAndWoroutsMissed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "goalsCompleted",
                table: "UserStats");

            migrationBuilder.RenameColumn(
                name: "totalWorkouts",
                table: "UserStats",
                newName: "totalWorkoutsMissed");

            migrationBuilder.RenameColumn(
                name: "totalExercises",
                table: "UserStats",
                newName: "totalWorkoutsCompleted");

            migrationBuilder.RenameColumn(
                name: "goalsFailed",
                table: "UserStats",
                newName: "totalExercisesCompleted");

            migrationBuilder.RenameColumn(
                name: "goalCompletionRate",
                table: "UserStats",
                newName: "workoutCompletionRate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "workoutCompletionRate",
                table: "UserStats",
                newName: "goalCompletionRate");

            migrationBuilder.RenameColumn(
                name: "totalWorkoutsMissed",
                table: "UserStats",
                newName: "totalWorkouts");

            migrationBuilder.RenameColumn(
                name: "totalWorkoutsCompleted",
                table: "UserStats",
                newName: "totalExercises");

            migrationBuilder.RenameColumn(
                name: "totalExercisesCompleted",
                table: "UserStats",
                newName: "goalsFailed");

            migrationBuilder.AddColumn<int>(
                name: "goalsCompleted",
                table: "UserStats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
