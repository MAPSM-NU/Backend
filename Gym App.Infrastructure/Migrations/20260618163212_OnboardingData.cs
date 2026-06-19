using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Gym_App.Migrations
{
    /// <inheritdoc />
    public partial class OnboardingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExerciseRestrictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseRestrictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FitnessGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FitnessGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Injuries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Injuries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(30)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalConditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseRestrictionsUser",
                columns: table => new
                {
                    ExerciseRestrictionsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseRestrictionsUser", x => new { x.ExerciseRestrictionsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_ExerciseRestrictionsUser_ExerciseRestrictions_ExerciseRestrictionsId",
                        column: x => x.ExerciseRestrictionsId,
                        principalTable: "ExerciseRestrictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseRestrictionsUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FitnessGoalsUser",
                columns: table => new
                {
                    FitnessGoalsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FitnessGoalsUser", x => new { x.FitnessGoalsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_FitnessGoalsUser_FitnessGoals_FitnessGoalsId",
                        column: x => x.FitnessGoalsId,
                        principalTable: "FitnessGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FitnessGoalsUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InjuryUser",
                columns: table => new
                {
                    InjuriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InjuryUser", x => new { x.InjuriesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_InjuryUser_Injuries_InjuriesId",
                        column: x => x.InjuriesId,
                        principalTable: "Injuries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InjuryUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicalConditionUser",
                columns: table => new
                {
                    MedicalConditionsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsersId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalConditionUser", x => new { x.MedicalConditionsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_MedicalConditionUser_MedicalConditions_MedicalConditionsId",
                        column: x => x.MedicalConditionsId,
                        principalTable: "MedicalConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalConditionUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ExerciseRestrictions",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1d7a4c9e-2b58-4f6a-8c71-9e3d5b7a1c11"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NoHeavyLifting", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("2e8b5d1f-3c69-4a7b-9d82-1f4e6c8b2d22"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "NoRunning", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("3f9c6e2a-4d7a-4b8c-a193-2a5f7d9c3e33"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "LowerBodyOnly", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("4a1d7f3b-5e8b-4c9d-b2a4-3b6a8e1d4f44"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "LowImpactOnly", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("5b2e8a4c-6f9c-4d1e-c3b5-4c7b9f2e5a55"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "UpperBodyOnly", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "FitnessGoals",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2e8a1f93-6c47-4d7f-9c21-4b5e7d3f8a22"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "GeneralFitness", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("4c7e2a11-9f85-4d3b-a1e7-5d8c6b2f4c44"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "WeightLoss", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("5a9c1e7f-3d82-4b6e-b4f8-2d7c9a6e1f66"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Flexibility", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("7b1f4c4d-5c0e-4f6d-8d8d-9f4c6b8a2e11"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Endurance", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("8f2d7c3a-1b64-4e9d-9a72-3c5e8f1b5d55"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "MuscleGain", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("91d3b6e7-8a54-4f12-b8c4-7e9f2d1a3b33"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "StrengthTraining", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Injuries",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a1f3c7d9-5b42-4e8a-9d71-2c6f8b3e7a11"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Back", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("b2e4d8f1-6c53-4f9b-a182-3d7a9c4f8b22"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Knee", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("c3f5e9a2-7d64-4a1c-b293-4e8b1d5a9c33"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Shoulder", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("d4a6f1b3-8e75-4b2d-c3a4-5f9c2e6b1d44"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ankle", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("e5b7a2c4-9f86-4c3e-d4b5-6a1d3f7c2e55"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Wrist", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("f6c8b3d5-1a97-4d4f-e5c6-7b2e4a8d3f66"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hip", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "MedicalConditions",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("6e3f2a91-4d8b-47c6-a2e1-9f5d7b3c8a77"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HeartCondition", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("7f4a3b82-5e9c-48d7-b3f2-a6e8c4d9b188"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "HighBloodPressure", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("8a5b4c73-6f1d-49e8-c4a3-b7f9d5e1c299"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Arthritis", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("9b6c5d64-7a2e-4af9-d5b4-c8a1e6f2d3aa"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Asthma", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { new Guid("ac7d8e91-3f42-4b6a-9c15-e7d2f8a4b3c1"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diabetes", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseRestrictionsUser_UsersId",
                table: "ExerciseRestrictionsUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_FitnessGoalsUser_UsersId",
                table: "FitnessGoalsUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_InjuryUser_UsersId",
                table: "InjuryUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicalConditionUser_UsersId",
                table: "MedicalConditionUser",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseRestrictionsUser");

            migrationBuilder.DropTable(
                name: "FitnessGoalsUser");

            migrationBuilder.DropTable(
                name: "InjuryUser");

            migrationBuilder.DropTable(
                name: "MedicalConditionUser");

            migrationBuilder.DropTable(
                name: "ExerciseRestrictions");

            migrationBuilder.DropTable(
                name: "FitnessGoals");

            migrationBuilder.DropTable(
                name: "Injuries");

            migrationBuilder.DropTable(
                name: "MedicalConditions");
        }
    }
}
