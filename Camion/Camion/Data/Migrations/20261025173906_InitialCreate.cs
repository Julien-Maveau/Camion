using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Camion.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nom = table.Column<string>(type: "text", nullable: false),
                    Prenom = table.Column<string>(type: "text", nullable: false),
                    DateNaissance = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Telephone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Voitures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Marque = table.Column<string>(type: "text", nullable: false),
                    Modele = table.Column<string>(type: "text", nullable: false),
                    Annee = table.Column<int>(type: "integer", nullable: false),
                    PrixHt = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Couleur = table.Column<string>(type: "text", nullable: false),
                    EstVendue = table.Column<bool>(type: "boolean", nullable: false),
                    DateVente = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voitures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientVoitures",
                columns: table => new
                {
                    ClientsId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoituresId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientVoitures", x => new { x.ClientsId, x.VoituresId });
                    table.ForeignKey(
                        name: "FK_ClientVoitures_Clients_ClientsId",
                        column: x => x.ClientsId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientVoitures_Voitures_VoituresId",
                        column: x => x.VoituresId,
                        principalTable: "Voitures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientVoitures_VoituresId",
                table: "ClientVoitures",
                column: "VoituresId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientVoitures");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Voitures");
        }
    }
}
