using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class hello : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleUser_Role_PolicyRoleID",
                table: "RoleUser");

            migrationBuilder.RenameColumn(
                name: "PolicyRoleID",
                table: "RoleUser",
                newName: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleUser_Role_RoleID",
                table: "RoleUser",
                column: "RoleID",
                principalTable: "Role",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleUser_Role_RoleID",
                table: "RoleUser");

            migrationBuilder.RenameColumn(
                name: "RoleID",
                table: "RoleUser",
                newName: "PolicyRoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleUser_Role_PolicyRoleID",
                table: "RoleUser",
                column: "PolicyRoleID",
                principalTable: "Role",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
