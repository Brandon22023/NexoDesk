using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestructura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SlaHoras = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.CheckConstraint("CK_Categorias_SlaHoras_Positivo", "\"SlaHoras\" > 0");
                    table.ForeignKey(
                        name: "FK_Categorias_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 254, nullable: false, collation: "NOCASE"),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Rol = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solicitudes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TenantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CategoriaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Prioridad = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SolicitanteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgenteId = table.Column<Guid>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaLimiteSla = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotivoResolucion = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitudes", x => x.Id);
                    table.CheckConstraint("CK_Solicitudes_Descripcion_Longitud", "length(\"Descripcion\") BETWEEN 10 AND 4000");
                    table.CheckConstraint("CK_Solicitudes_Titulo_Longitud", "length(\"Titulo\") BETWEEN 5 AND 120");
                    table.ForeignKey(
                        name: "FK_Solicitudes_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_AgenteId",
                        column: x => x.AgenteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitudes_Usuarios_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_TenantId_Activo",
                table: "Categorias",
                columns: new[] { "TenantId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "UX_Categorias_TenantId_Nombre",
                table: "Categorias",
                columns: new[] { "TenantId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_AgenteId",
                table: "Solicitudes",
                column: "AgenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_CategoriaId",
                table: "Solicitudes",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_SolicitanteId",
                table: "Solicitudes",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_AgenteId",
                table: "Solicitudes",
                columns: new[] { "TenantId", "AgenteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_CategoriaId",
                table: "Solicitudes",
                columns: new[] { "TenantId", "CategoriaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_Estado_FechaCreacion",
                table: "Solicitudes",
                columns: new[] { "TenantId", "Estado", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_FechaLimiteSla",
                table: "Solicitudes",
                columns: new[] { "TenantId", "FechaLimiteSla" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_Prioridad_FechaCreacion",
                table: "Solicitudes",
                columns: new[] { "TenantId", "Prioridad", "FechaCreacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Solicitudes_TenantId_SolicitanteId",
                table: "Solicitudes",
                columns: new[] { "TenantId", "SolicitanteId" });

            migrationBuilder.CreateIndex(
                name: "UX_Solicitudes_TenantId_Codigo",
                table: "Solicitudes",
                columns: new[] { "TenantId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Activo",
                table: "Tenants",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "UX_Tenants_Nombre",
                table: "Tenants",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId_Activo",
                table: "Usuarios",
                columns: new[] { "TenantId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TenantId_Rol",
                table: "Usuarios",
                columns: new[] { "TenantId", "Rol" });

            migrationBuilder.CreateIndex(
                name: "UX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Solicitudes");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
