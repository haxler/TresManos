using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TresManos.Backend.Migrations
{
    /// <inheritdoc />
    public partial class inicitaldb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioId);
                });

            migrationBuilder.CreateTable(
                name: "Partidas",
                columns: table => new
                {
                    PartidaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId_Jugador1 = table.Column<int>(type: "int", nullable: false),
                    UsuarioId_Jugador2 = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GanadorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    PerdedorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    EsRevancha = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PartidaOriginalId = table.Column<int>(type: "int", nullable: true),
                    RevanchaAceptadaPorPerdedor = table.Column<bool>(type: "bit", nullable: true),
                    FechaAceptacionRevancha = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidas", x => x.PartidaId);
                    table.ForeignKey(
                        name: "FK_Partidas_Partidas_PartidaOriginalId",
                        column: x => x.PartidaOriginalId,
                        principalTable: "Partidas",
                        principalColumn: "PartidaId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Usuarios_GanadorUsuarioId",
                        column: x => x.GanadorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Usuarios_PerdedorUsuarioId",
                        column: x => x.PerdedorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Usuarios_UsuarioId_Jugador1",
                        column: x => x.UsuarioId_Jugador1,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Usuarios_UsuarioId_Jugador2",
                        column: x => x.UsuarioId_Jugador2,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rondas",
                columns: table => new
                {
                    RondaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartidaId = table.Column<int>(type: "int", nullable: false),
                    NumeroRonda = table.Column<int>(type: "int", nullable: false),
                    MovimientoJugador1 = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    MovimientoJugador2 = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Resultado = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    GanadorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    PerdedorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rondas", x => x.RondaId);
                    table.ForeignKey(
                        name: "FK_Rondas_Partidas_PartidaId",
                        column: x => x.PartidaId,
                        principalTable: "Partidas",
                        principalColumn: "PartidaId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rondas_Usuarios_GanadorUsuarioId",
                        column: x => x.GanadorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rondas_Usuarios_PerdedorUsuarioId",
                        column: x => x.PerdedorUsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_GanadorUsuarioId",
                table: "Partidas",
                column: "GanadorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_PartidaOriginalId",
                table: "Partidas",
                column: "PartidaOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_PerdedorUsuarioId",
                table: "Partidas",
                column: "PerdedorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_UsuarioId_Jugador1",
                table: "Partidas",
                column: "UsuarioId_Jugador1");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_UsuarioId_Jugador2",
                table: "Partidas",
                column: "UsuarioId_Jugador2");

            migrationBuilder.CreateIndex(
                name: "IX_Rondas_GanadorUsuarioId",
                table: "Rondas",
                column: "GanadorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Rondas_PartidaId",
                table: "Rondas",
                column: "PartidaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rondas_PerdedorUsuarioId",
                table: "Rondas",
                column: "PerdedorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_NombreUsuario",
                table: "Usuarios",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rondas");

            migrationBuilder.DropTable(
                name: "Partidas");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
