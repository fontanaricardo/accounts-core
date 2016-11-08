using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using Accounts.Models;

namespace Accounts.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    City = table.Column<string>(maxLength: 200, nullable: false),
                    Complement = table.Column<string>(maxLength: 2000, nullable: true),
                    District = table.Column<string>(maxLength: 200, nullable: false),
                    Number = table.Column<int>(nullable: true),
                    State = table.Column<string>(nullable: false),
                    Street = table.Column<string>(maxLength: 2000, nullable: false),
                    ZipCode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressID);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    ApplicationId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ApplicationName = table.Column<string>(maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Enabled = table.Column<bool>(nullable: false),
                    RequiresApproval = table.Column<bool>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    UseTerms = table.Column<string>(nullable: true),
                    UserType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.ApplicationId);
                });

            migrationBuilder.CreateTable(
                name: "AuthenticationTokens",
                columns: table => new
                {
                    TokenID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Domain = table.Column<string>(nullable: false),
                    Expiration = table.Column<DateTime>(nullable: false),
                    Token = table.Column<string>(nullable: false),
                    UsedAt = table.Column<DateTime>(nullable: true),
                    UserName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthenticationTokens", x => x.TokenID);
                });

            migrationBuilder.CreateTable(
                name: "Phones",
                columns: table => new
                {
                    PhoneID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    Document = table.Column<string>(maxLength: 14, nullable: false),
                    Number = table.Column<string>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phones", x => x.PhoneID);
                });

            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    CompanyID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddressID = table.Column<int>(nullable: false),
                    CNPJ = table.Column<string>(maxLength: 14, nullable: false),
                    CompanyName = table.Column<string>(maxLength: 2000, nullable: false),
                    Email = table.Column<string>(nullable: false),
                    MunicipalRegistration = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.CompanyID);
                    table.ForeignKey(
                        name: "FK_Companies_Addresses_AddressID",
                        column: x => x.AddressID,
                        principalTable: "Addresses",
                        principalColumn: "AddressID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    PersonID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddressID = table.Column<int>(nullable: false),
                    CPF = table.Column<string>(maxLength: 11, nullable: false),
                    Dispatcher = table.Column<string>(maxLength: 2000, nullable: false),
                    EletronicSignatureStatus = table.Column<int>(nullable: false),
                    Email = table.Column<string>(nullable: false),
                    LinkSeiProtocol = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 1500, nullable: false),
                    RG = table.Column<string>(maxLength: 500, nullable: false),
                    SeiId = table.Column<long>(nullable: true),
                    SeiProtocol = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.PersonID);
                    table.ForeignKey(
                        name: "FK_People_Addresses_AddressID",
                        column: x => x.AddressID,
                        principalTable: "Addresses",
                        principalColumn: "AddressID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accesses",
                columns: table => new
                {
                    AccessId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AcceptedTerms = table.Column<bool>(nullable: true),
                    ApplicationId = table.Column<int>(nullable: false),
                    ApprovedAt = table.Column<DateTime>(nullable: true),
                    ApprovedBy = table.Column<string>(nullable: true),
                    CompanyID = table.Column<int>(nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    DeniedAt = table.Column<DateTime>(nullable: true),
                    DeniedBy = table.Column<string>(nullable: true),
                    DeniedCause = table.Column<string>(nullable: true),
                    Document = table.Column<string>(maxLength: 14, nullable: false),
                    PersonID = table.Column<int>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accesses", x => x.AccessId);
                    table.ForeignKey(
                        name: "FK_Accesses_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Accesses_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accesses_People_PersonID",
                        column: x => x.PersonID,
                        principalTable: "People",
                        principalColumn: "PersonID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<int>(
                name: "EletronicSignatureStatus",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: EletronicSignatureStatus.Unsolicited);

            migrationBuilder.AddColumn<string>(
                name: "FullUserName",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_ApplicationId",
                table: "Accesses",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_CompanyID",
                table: "Accesses",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_Accesses_PersonID",
                table: "Accesses",
                column: "PersonID");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_AddressID",
                table: "Companies",
                column: "AddressID");

            migrationBuilder.CreateIndex(
                name: "IX_People_AddressID",
                table: "People",
                column: "AddressID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EletronicSignatureStatus",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullUserName",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Accesses");

            migrationBuilder.DropTable(
                name: "AuthenticationTokens");

            migrationBuilder.DropTable(
                name: "Phones");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Companies");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Addresses");
        }
    }
}
