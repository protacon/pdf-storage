﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pdf.Storage.Data
{
    public class PdfDataContext : DbContext
    {
        public PdfDataContext(DbContextOptions<PdfDataContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (Database.IsNpgsql())
            {
                modelBuilder.Entity<PdfRawDataEntity>(eb =>
                {
                    eb.Property(b => b.TemplateData).HasColumnType("jsonb");
                    eb.Property(b => b.Options).HasColumnType("jsonb");
                });
            }

            if (Database.IsSqlServer())
            {
                modelBuilder.Entity<PdfRawDataEntity>(eb =>
                {
                    eb.Property(b => b.TemplateData).HasColumnType("nvarchar(max)");
                    eb.Property(b => b.Options).HasColumnType("nvarchar(max)");
                });
            }

            modelBuilder.Entity<PdfOpenedEntity>()
                .HasOne(x => x.Parent)
                .WithMany(x => x.Usage);

            modelBuilder.Entity<PdfRawDataEntity>().Property(e => e.TemplateData).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<JObject>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            modelBuilder.Entity<PdfRawDataEntity>().Property(e => e.Options).HasConversion(
                v => JsonConvert.SerializeObject(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                v => JsonConvert.DeserializeObject<JObject>(v, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

            modelBuilder.Entity<PdfEntity>()
                .HasIndex(b => b.FileId);

            modelBuilder.Entity<PdfEntity>()
                .HasIndex(b => b.GroupId);
        }

        public DbSet<PdfEntity> PdfFiles { get; set; }
        public DbSet<PdfRawDataEntity> RawData { get; set; }
    }
}
