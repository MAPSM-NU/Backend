using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class check : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengesUser_Users_ParticipantsUserID",
                table: "ChallengesUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_UserID",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_LiveFeedbacks_Users_UserID",
                table: "LiveFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderUserID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PastInjuries_Users_UserID",
                table: "PastInjuries");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserID",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_UserID",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionUser_Users_UsersUserID",
                table: "SessionUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserID",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Users_UserID",
                table: "Workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Workouts",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Workouts_UserID",
                table: "Workouts",
                newName: "IX_Workouts_UserId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Transactions",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserID",
                table: "Transactions",
                newName: "IX_Transactions_UserId");

            migrationBuilder.RenameColumn(
                name: "UsersUserID",
                table: "SessionUser",
                newName: "UsersId");

            migrationBuilder.RenameIndex(
                name: "IX_SessionUser_UsersUserID",
                table: "SessionUser",
                newName: "IX_SessionUser_UsersId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Schedules",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_UserID",
                table: "Schedules",
                newName: "IX_Schedules_UserId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "PastInjuries",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PastInjuries_UserID",
                table: "PastInjuries",
                newName: "IX_PastInjuries_UserId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Notifications",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserID",
                table: "Notifications",
                newName: "IX_Notifications_UserId");

            migrationBuilder.RenameColumn(
                name: "SenderUserID",
                table: "Messages",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SenderUserID",
                table: "Messages",
                newName: "IX_Messages_SenderId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "LiveFeedbacks",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_LiveFeedbacks_UserID",
                table: "LiveFeedbacks",
                newName: "IX_LiveFeedbacks_UserId");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Feedbacks",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_UserID",
                table: "Feedbacks",
                newName: "IX_Feedbacks_UserId");

            migrationBuilder.RenameColumn(
                name: "ParticipantsUserID",
                table: "ChallengesUser",
                newName: "ParticipantsId");

            migrationBuilder.RenameIndex(
                name: "IX_ChallengesUser_ParticipantsUserID",
                table: "ChallengesUser",
                newName: "IX_ChallengesUser_ParticipantsId");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengesUser_Users_ParticipantsId",
                table: "ChallengesUser",
                column: "ParticipantsId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LiveFeedbacks_Users_UserId",
                table: "LiveFeedbacks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PastInjuries_Users_UserId",
                table: "PastInjuries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserID",
                table: "RefreshTokens",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_UserId",
                table: "Schedules",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionUser_Users_UsersId",
                table: "SessionUser",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Users_UserId",
                table: "Workouts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChallengesUser_Users_ParticipantsId",
                table: "ChallengesUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Users_UserId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_LiveFeedbacks_Users_UserId",
                table: "LiveFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Users_SenderId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_PastInjuries_Users_UserId",
                table: "PastInjuries");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserID",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Users_UserId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionUser_Users_UsersId",
                table: "SessionUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Workouts_Users_UserId",
                table: "Workouts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Workouts",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Workouts_UserId",
                table: "Workouts",
                newName: "IX_Workouts_UserID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Transactions",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                newName: "IX_Transactions_UserID");

            migrationBuilder.RenameColumn(
                name: "UsersId",
                table: "SessionUser",
                newName: "UsersUserID");

            migrationBuilder.RenameIndex(
                name: "IX_SessionUser_UsersId",
                table: "SessionUser",
                newName: "IX_SessionUser_UsersUserID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Schedules",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_UserId",
                table: "Schedules",
                newName: "IX_Schedules_UserID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PastInjuries",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_PastInjuries_UserId",
                table: "PastInjuries",
                newName: "IX_PastInjuries_UserID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Notifications",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                newName: "IX_Notifications_UserID");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Messages",
                newName: "SenderUserID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                newName: "IX_Messages_SenderUserID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "LiveFeedbacks",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_LiveFeedbacks_UserId",
                table: "LiveFeedbacks",
                newName: "IX_LiveFeedbacks_UserID");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Feedbacks",
                newName: "UserID");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_UserID");

            migrationBuilder.RenameColumn(
                name: "ParticipantsId",
                table: "ChallengesUser",
                newName: "ParticipantsUserID");

            migrationBuilder.RenameIndex(
                name: "IX_ChallengesUser_ParticipantsId",
                table: "ChallengesUser",
                newName: "IX_ChallengesUser_ParticipantsUserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_ChallengesUser_Users_ParticipantsUserID",
                table: "ChallengesUser",
                column: "ParticipantsUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Users_UserID",
                table: "Feedbacks",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LiveFeedbacks_Users_UserID",
                table: "LiveFeedbacks",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Users_SenderUserID",
                table: "Messages",
                column: "SenderUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserID",
                table: "Notifications",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PastInjuries_Users_UserID",
                table: "PastInjuries",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserID",
                table: "RefreshTokens",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Users_UserID",
                table: "Schedules",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionUser_Users_UsersUserID",
                table: "SessionUser",
                column: "UsersUserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserID",
                table: "Transactions",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Workouts_Users_UserID",
                table: "Workouts",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
