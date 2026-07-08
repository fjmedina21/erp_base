using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ErpBase.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCloudinaryStorageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cloudinary_public_id",
                schema: "propiedades",
                table: "propiedad_imagenes",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cloudinary_resource_type",
                schema: "propiedades",
                table: "propiedad_imagenes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cloudinary_public_id",
                schema: "propiedades",
                table: "propiedad_documentos",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cloudinary_resource_type",
                schema: "propiedades",
                table: "propiedad_documentos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cloudinary_public_id",
                schema: "propiedades",
                table: "propiedad_imagenes");

            migrationBuilder.DropColumn(
                name: "cloudinary_resource_type",
                schema: "propiedades",
                table: "propiedad_imagenes");

            migrationBuilder.DropColumn(
                name: "cloudinary_public_id",
                schema: "propiedades",
                table: "propiedad_documentos");

            migrationBuilder.DropColumn(
                name: "cloudinary_resource_type",
                schema: "propiedades",
                table: "propiedad_documentos");
        }
    }
}
