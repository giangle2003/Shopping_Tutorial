using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class RatingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
        name: "Ratings",
        columns: table => new
        {
            Id = table.Column<int>(type: "int").Annotation("SqlServer:Identity", "1, 1"),
            ProductId = table.Column<long>(type: "bigint", nullable: false),
            Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Star = table.Column<string>(type: "nvarchar(max)", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Ratings", x => x.Id);
            table.ForeignKey(
                name: "FK_Ratings_Products_ProductId",
                column: x => x.ProductId,
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ratings"
            );
        }
    }
}
