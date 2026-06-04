using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fasally.Migrations
{
    /// <inheritdoc />
    public partial class RenameCategoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_TailorCategories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_TailorCategoryMappings_TailorCategories_CategoriesId",
                table: "TailorCategoryMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_TailorCategoryMappings_Tailors_TailorsApplicationUserId",
                table: "TailorCategoryMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TailorCategoryMappings",
                table: "TailorCategoryMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TailorCategories",
                table: "TailorCategories");

            migrationBuilder.RenameTable(
                name: "TailorCategories",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "TailorCategoryMappings",
                newName: "CategoryTailorMappings");

            migrationBuilder.RenameIndex(
                name: "IX_TailorCategories_Name",
                table: "Categories",
                newName: "IX_Categories_Name");

            migrationBuilder.RenameIndex(
                name: "IX_TailorCategoryMappings_TailorsApplicationUserId",
                table: "CategoryTailorMappings",
                newName: "IX_CategoryTailorMappings_TailorsApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryTailorMappings",
                table: "CategoryTailorMappings",
                columns: new[] { "CategoriesId", "TailorsApplicationUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryTailorMappings_Categories_CategoriesId",
                table: "CategoryTailorMappings",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryTailorMappings_Tailors_TailorsApplicationUserId",
                table: "CategoryTailorMappings",
                column: "TailorsApplicationUserId",
                principalTable: "Tailors",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryTailorMappings_Categories_CategoriesId",
                table: "CategoryTailorMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryTailorMappings_Tailors_TailorsApplicationUserId",
                table: "CategoryTailorMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryTailorMappings",
                table: "CategoryTailorMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "TailorCategories");

            migrationBuilder.RenameTable(
                name: "CategoryTailorMappings",
                newName: "TailorCategoryMappings");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_Name",
                table: "TailorCategories",
                newName: "IX_TailorCategories_Name");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryTailorMappings_TailorsApplicationUserId",
                table: "TailorCategoryMappings",
                newName: "IX_TailorCategoryMappings_TailorsApplicationUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TailorCategories",
                table: "TailorCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TailorCategoryMappings",
                table: "TailorCategoryMappings",
                columns: new[] { "CategoriesId", "TailorsApplicationUserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TailorCategoryMappings_TailorCategories_CategoriesId",
                table: "TailorCategoryMappings",
                column: "CategoriesId",
                principalTable: "TailorCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TailorCategoryMappings_Tailors_TailorsApplicationUserId",
                table: "TailorCategoryMappings",
                column: "TailorsApplicationUserId",
                principalTable: "Tailors",
                principalColumn: "ApplicationUserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_TailorCategories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "TailorCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
