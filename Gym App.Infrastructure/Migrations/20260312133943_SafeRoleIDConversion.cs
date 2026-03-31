using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class SafeRoleIDConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop the foreign key constraint first
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleID",
                table: "Users");

            // Step 2: Drop the index on Users.RoleID
            migrationBuilder.DropIndex(
                name: "IX_Users_RoleID",
                table: "Users");

            // Step 3: Drop the old primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            // Step 4: Add Id column to Roles
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: Guid.NewGuid());

            // Step 5: Add CreatedAt and UpdatedAt to Roles
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Roles",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Roles",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            // Step 6: Add new primary key based on Id
            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "Id");

            // Step 7: Add temporary RoleID_New column to Users
            migrationBuilder.AddColumn<Guid>(
                name: "RoleID_New",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            // Step 8: Map old RoleID values to new Guid RoleID values
            migrationBuilder.Sql(@"
                UPDATE Users
                SET RoleID_New = r.Id
                FROM Users u
                INNER JOIN Roles r ON u.RoleID = r.RoleID
                WHERE u.RoleID IS NOT NULL;
            ");

            // Step 9: Drop old RoleID column from Users
            migrationBuilder.DropColumn(
                name: "RoleID",
                table: "Users");

            // Step 10: Rename new column to RoleID
            migrationBuilder.RenameColumn(
                name: "RoleID_New",
                table: "Users",
                newName: "RoleID");

            // Step 11: Make RoleID NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "RoleID",
                table: "Users",
                type: "uniqueidentifier",
                nullable: false);

            // Step 12: Drop old RoleID column from Roles
            migrationBuilder.DropColumn(
                name: "RoleID",
                table: "Roles");

            // Step 13: Re-add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleID",
                table: "Users",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            // Step 14: Create index
            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");

            // Step 15: Add CreatedAt and UpdatedAt to Transactions
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValue: DateTime.UtcNow);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop foreign key first
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleID",
                table: "Users");

            // Step 2: Drop index
            migrationBuilder.DropIndex(
                name: "IX_Users_RoleID",
                table: "Users");

            // Step 3: Drop columns from Transactions
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Transactions");

            // Step 4: Rename RoleID back to RoleID_New temporarily
            migrationBuilder.RenameColumn(
                name: "RoleID",
                table: "Users",
                newName: "RoleID_New");

            // Step 5: Drop the new primary key from Roles
            migrationBuilder.DropPrimaryKey(
                name: "PK_Roles",
                table: "Roles");

            // Step 6: Recreate old RoleID column in Roles with Identity
            migrationBuilder.AddColumn<int>(
                name: "RoleID",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // Step 7: Re-add old primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Roles",
                table: "Roles",
                column: "RoleID");

            // Step 8: Recreate old RoleID column in Users
            migrationBuilder.AddColumn<int>(
                name: "RoleID",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 2);

            // Step 9: Restore mapping
            migrationBuilder.Sql(@"
                UPDATE Users
                SET RoleID = (SELECT RoleID FROM Roles WHERE Id = RoleID_New)
            ");

            // Step 10: Drop temporary column
            migrationBuilder.DropColumn(
                name: "RoleID_New",
                table: "Users");

            // Step 11: Drop new columns from Roles
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Roles");

            // Step 12: Re-add old foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleID",
                table: "Users",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "RoleID");

            // Step 13: Re-create index
            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");
        }
    }
}