﻿// <auto-generated />
using System;
using Liyanjie.FakeMQ.Sample.AspNetCore_3_0.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Liyanjie.FakeMQ.Sample.AspNetCore_3_0.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20191101154750_000")]
    partial class _000
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0");

            modelBuilder.Entity("Liyanjie.FakeMQ.Sample.AspNetCore_3_0.Domains.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT")
                        .HasMaxLength(500);

                    b.HasKey("Id");

                    b.ToTable("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
