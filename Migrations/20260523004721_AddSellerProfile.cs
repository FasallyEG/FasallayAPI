using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fasally.Migrations
{
    /// <inheritdoc />
    public partial class AddSellerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SellerProfiles",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BusinessPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BusinessEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ShopImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    AverageRating = table.Column<double>(type: "float", nullable: false),
                    TotalReviews = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerProfiles", x => x.ApplicationUserId);
                    table.ForeignKey(
                        name: "FK_SellerProfiles_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SellerProfiles");
        }
    }
}
