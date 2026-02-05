using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SadStore.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CityName",
                table: "ShippingLocations",
                newName: "CityNameEn");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "BlogPosts",
                newName: "TitleEn");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "BlogPosts",
                newName: "TitleAr");

            migrationBuilder.AddColumn<string>(
                name: "CityNameAr",
                table: "ShippingLocations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ContentAr",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentEn",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityNameAr",
                table: "ShippingLocations");

            migrationBuilder.DropColumn(
                name: "ContentAr",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "ContentEn",
                table: "BlogPosts");

            migrationBuilder.RenameColumn(
                name: "CityNameEn",
                table: "ShippingLocations",
                newName: "CityName");

            migrationBuilder.RenameColumn(
                name: "TitleEn",
                table: "BlogPosts",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "TitleAr",
                table: "BlogPosts",
                newName: "Content");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "BlogPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
