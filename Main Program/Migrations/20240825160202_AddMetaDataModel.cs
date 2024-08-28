using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TvMazeScrapper.Migrations
{
    /// <inheritdoc />
    public partial class AddMetaDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CastPersons_Shows_ShowId",
                table: "CastPersons");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Shows",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Shows",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ShowId",
                table: "CastPersons",
                newName: "Showid");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CastPersons",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Birthday",
                table: "CastPersons",
                newName: "birthday");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "CastPersons",
                newName: "id");

            migrationBuilder.RenameIndex(
                name: "IX_CastPersons_ShowId",
                table: "CastPersons",
                newName: "IX_CastPersons_Showid");

            migrationBuilder.CreateTable(
                name: "ShowMetaData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReveivedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowMetaData", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CastPersons_Shows_Showid",
                table: "CastPersons",
                column: "Showid",
                principalTable: "Shows",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CastPersons_Shows_Showid",
                table: "CastPersons");

            migrationBuilder.DropTable(
                name: "ShowMetaData");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Shows",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Shows",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "CastPersons",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "birthday",
                table: "CastPersons",
                newName: "Birthday");

            migrationBuilder.RenameColumn(
                name: "Showid",
                table: "CastPersons",
                newName: "ShowId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "CastPersons",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_CastPersons_Showid",
                table: "CastPersons",
                newName: "IX_CastPersons_ShowId");

            migrationBuilder.AddForeignKey(
                name: "FK_CastPersons_Shows_ShowId",
                table: "CastPersons",
                column: "ShowId",
                principalTable: "Shows",
                principalColumn: "Id");
        }
    }
}
