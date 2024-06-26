﻿// <auto-generated />
using System;
using FileUploadService.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FileUploadService.Migrations
{
    [DbContext(typeof(ProjectFileDbContext))]
    [Migration("20190703025549_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FileUploadService.Model.ProjectFile", b =>
                {
                    b.Property<int>("ProjectFileId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("FileName");

                    b.Property<string>("FilePath");

                    b.Property<bool?>("IsActive");

                    b.Property<bool>("IsDeleted");

                    b.Property<int?>("ProjectId");

                    b.Property<string>("SourceConfiguration");

                    b.Property<int>("SourceTypeId");

                    b.Property<DateTime>("UploadDate");

                    b.Property<int>("UserId");

                    b.HasKey("ProjectFileId");

                    b.ToTable("ProjectFile");
                });
#pragma warning restore 612, 618
        }
    }
}
