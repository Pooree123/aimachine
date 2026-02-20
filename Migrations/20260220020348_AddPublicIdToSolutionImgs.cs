using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aimachine.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicIdToSolutionImgs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ ทำแค่เพิ่มคอลัมน์ PublicId ลงในตาราง solution_img
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "solution_img",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ✅ เวลา Rollback กลับ (Down) ก็สั่งลบแค่คอลัมน์ PublicId ออก
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "solution_img");
        }
    }
}