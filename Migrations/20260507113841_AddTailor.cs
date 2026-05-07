using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fasally.Migrations
{
    /// <inheritdoc />
    public partial class AddTailor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsProfileCompleted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PendingProfileType",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TailorCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TailorCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tailors",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NationalIdImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ShopImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ResponseRate = table.Column<double>(type: "float", nullable: false),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tailors", x => x.ApplicationUserId);
                    table.ForeignKey(
                        name: "FK_Tailors_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PortfolioItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImageUrls = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TailorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortfolioItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortfolioItems_Tailors_TailorId",
                        column: x => x.TailorId,
                        principalTable: "Tailors",
                        principalColumn: "ApplicationUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TailorCategoryMappings",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "int", nullable: false),
                    TailorsApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TailorCategoryMappings", x => new { x.CategoriesId, x.TailorsApplicationUserId });
                    table.ForeignKey(
                        name: "FK_TailorCategoryMappings_TailorCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "TailorCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TailorCategoryMappings_Tailors_TailorsApplicationUserId",
                        column: x => x.TailorsApplicationUserId,
                        principalTable: "Tailors",
                        principalColumn: "ApplicationUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioItems_TailorId",
                table: "PortfolioItems",
                column: "TailorId");

            migrationBuilder.CreateIndex(
                name: "IX_TailorCategories_Name",
                table: "TailorCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TailorCategoryMappings_TailorsApplicationUserId",
                table: "TailorCategoryMappings",
                column: "TailorsApplicationUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortfolioItems");

            migrationBuilder.DropTable(
                name: "TailorCategoryMappings");

            migrationBuilder.DropTable(
                name: "TailorCategories");

            migrationBuilder.DropTable(
                name: "Tailors");

            migrationBuilder.DropColumn(
                name: "IsProfileCompleted",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PendingProfileType",
                table: "AspNetUsers");
        }
    }
}
