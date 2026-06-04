using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fasally.Migrations
{
    /// <inheritdoc />
    public partial class AddProposalAcceptedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "Proposals",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "Proposals");
        }
    }
}
