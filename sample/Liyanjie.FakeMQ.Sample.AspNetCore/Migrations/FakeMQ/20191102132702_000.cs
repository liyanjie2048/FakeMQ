using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Liyanjie.FakeMQ.Sample.AspNetCore.Migrations.FakeMQ
{
    public partial class _000 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FakeMQEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(maxLength: 50, nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Timestamp = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FakeMQEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FakeMQProcesses",
                columns: table => new
                {
                    Subscription = table.Column<string>(nullable: false),
                    Timestamp = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FakeMQProcesses", x => x.Subscription);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FakeMQEvents_Timestamp",
                table: "FakeMQEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_FakeMQEvents_Type",
                table: "FakeMQEvents",
                column: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FakeMQEvents");

            migrationBuilder.DropTable(
                name: "FakeMQProcesses");
        }
    }
}
