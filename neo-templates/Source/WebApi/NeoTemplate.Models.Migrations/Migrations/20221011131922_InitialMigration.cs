#pragma warning disable
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NeoTemplate.Models.Migrations.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AggregateRoots",
                columns: table => new
                {
                    AggregateRootId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AggregateRootName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExampleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Audit_CreatedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_CreatedOn = table.Column<DateTime>(type: "datetime", nullable: true),
                    Audit_ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    Audit_ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AggregateRoots", x => x.AggregateRootId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AggregateRoots");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
