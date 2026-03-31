using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NewPropertiesInOutboxMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "OutboxMessages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "PublishedAt",
                table: "OutboxMessages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Published_CreatedAt",
                table: "OutboxMessages",
                columns: new[] { "Published", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Published_CreatedAt",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "PublishedAt",
                table: "OutboxMessages");
        }
    }
}
