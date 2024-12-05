using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chirp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_AuthorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Cheeps_AspNetUsers_AuthorId",
                table: "Cheeps");

            migrationBuilder.DropTable(
                name: "AuthorAuthor");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AuthorId",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "AuthorFollows",
                columns: table => new
                {
                    FollowedId = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowerId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorFollows", x => new { x.FollowedId, x.FollowerId });
                    table.ForeignKey(
                        name: "FK_AuthorFollows_AspNetUsers_FollowedId",
                        column: x => x.FollowedId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuthorFollows_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorFollows_FollowerId",
                table: "AuthorFollows",
                column: "FollowerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cheeps_AspNetUsers_AuthorId",
                table: "Cheeps",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cheeps_AspNetUsers_AuthorId",
                table: "Cheeps");

            migrationBuilder.DropTable(
                name: "AuthorFollows");

            migrationBuilder.CreateTable(
                name: "AuthorAuthor",
                columns: table => new
                {
                    FollowedAuthorsId = table.Column<string>(type: "TEXT", nullable: false),
                    FollowersId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorAuthor", x => new { x.FollowedAuthorsId, x.FollowersId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AuthorId",
                table: "AspNetUsers",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorAuthor_FollowersId",
                table: "AuthorAuthor",
                column: "FollowersId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_AuthorId",
                table: "AspNetUsers",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cheeps_AspNetUsers_AuthorId",
                table: "Cheeps",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
