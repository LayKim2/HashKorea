using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HashKorea.Migrations
{
    /// <inheritdoc />
    public partial class update_UserPost_addType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "UserPosts",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "UserPosts");
        }
    }
}
