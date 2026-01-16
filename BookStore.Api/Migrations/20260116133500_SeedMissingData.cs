using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BookStore.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedMissingData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "Id", "FirstName", "LastName" },
                values: new object[] { 3, "Marguerite", "Duras" });

            migrationBuilder.InsertData(
                table: "Illustrators",
                columns: new[] { "Id", "FirstName", "LastName" },
                values: new object[,]
                {
                    { 2, "Norman", "Rockwell" },
                    { 3, "Beya", "Rebaï" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Authors",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Illustrators",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Illustrators",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
