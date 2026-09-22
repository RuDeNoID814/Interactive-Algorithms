using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoLab.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRunData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAverage",
                table: "ExperimentPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Run",
                table: "ExperimentPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAverage",
                table: "ExperimentPoints");

            migrationBuilder.DropColumn(
                name: "Run",
                table: "ExperimentPoints");
        }
    }
}
