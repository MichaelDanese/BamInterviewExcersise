using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StargateAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCurrentDutyAndRank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AstronautDuty_PersonId",
                table: "AstronautDuty");

            migrationBuilder.DropColumn(
                name: "CurrentDutyTitle",
                table: "AstronautDetail");

            migrationBuilder.DropColumn(
                name: "CurrentRank",
                table: "AstronautDetail");

            migrationBuilder.CreateIndex(
                name: "IX_AstronautDuty_PersonId_DutyStartDate_DutyEndDate",
                table: "AstronautDuty",
                columns: new[] { "PersonId", "DutyStartDate", "DutyEndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AstronautDuty_PersonId_DutyStartDate_DutyEndDate",
                table: "AstronautDuty");

            migrationBuilder.AddColumn<string>(
                name: "CurrentDutyTitle",
                table: "AstronautDetail",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CurrentRank",
                table: "AstronautDetail",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AstronautDuty_PersonId",
                table: "AstronautDuty",
                column: "PersonId");
        }
    }
}
