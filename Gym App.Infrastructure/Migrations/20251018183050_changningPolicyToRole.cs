using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class changningPolicyToRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PolicyUser");

            migrationBuilder.DropTable(
                name: "Policy");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "varchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    PolicyRoleID = table.Column<int>(type: "int", nullable: false),
                    UsersUserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.PolicyRoleID, x.UsersUserID });
                    table.ForeignKey(
                        name: "FK_RoleUser_Role_PolicyRoleID",
                        column: x => x.PolicyRoleID,
                        principalTable: "Role",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersUserID",
                        column: x => x.UsersUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersUserID",
                table: "RoleUser",
                column: "UsersUserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.CreateTable(
                name: "Policy",
                columns: table => new
                {
                    PolicyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyName = table.Column<string>(type: "varchar(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policy", x => x.PolicyID);
                });

            migrationBuilder.CreateTable(
                name: "PolicyUser",
                columns: table => new
                {
                    PolicyID = table.Column<int>(type: "int", nullable: false),
                    UsersUserID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyUser", x => new { x.PolicyID, x.UsersUserID });
                    table.ForeignKey(
                        name: "FK_PolicyUser_Policy_PolicyID",
                        column: x => x.PolicyID,
                        principalTable: "Policy",
                        principalColumn: "PolicyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolicyUser_Users_UsersUserID",
                        column: x => x.UsersUserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PolicyUser_UsersUserID",
                table: "PolicyUser",
                column: "UsersUserID");
        }
    }
}
