using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddStargateLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StargateLogSeverity",
                columns: table => new
                {
                    StargateLogSeverityId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StargateLogSeverity", x => x.StargateLogSeverityId);
                });

            migrationBuilder.CreateTable(
                name: "StargateLog",
                columns: table => new
                {
                    StargateLogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StargateLogSeverityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: false),
                    Exception = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StargateLog", x => x.StargateLogId);
                    table.ForeignKey(
                        name: "FK_StargateLog_StargateLogSeverity_StargateLogSeverityId",
                        column: x => x.StargateLogSeverityId,
                        principalTable: "StargateLogSeverity",
                        principalColumn: "StargateLogSeverityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "StargateLogSeverity",
                columns: new[] { "StargateLogSeverityId", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Informational message", "Info" },
                    { 2, "Warning message", "Warning" },
                    { 3, "Error message", "Error" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Person_Name",
                table: "Person",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StargateLog_CreatedOn",
                table: "StargateLog",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_StargateLog_StargateLogSeverityId",
                table: "StargateLog",
                column: "StargateLogSeverityId");

            migrationBuilder.CreateIndex(
                name: "IX_StargateLogSeverity_Name",
                table: "StargateLogSeverity",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StargateLog");

            migrationBuilder.DropTable(
                name: "StargateLogSeverity");

            migrationBuilder.DropIndex(
                name: "IX_Person_Name",
                table: "Person");
        }
    }
}
