﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Pdf.Storage.Migrations;

namespace Pdf.Storage.Migrations.NpSql
{
    [DbContext(typeof(NpSqlDataContextForMigrations))]
    [Migration("20191212135553_RawDataDrop")]
    partial class RawDataDrop
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Pdf.Storage.Data.PdfEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("FileId")
                        .HasColumnType("text");

                    b.Property<string>("GroupId")
                        .HasColumnType("text");

                    b.Property<string>("HangfireJobId")
                        .HasColumnType("text");

                    b.Property<int>("OpenedTimes")
                        .HasColumnType("integer");

                    b.Property<string>("Options")
                        .HasColumnType("jsonb");

                    b.Property<bool>("Processed")
                        .HasColumnType("boolean");

                    b.Property<bool>("Removed")
                        .HasColumnType("boolean");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("FileId");

                    b.HasIndex("GroupId");

                    b.ToTable("PdfFiles");
                });

            modelBuilder.Entity("Pdf.Storage.Data.PdfOpenedEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Stamp")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("PdfOpenedEntity");
                });

            modelBuilder.Entity("Pdf.Storage.Data.PdfOpenedEntity", b =>
                {
                    b.HasOne("Pdf.Storage.Data.PdfEntity", "Parent")
                        .WithMany("Usage")
                        .HasForeignKey("ParentId");
                });
#pragma warning restore 612, 618
        }
    }
}
