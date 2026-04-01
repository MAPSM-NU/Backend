using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class changingIdName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengesUser_Challenges_ChallengesChallengeId",
                table: "ChallengesUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseMuscles_Exercises_ExercisesExerciseID",
                table: "ExerciseMuscles");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseMuscles_Muscles_MusclesID",
                table: "ExerciseMuscles");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseWorkout_Exercises_ExercisesExerciseID",
                table: "ExerciseWorkout");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseWorkout_Workouts_WorkoutsWorkoutID",
                table: "ExerciseWorkout");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Sessions_SessionID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_Id",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionUser_Sessions_SessionsSessionID",
                table: "SessionUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Schedules_ScheduleID",
                table: "Workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_Id",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "ScheduleID",
                table: "Workouts",
                newName: "ScheduleId");

            migrationBuilder.RenameColumn(
                name: "CreatAt",
                table: "Workouts",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "WorkoutID",
                table: "Workouts",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Workouts_ScheduleID",
                table: "Workouts",
                newName: "IX_Workouts_ScheduleId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Users",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SessionsSessionID",
                table: "SessionUser",
                newName: "SessionsId");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Sessions",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "SessionID",
                table: "Sessions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ScheduleID",
                table: "Schedules",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "RefreshTokenID",
                table: "RefreshTokens",
                newName: "UserID");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Notifications",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "NotificationID",
                table: "Notifications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MusclesID",
                table: "Muscles",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "SessionID",
                table: "Messages",
                newName: "SessionId");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Messages",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "MessageID",
                table: "Messages",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SessionID",
                table: "Messages",
                newName: "IX_Messages_SessionId");

            migrationBuilder.RenameColumn(
                name: "Date",
                table: "Feedbacks",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "FeedbackID",
                table: "Feedbacks",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "WorkoutsWorkoutID",
                table: "ExerciseWorkout",
                newName: "WorkoutsId");

            migrationBuilder.RenameColumn(
                name: "ExercisesExerciseID",
                table: "ExerciseWorkout",
                newName: "ExercisesId");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseWorkout_WorkoutsWorkoutID",
                table: "ExerciseWorkout",
                newName: "IX_ExerciseWorkout_WorkoutsId");

            migrationBuilder.RenameColumn(
                name: "ExerciseID",
                table: "Exercises",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MusclesID",
                table: "ExerciseMuscles",
                newName: "MusclesId");

            migrationBuilder.RenameColumn(
                name: "ExercisesExerciseID",
                table: "ExerciseMuscles",
                newName: "ExercisesId");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseMuscles_MusclesID",
                table: "ExerciseMuscles",
                newName: "IX_ExerciseMuscles_MusclesId");

            migrationBuilder.RenameColumn(
                name: "ChallengesChallengeId",
                table: "ChallengesUser",
                newName: "ChallengesId");

            migrationBuilder.RenameColumn(
                name: "ChallengeId",
                table: "Challenges",
                newName: "Id");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Workouts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Sessions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Schedules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Muscles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Muscles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Messages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Feedbacks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Exercises",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Exercises",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Challenges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Challenges",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserID",
                table: "RefreshTokens",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengesUser_Challenges_ChallengesId",
                table: "ChallengesUser",
                column: "ChallengesId",
                principalTable: "Challenges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseMuscles_Exercises_ExercisesId",
                table: "ExerciseMuscles",
                column: "ExercisesId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseMuscles_Muscles_MusclesId",
                table: "ExerciseMuscles",
                column: "MusclesId",
                principalTable: "Muscles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseWorkout_Exercises_ExercisesId",
                table: "ExerciseWorkout",
                column: "ExercisesId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseWorkout_Workouts_WorkoutsId",
                table: "ExerciseWorkout",
                column: "WorkoutsId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Sessions_SessionId",
                table: "Messages",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserID",
                table: "RefreshTokens",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionUser_Sessions_SessionsId",
                table: "SessionUser",
                column: "SessionsId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Schedules_ScheduleId",
                table: "Workouts",
                column: "ScheduleId",
                principalTable: "Schedules",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengesUser_Challenges_ChallengesId",
                table: "ChallengesUser");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseMuscles_Exercises_ExercisesId",
                table: "ExerciseMuscles");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseMuscles_Muscles_MusclesId",
                table: "ExerciseMuscles");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseWorkout_Exercises_ExercisesId",
                table: "ExerciseWorkout");

            migrationBuilder.DropForeignKey(
                name: "FK_ExerciseWorkout_Workouts_WorkoutsId",
                table: "ExerciseWorkout");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Sessions_SessionId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserID",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionUser_Sessions_SessionsId",
                table: "SessionUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Schedules_ScheduleId",
                table: "Workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserID",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Muscles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Muscles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Challenges");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "Workouts",
                newName: "ScheduleID");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Workouts",
                newName: "CreatAt");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Workouts",
                newName: "WorkoutID");

            migrationBuilder.RenameIndex(
                name: "IX_Workouts_ScheduleId",
                table: "Workouts",
                newName: "IX_Workouts_ScheduleID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "UserID");

            migrationBuilder.RenameColumn(
                name: "SessionsId",
                table: "SessionUser",
                newName: "SessionsSessionID");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Sessions",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Sessions",
                newName: "SessionID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Schedules",
                newName: "ScheduleID");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "RefreshTokens",
                newName: "RefreshTokenID");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Notifications",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Notifications",
                newName: "NotificationID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Muscles",
                newName: "MusclesID");

            migrationBuilder.RenameColumn(
                name: "SessionId",
                table: "Messages",
                newName: "SessionID");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Messages",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Messages",
                newName: "MessageID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SessionId",
                table: "Messages",
                newName: "IX_Messages_SessionID");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Feedbacks",
                newName: "Date");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Feedbacks",
                newName: "FeedbackID");

            migrationBuilder.RenameColumn(
                name: "WorkoutsId",
                table: "ExerciseWorkout",
                newName: "WorkoutsWorkoutID");

            migrationBuilder.RenameColumn(
                name: "ExercisesId",
                table: "ExerciseWorkout",
                newName: "ExercisesExerciseID");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseWorkout_WorkoutsId",
                table: "ExerciseWorkout",
                newName: "IX_ExerciseWorkout_WorkoutsWorkoutID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Exercises",
                newName: "ExerciseID");

            migrationBuilder.RenameColumn(
                name: "MusclesId",
                table: "ExerciseMuscles",
                newName: "MusclesID");

            migrationBuilder.RenameColumn(
                name: "ExercisesId",
                table: "ExerciseMuscles",
                newName: "ExercisesExerciseID");

            migrationBuilder.RenameIndex(
                name: "IX_ExerciseMuscles_MusclesId",
                table: "ExerciseMuscles",
                newName: "IX_ExerciseMuscles_MusclesID");

            migrationBuilder.RenameColumn(
                name: "ChallengesId",
                table: "ChallengesUser",
                newName: "ChallengesChallengeId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Challenges",
                newName: "ChallengeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "RefreshTokenID");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Id",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengesUser_Challenges_ChallengesChallengeId",
                table: "ChallengesUser",
                column: "ChallengesChallengeId",
                principalTable: "Challenges",
                principalColumn: "ChallengeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseMuscles_Exercises_ExercisesExerciseID",
                table: "ExerciseMuscles",
                column: "ExercisesExerciseID",
                principalTable: "Exercises",
                principalColumn: "ExerciseID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseMuscles_Muscles_MusclesID",
                table: "ExerciseMuscles",
                column: "MusclesID",
                principalTable: "Muscles",
                principalColumn: "MusclesID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseWorkout_Exercises_ExercisesExerciseID",
                table: "ExerciseWorkout",
                column: "ExercisesExerciseID",
                principalTable: "Exercises",
                principalColumn: "ExerciseID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExerciseWorkout_Workouts_WorkoutsWorkoutID",
                table: "ExerciseWorkout",
                column: "WorkoutsWorkoutID",
                principalTable: "Workouts",
                principalColumn: "WorkoutID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Sessions_SessionID",
                table: "Messages",
                column: "SessionID",
                principalTable: "Sessions",
                principalColumn: "SessionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_Id",
                table: "RefreshTokens",
                column: "Id",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionUser_Sessions_SessionsSessionID",
                table: "SessionUser",
                column: "SessionsSessionID",
                principalTable: "Sessions",
                principalColumn: "SessionID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Schedules_ScheduleID",
                table: "Workouts",
                column: "ScheduleID",
                principalTable: "Schedules",
                principalColumn: "ScheduleID");
        }
    }
}
