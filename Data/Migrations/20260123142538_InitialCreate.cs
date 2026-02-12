using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "materiais",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    ultimaker_material_guid = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    densidade = table.Column<decimal>(type: "numeric(5,3)", precision: 5, scale: 3, nullable: false),
                    fabricante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materiais", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mesas_producao",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    datetime_started = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    datetime_finished = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    material_0_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    material_1_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    material_0_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    material_1_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    material_0_weight_g = table.Column<decimal>(type: "numeric", nullable: false),
                    material_1_weight_g = table.Column<decimal>(type: "numeric", nullable: false),
                    print_time = table.Column<decimal>(type: "numeric", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    mesa_id = table.Column<int>(type: "integer", nullable: false),
                    machine_id = table.Column<int>(type: "integer", nullable: false),
                    job_name = table.Column<string>(type: "text", nullable: false),
                    job_id = table.Column<string>(type: "text", nullable: false),
                    ultimaker_job_uuid = table.Column<string>(type: "text", nullable: false),
                    is_prototype = table.Column<bool>(type: "boolean", nullable: false),
                    is_recondicionado = table.Column<bool>(type: "boolean", nullable: false),
                    job_type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mesas_producao", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_materiais_ultimaker_material_guid",
                table: "materiais",
                column: "ultimaker_material_guid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "materiais");

            migrationBuilder.DropTable(
                name: "mesas_producao");
        }
    }
}
