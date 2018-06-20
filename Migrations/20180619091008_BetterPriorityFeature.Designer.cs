﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Pdf.Storage.Data;
using System;

namespace Pdf.Storage.Migrations
{
    [DbContext(typeof(PdfDataContext))]
    [Migration("20180619091008_BetterPriorityFeature")]
    partial class BetterPriorityFeature
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.3-rtm-10026");

            modelBuilder.Entity("Pdf.Storage.Data.PdfEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("FileId");

                    b.Property<string>("GroupId");

                    b.Property<string>("HangfireJobId");

                    b.Property<int>("OpenedTimes");

                    b.Property<bool>("Processed");

                    b.Property<bool>("Removed");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("PdfFiles");
                });

            modelBuilder.Entity("Pdf.Storage.Data.PdfOpenedEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("ParentId");

                    b.Property<DateTime>("Stamp");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("PdfOpenedEntity");
                });

            modelBuilder.Entity("Pdf.Storage.Data.PdfRawDataEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Html");

                    b.Property<string>("Options")
                        .HasColumnType("jsonb");

                    b.Property<Guid>("ParentId");

                    b.Property<string>("TemplateData")
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.ToTable("RawData");
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
