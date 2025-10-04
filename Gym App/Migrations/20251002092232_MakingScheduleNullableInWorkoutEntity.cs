using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class MakingScheduleNullableInWorkoutEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PastInjuries_Users_UserID",
                table: "PastInjuries");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Schedules_ScheduleID",
                table: "Workouts");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Users_UserID",
                table: "Workouts");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserID",
                table: "Workouts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleID",
                table: "Workouts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserID",
                table: "PastInjuries",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_PastInjuries_Users_UserID",
                table: "PastInjuries",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Schedules_ScheduleID",
                table: "Workouts",
                column: "ScheduleID",
                principalTable: "Schedules",
                principalColumn: "ScheduleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Users_UserID",
                table: "Workouts",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PastInjuries_Users_UserID",
                table: "PastInjuries");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Schedules_ScheduleID",
                table: "Workouts");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Users_UserID",
                table: "Workouts");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserID",
                table: "Workouts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "ScheduleID",
                table: "Workouts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserID",
                table: "PastInjuries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PastInjuries_Users_UserID",
                table: "PastInjuries",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Schedules_ScheduleID",
                table: "Workouts",
                column: "ScheduleID",
                principalTable: "Schedules",
                principalColumn: "ScheduleID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Users_UserID",
                table: "Workouts",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }
    }
}
