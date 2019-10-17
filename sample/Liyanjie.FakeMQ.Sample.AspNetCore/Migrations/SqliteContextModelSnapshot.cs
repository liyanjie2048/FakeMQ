﻿// <auto-generated />
using System;
using Liyanjie.FakeMQ.Sample.AspNetCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Migrations
{
    [DbContext(typeof(SqliteContext))]
    partial class SqliteContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.11-servicing-32099");

            modelBuilder.Entity("Liyanjie.FakeMQ.FakeMQEvent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Message");

                    b.Property<long>("Timestamp");

                    b.Property<string>("Type")
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.HasIndex("Timestamp");

                    b.HasIndex("Type");

                    b.ToTable("FakeMQEvents");
                });

            modelBuilder.Entity("Liyanjie.FakeMQ.FakeMQProcess", b =>
                {
                    b.Property<string>("Subscription")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("Timestamp");

                    b.HasKey("Subscription");

                    b.ToTable("FakeMQProcesses");
                });

            modelBuilder.Entity("Liyanjie.FakeMQ.Sample.AspNetCore.Domains.Message", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Content")
                        .HasMaxLength(500);

                    b.HasKey("Id");

                    b.ToTable("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
