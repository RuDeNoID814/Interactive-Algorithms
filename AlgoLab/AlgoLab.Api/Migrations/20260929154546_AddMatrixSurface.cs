using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoLab.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMatrixSurface : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Is3D",
                table: "ExperimentPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "M",
                table: "ExperimentPoints",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Is3D",
                table: "ExperimentPoints");

            migrationBuilder.DropColumn(
                name: "M",
                table: "ExperimentPoints");
        }
    }
}
