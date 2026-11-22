using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeFlow.Infrastructure.Data.Migrations
{
    public partial class AddEmailOtpAndExternalLogins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmailOtpExpiresUtc",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailOtpFailedAttempts",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EmailOtpHash",
                table: "Users",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailOtpPurpose",
                table: "Users",
                type: "TEXT",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExternalLogins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ProviderSubject = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_Provider_ProviderSubject",
                table: "ExternalLogins",
                columns: new[] { "Provider", "ProviderSubject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_UserId",
                table: "ExternalLogins",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalLogins");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailOtpExpiresUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailOtpFailedAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailOtpHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailOtpPurpose",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Users");
        }
    }
}
