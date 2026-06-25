using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblioteka.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InicijalnaBaza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Autori",
                columns: table => new
                {
                    AutorId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ime = table.Column<string>(type: "TEXT", nullable: false),
                    Prezime = table.Column<string>(type: "TEXT", nullable: false),
                    Biografija = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autori", x => x.AutorId);
                });

            migrationBuilder.CreateTable(
                name: "Bibliotekari",
                columns: table => new
                {
                    Jmbg = table.Column<string>(type: "TEXT", nullable: false),
                    Ime = table.Column<string>(type: "TEXT", nullable: false),
                    Prezime = table.Column<string>(type: "TEXT", nullable: false),
                    KorisnickoIme = table.Column<string>(type: "TEXT", nullable: false),
                    Lozinka = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bibliotekari", x => x.Jmbg);
                });

            migrationBuilder.CreateTable(
                name: "Gradovi",
                columns: table => new
                {
                    GradId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Naziv = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gradovi", x => x.GradId);
                });

            migrationBuilder.CreateTable(
                name: "Knjige",
                columns: table => new
                {
                    KnjigaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Naslov = table.Column<string>(type: "TEXT", nullable: false),
                    GodinaIzdanja = table.Column<int>(type: "INTEGER", nullable: false),
                    Isbn = table.Column<string>(type: "TEXT", nullable: false),
                    Slika = table.Column<string>(type: "TEXT", nullable: false),
                    BrojStrana = table.Column<int>(type: "INTEGER", nullable: false),
                    Opis = table.Column<string>(type: "TEXT", nullable: false),
                    Zanr = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Knjige", x => x.KnjigaId);
                });

            migrationBuilder.CreateTable(
                name: "Clanovi",
                columns: table => new
                {
                    Jmbg = table.Column<string>(type: "TEXT", nullable: false),
                    Ime = table.Column<string>(type: "TEXT", nullable: false),
                    Prezime = table.Column<string>(type: "TEXT", nullable: false),
                    BrojTelefona = table.Column<string>(type: "TEXT", nullable: false),
                    KorisnickoIme = table.Column<string>(type: "TEXT", nullable: false),
                    Lozinka = table.Column<string>(type: "TEXT", nullable: false),
                    GradId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clanovi", x => x.Jmbg);
                    table.ForeignKey(
                        name: "FK_Clanovi_Gradovi_GradId",
                        column: x => x.GradId,
                        principalTable: "Gradovi",
                        principalColumn: "GradId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pisanje",
                columns: table => new
                {
                    KnjigaId = table.Column<int>(type: "INTEGER", nullable: false),
                    AutorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pisanje", x => new { x.KnjigaId, x.AutorId });
                    table.ForeignKey(
                        name: "FK_Pisanje_Autori_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Autori",
                        principalColumn: "AutorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pisanje_Knjige_KnjigaId",
                        column: x => x.KnjigaId,
                        principalTable: "Knjige",
                        principalColumn: "KnjigaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Izdavanja",
                columns: table => new
                {
                    IzdavanjeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DatumIzdavanja = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DatumVracanja = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Napomena = table.Column<string>(type: "TEXT", nullable: false),
                    BibliotekarJmbg = table.Column<string>(type: "TEXT", nullable: true),
                    ClanJmbg = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Izdavanja", x => x.IzdavanjeId);
                    table.ForeignKey(
                        name: "FK_Izdavanja_Bibliotekari_BibliotekarJmbg",
                        column: x => x.BibliotekarJmbg,
                        principalTable: "Bibliotekari",
                        principalColumn: "Jmbg");
                    table.ForeignKey(
                        name: "FK_Izdavanja_Clanovi_ClanJmbg",
                        column: x => x.ClanJmbg,
                        principalTable: "Clanovi",
                        principalColumn: "Jmbg",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StavkaIzdavanja",
                columns: table => new
                {
                    KnjigaId = table.Column<int>(type: "INTEGER", nullable: false),
                    IzdavanjeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StavkaIzdavanja", x => new { x.IzdavanjeId, x.KnjigaId });
                    table.ForeignKey(
                        name: "FK_StavkaIzdavanja_Izdavanja_IzdavanjeId",
                        column: x => x.IzdavanjeId,
                        principalTable: "Izdavanja",
                        principalColumn: "IzdavanjeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StavkaIzdavanja_Knjige_KnjigaId",
                        column: x => x.KnjigaId,
                        principalTable: "Knjige",
                        principalColumn: "KnjigaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clanovi_GradId",
                table: "Clanovi",
                column: "GradId");

            migrationBuilder.CreateIndex(
                name: "IX_Izdavanja_BibliotekarJmbg",
                table: "Izdavanja",
                column: "BibliotekarJmbg");

            migrationBuilder.CreateIndex(
                name: "IX_Izdavanja_ClanJmbg",
                table: "Izdavanja",
                column: "ClanJmbg");

            migrationBuilder.CreateIndex(
                name: "IX_Pisanje_AutorId",
                table: "Pisanje",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_StavkaIzdavanja_KnjigaId",
                table: "StavkaIzdavanja",
                column: "KnjigaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pisanje");

            migrationBuilder.DropTable(
                name: "StavkaIzdavanja");

            migrationBuilder.DropTable(
                name: "Autori");

            migrationBuilder.DropTable(
                name: "Izdavanja");

            migrationBuilder.DropTable(
                name: "Knjige");

            migrationBuilder.DropTable(
                name: "Bibliotekari");

            migrationBuilder.DropTable(
                name: "Clanovi");

            migrationBuilder.DropTable(
                name: "Gradovi");
        }
    }
}
