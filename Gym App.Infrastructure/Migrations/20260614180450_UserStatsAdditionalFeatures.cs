using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class UserStatsAdditionalFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "activeDays",
                table: "UserStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "personalRecordsBroken",
                table: "UserStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "totalRepsCompleted",
                table: "UserStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "totalSetsCompleted",
                table: "UserStats",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "totalWeightLifted",
                table: "UserStats",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "weeklyGoalAchieved",
                table: "UserStats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserStatsMonthly",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    totalWorkoutsCompleted = table.Column<int>(type: "int", nullable: false),
                    totalWorkoutsMissed = table.Column<int>(type: "int", nullable: false),
                    totalExercisesCompleted = table.Column<int>(type: "int", nullable: false),
                    totalHours = table.Column<double>(type: "float", nullable: false),
                    workoutStreak = table.Column<int>(type: "int", nullable: false),
                    longestWorkoutStreak = table.Column<int>(type: "int", nullable: false),
                    workoutCompletionRate = table.Column<double>(type: "float", nullable: false),
                    activeDays = table.Column<int>(type: "int", nullable: false),
                    totalSetsCompleted = table.Column<int>(type: "int", nullable: false),
                    totalRepsCompleted = table.Column<int>(type: "int", nullable: false),
                    totalWeightLifted = table.Column<double>(type: "float", nullable: false),
                    personalRecordsBroken = table.Column<int>(type: "int", nullable: false),
                    weeklyGoalAchieved = table.Column<bool>(type: "bit", nullable: false),
                    KcaloriesBurned = table.Column<double>(type: "float", nullable: false),
                    monthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    monthName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    monthNumber = table.Column<int>(type: "int", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatsMonthly", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStatsMonthly_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserStatsWeekly",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    totalWorkoutsCompleted = table.Column<int>(type: "int", nullable: false),
                    totalWorkoutsMissed = table.Column<int>(type: "int", nullable: false),
                    totalExercisesCompleted = table.Column<int>(type: "int", nullable: false),
                    totalHours = table.Column<double>(type: "float", nullable: false),
                    workoutStreak = table.Column<int>(type: "int", nullable: false),
                    workoutCompletionRate = table.Column<double>(type: "float", nullable: false),
                    activeDays = table.Column<int>(type: "int", nullable: false),
                    totalSetsCompleted = table.Column<int>(type: "int", nullable: false),
                    totalRepsCompleted = table.Column<int>(type: "int", nullable: false),
                    totalWeightLifted = table.Column<double>(type: "float", nullable: false),
                    personalRecordsBroken = table.Column<int>(type: "int", nullable: false),
                    weeklyGoalAchieved = table.Column<bool>(type: "bit", nullable: false),
                    KcaloriesBurned = table.Column<double>(type: "float", nullable: false),
                    weekDate = table.Column<DateOnly>(type: "date", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    weekNumber = table.Column<int>(type: "int", nullable: false),
                    UserStatsMonthlyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatsWeekly", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStatsWeekly_UserStatsMonthly_UserStatsMonthlyId",
                        column: x => x.UserStatsMonthlyId,
                        principalTable: "UserStatsMonthly",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserStatsWeekly_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserStatsDaily",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    totalExercisesCompleted = table.Column<int>(type: "int", nullable: false),
                    totalSetsCompleted = table.Column<int>(type: "int", nullable: false),
                    totalHours = table.Column<double>(type: "float", nullable: false),
                    totalReps = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    dayOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    year = table.Column<int>(type: "int", nullable: false),
                    KcaloriesBurned = table.Column<double>(type: "float", nullable: false),
                    UserStatsWeeklyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatsDaily", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStatsDaily_UserStatsWeekly_UserStatsWeeklyId",
                        column: x => x.UserStatsWeeklyId,
                        principalTable: "UserStatsWeekly",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserStatsDaily_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserStatsDaily_userId_date",
                table: "UserStatsDaily",
                columns: new[] { "userId", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStatsDaily_UserStatsWeeklyId",
                table: "UserStatsDaily",
                column: "UserStatsWeeklyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatsMonthly_userId_monthName_year",
                table: "UserStatsMonthly",
                columns: new[] { "userId", "monthName", "year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStatsWeekly_userId_weekNumber_year",
                table: "UserStatsWeekly",
                columns: new[] { "userId", "weekNumber", "year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserStatsWeekly_UserStatsMonthlyId",
                table: "UserStatsWeekly",
                column: "UserStatsMonthlyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStatsDaily");

            migrationBuilder.DropTable(
                name: "UserStatsWeekly");

            migrationBuilder.DropTable(
                name: "UserStatsMonthly");

            migrationBuilder.DropColumn(
                name: "activeDays",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "personalRecordsBroken",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "totalRepsCompleted",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "totalSetsCompleted",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "totalWeightLifted",
                table: "UserStats");

            migrationBuilder.DropColumn(
                name: "weeklyGoalAchieved",
                table: "UserStats");
        }
    }
}
