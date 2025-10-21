using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokemonTCG.API.Migrations
{
    /// <inheritdoc />
    public partial class DeckTableAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CardImages",
                columns: table => new
                {
                    CardImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Small = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Large = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardImages", x => x.CardImageId);
                });

            migrationBuilder.CreateTable(
                name: "Decks",
                columns: table => new
                {
                    DeckId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decks", x => x.DeckId);
                });

            migrationBuilder.CreateTable(
                name: "Legalities",
                columns: table => new
                {
                    LegalityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Unlimited = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Standard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Expanded = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Legalities", x => x.LegalityId);
                });

            migrationBuilder.CreateTable(
                name: "SetImages",
                columns: table => new
                {
                    SetImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetImages", x => x.SetImageId);
                });

            migrationBuilder.CreateTable(
                name: "Sets",
                columns: table => new
                {
                    SetId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Series = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrintedTotal = table.Column<long>(type: "bigint", nullable: true),
                    Total = table.Column<long>(type: "bigint", nullable: true),
                    LegalitiesId = table.Column<int>(type: "int", nullable: true),
                    PtcgoCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleaseDate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagesId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sets", x => x.SetId);
                    table.ForeignKey(
                        name: "FK_Sets_Legalities_LegalitiesId",
                        column: x => x.LegalitiesId,
                        principalTable: "Legalities",
                        principalColumn: "LegalityId");
                    table.ForeignKey(
                        name: "FK_Sets_SetImages_ImagesId",
                        column: x => x.ImagesId,
                        principalTable: "SetImages",
                        principalColumn: "SetImageId");
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SuperType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubTypes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Hp = table.Column<int>(type: "int", nullable: true),
                    Types = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EvolvesFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetreatCost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConvertedRetreatCost = table.Column<int>(type: "int", nullable: true),
                    SetId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeckId = table.Column<int>(type: "int", nullable: true),
                    Number = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Artist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rarity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalPokedexNumbers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LegalitiesId = table.Column<int>(type: "int", nullable: true),
                    CardImageId = table.Column<int>(type: "int", nullable: true),
                    EvolvesTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlavorText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rules = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegulationMark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.CardId);
                    table.ForeignKey(
                        name: "FK_Cards_CardImages_CardImageId",
                        column: x => x.CardImageId,
                        principalTable: "CardImages",
                        principalColumn: "CardImageId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cards_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "DeckId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cards_Legalities_LegalitiesId",
                        column: x => x.LegalitiesId,
                        principalTable: "Legalities",
                        principalColumn: "LegalityId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cards_Sets_SetId",
                        column: x => x.SetId,
                        principalTable: "Sets",
                        principalColumn: "SetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Abilities",
                columns: table => new
                {
                    AbilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abilities", x => x.AbilityId);
                    table.ForeignKey(
                        name: "FK_Abilities_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AncientTraits",
                columns: table => new
                {
                    AncientTraitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AncientTraits", x => x.AncientTraitId);
                    table.ForeignKey(
                        name: "FK_AncientTraits_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "CardId");
                });

            migrationBuilder.CreateTable(
                name: "Attacks",
                columns: table => new
                {
                    AttackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Damage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConvertedEnergyCost = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CostJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attacks", x => x.AttackId);
                    table.ForeignKey(
                        name: "FK_Attacks_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardMarkets",
                columns: table => new
                {
                    CardMarketId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardMarkets", x => x.CardMarketId);
                    table.ForeignKey(
                        name: "FK_CardMarkets_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Resistances",
                columns: table => new
                {
                    ResistanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resistances", x => x.ResistanceId);
                    table.ForeignKey(
                        name: "FK_Resistances_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCGPlayers",
                columns: table => new
                {
                    TCGPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCGPlayers", x => x.TCGPlayerId);
                    table.ForeignKey(
                        name: "FK_TCGPlayers_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Weaknesses",
                columns: table => new
                {
                    WeaknessId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weaknesses", x => x.WeaknessId);
                    table.ForeignKey(
                        name: "FK_Weaknesses_Cards_CardId",
                        column: x => x.CardId,
                        principalTable: "Cards",
                        principalColumn: "CardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CardMarketPrices",
                columns: table => new
                {
                    CardMarketId = table.Column<int>(type: "int", nullable: false),
                    CardMarketPriceId = table.Column<int>(type: "int", nullable: false),
                    AverageSellPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LowPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TrendPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReverseHoloLow = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReverseHoloTrend = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LowPriceExPlus = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageDay = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageWeek = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageDayReverseHolo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageWeekReverseHolo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    AverageMonthReverseHolo = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardMarketPrices", x => x.CardMarketId);
                    table.ForeignKey(
                        name: "FK_CardMarketPrices_CardMarkets_CardMarketId",
                        column: x => x.CardMarketId,
                        principalTable: "CardMarkets",
                        principalColumn: "CardMarketId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TCGPlayerPrices",
                columns: table => new
                {
                    TCGPlayerPriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCGPlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TCGPlayerPrices", x => x.TCGPlayerPriceId);
                    table.ForeignKey(
                        name: "FK_TCGPlayerPrices_TCGPlayers_TCGPlayerId",
                        column: x => x.TCGPlayerId,
                        principalTable: "TCGPlayers",
                        principalColumn: "TCGPlayerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prices",
                columns: table => new
                {
                    PriceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TCGPlayerPriceId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Low = table.Column<double>(type: "float", nullable: true),
                    Mid = table.Column<double>(type: "float", nullable: true),
                    High = table.Column<double>(type: "float", nullable: true),
                    Market = table.Column<double>(type: "float", nullable: true),
                    DirectLow = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prices", x => x.PriceId);
                    table.ForeignKey(
                        name: "FK_Prices_TCGPlayerPrices_TCGPlayerPriceId",
                        column: x => x.TCGPlayerPriceId,
                        principalTable: "TCGPlayerPrices",
                        principalColumn: "TCGPlayerPriceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abilities_CardId",
                table: "Abilities",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_AncientTraits_CardId",
                table: "AncientTraits",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Attacks_CardId",
                table: "Attacks",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_CardMarkets_CardId",
                table: "CardMarkets",
                column: "CardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_CardImageId",
                table: "Cards",
                column: "CardImageId",
                unique: true,
                filter: "[CardImageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_DeckId",
                table: "Cards",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_ExternalId",
                table: "Cards",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cards_LegalitiesId",
                table: "Cards",
                column: "LegalitiesId",
                unique: true,
                filter: "[LegalitiesId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_SetId",
                table: "Cards",
                column: "SetId");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_TCGPlayerPriceId_Type",
                table: "Prices",
                columns: new[] { "TCGPlayerPriceId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resistances_CardId",
                table: "Resistances",
                column: "CardId");

            migrationBuilder.CreateIndex(
                name: "IX_Sets_ImagesId",
                table: "Sets",
                column: "ImagesId");

            migrationBuilder.CreateIndex(
                name: "IX_Sets_LegalitiesId",
                table: "Sets",
                column: "LegalitiesId");

            migrationBuilder.CreateIndex(
                name: "IX_TCGPlayerPrices_TCGPlayerId",
                table: "TCGPlayerPrices",
                column: "TCGPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TCGPlayers_CardId",
                table: "TCGPlayers",
                column: "CardId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Weaknesses_CardId",
                table: "Weaknesses",
                column: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Abilities");

            migrationBuilder.DropTable(
                name: "AncientTraits");

            migrationBuilder.DropTable(
                name: "Attacks");

            migrationBuilder.DropTable(
                name: "CardMarketPrices");

            migrationBuilder.DropTable(
                name: "Prices");

            migrationBuilder.DropTable(
                name: "Resistances");

            migrationBuilder.DropTable(
                name: "Weaknesses");

            migrationBuilder.DropTable(
                name: "CardMarkets");

            migrationBuilder.DropTable(
                name: "TCGPlayerPrices");

            migrationBuilder.DropTable(
                name: "TCGPlayers");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "CardImages");

            migrationBuilder.DropTable(
                name: "Decks");

            migrationBuilder.DropTable(
                name: "Sets");

            migrationBuilder.DropTable(
                name: "Legalities");

            migrationBuilder.DropTable(
                name: "SetImages");
        }
    }
}
