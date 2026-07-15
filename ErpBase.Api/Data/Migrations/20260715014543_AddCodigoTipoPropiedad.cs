using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBase.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoTipoPropiedad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo",
                schema: "propiedades",
                table: "tipos_propiedad",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE propiedades.tipos_propiedad
                SET codigo = CASE nombre
                    WHEN 'Apartamento' THEN 'APT'
                    WHEN 'Casa' THEN 'CAS'
                    WHEN 'Solar' THEN 'SOL'
                    WHEN 'Local Comercial' THEN 'LOC'
                    WHEN 'Oficina' THEN 'OFI'
                    ELSE codigo
                END
                WHERE nombre IN ('Apartamento', 'Casa', 'Solar', 'Local Comercial', 'Oficina');
                """);

            migrationBuilder.CreateIndex(
                name: "IX_tipos_propiedad_codigo",
                schema: "propiedades",
                table: "tipos_propiedad",
                column: "codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tipos_propiedad_codigo",
                schema: "propiedades",
                table: "tipos_propiedad");

            migrationBuilder.DropColumn(
                name: "codigo",
                schema: "propiedades",
                table: "tipos_propiedad");
        }
    }
}
