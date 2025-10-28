using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonTCG.API.Migrations
{
    /// <inheritdoc />
    public partial class DeckCardFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFoil",
                table: "DeckCards");

            migrationBuilder.DropColumn(
                name: "SlotType",
                table: "DeckCards");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFoil",
                table: "DeckCards",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SlotType",
                table: "DeckCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
