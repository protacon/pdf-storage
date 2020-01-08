﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pdf.Storage.Migrations.NpSql
{
    public partial class RawDataDrop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawData");

            migrationBuilder.AddColumn<string>(
                name: "Options",
                table: "PdfFiles",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Options",
                table: "PdfFiles");

            migrationBuilder.CreateTable(
                name: "RawData",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Html = table.Column<string>(nullable: true),
                    Options = table.Column<string>(type: "jsonb", nullable: true),
                    ParentId = table.Column<Guid>(nullable: false),
                    TemplateData = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawData", x => x.Id);
                });
        }
    }
}
