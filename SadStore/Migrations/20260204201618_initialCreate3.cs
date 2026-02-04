using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SadStore.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailableSizes",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CareInstructionsAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CareInstructionsEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CutTypeAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CutTypeEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesignTypeAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DesignTypeEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FitTypeAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FitTypeEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiningAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LiningEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelNumber",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccasionAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OccasionEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OldPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SleevesAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SleevesEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StretchAr",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StretchEn",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvailableSizes",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CareInstructionsAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CareInstructionsEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CutTypeAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CutTypeEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DesignTypeAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DesignTypeEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FitTypeAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "FitTypeEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LiningAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "LiningEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaterialAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MaterialEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ModelNumber",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OccasionAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OccasionEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OldPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SleevesAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SleevesEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StretchAr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "StretchEn",
                table: "Products");
        }
    }
}
