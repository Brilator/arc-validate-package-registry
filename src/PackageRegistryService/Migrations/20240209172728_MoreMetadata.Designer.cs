﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PackageRegistryService.Models;

#nullable disable

namespace PackageRegistryService.Migrations
{
    [DbContext(typeof(ValidationPackageDb))]
    [Migration("20240209172728_MoreMetadata")]
    partial class MoreMetadata
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PackageRegistryService.Models.ValidationPackage", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<int>("MajorVersion")
                        .HasColumnType("integer");

                    b.Property<int>("MinorVersion")
                        .HasColumnType("integer");

                    b.Property<int>("PatchVersion")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte[]>("PackageContent")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("ReleaseNotes")
                        .HasColumnType("text");

                    b.Property<string[]>("Tags")
                        .HasColumnType("text[]");

                    b.HasKey("Name", "MajorVersion", "MinorVersion", "PatchVersion");

                    b.ToTable("ValidationPackages");
                });

            modelBuilder.Entity("PackageRegistryService.Models.ValidationPackage", b =>
                {
                    b.OwnsMany("PackageRegistryService.Models.Author", "Authors", b1 =>
                        {
                            b1.Property<string>("ValidationPackageName")
                                .HasColumnType("text");

                            b1.Property<int>("ValidationPackageMajorVersion")
                                .HasColumnType("integer");

                            b1.Property<int>("ValidationPackageMinorVersion")
                                .HasColumnType("integer");

                            b1.Property<int>("ValidationPackagePatchVersion")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            b1.Property<string>("Affiliation")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("AffiliationLink")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Email")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("FullName")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("ValidationPackageName", "ValidationPackageMajorVersion", "ValidationPackageMinorVersion", "ValidationPackagePatchVersion", "Id");

                            b1.ToTable("ValidationPackages");

                            b1.ToJson("Authors");

                            b1.WithOwner()
                                .HasForeignKey("ValidationPackageName", "ValidationPackageMajorVersion", "ValidationPackageMinorVersion", "ValidationPackagePatchVersion");
                        });

                    b.Navigation("Authors");
                });
#pragma warning restore 612, 618
        }
    }
}
