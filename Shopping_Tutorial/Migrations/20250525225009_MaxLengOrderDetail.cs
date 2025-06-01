using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopping_Tutorial.Migrations
{
    /// <inheritdoc />
    public partial class MaxLengOrderDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
        name: "Name",
        table: "OrderDetails",
        type: "nvarchar(255)",
        maxLength: 255,
        nullable: false,
        oldClrType: typeof(string),
        oldType: "nvarchar(50)",
        oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
       name: "Name",
       table: "OrderDetails",
       type: "nvarchar(50)",
       maxLength: 50,
       nullable: false,
       oldClrType: typeof(string),
       oldType: "nvarchar(255)",
       oldMaxLength: 255);
        }
    }
}
