using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Accounts.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20161104153224_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Accounts.Models.Access", b =>
                {
                    b.Property<int>("AccessId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool?>("AcceptedTerms");

                    b.Property<int?>("ApplicationId")
                        .IsRequired();

                    b.Property<DateTime?>("ApprovedAt");

                    b.Property<string>("ApprovedBy");

                    b.Property<int?>("CompanyID");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<DateTime?>("DeniedAt");

                    b.Property<string>("DeniedBy");

                    b.Property<string>("DeniedCause");

                    b.Property<string>("Document")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 14);

                    b.Property<int?>("PersonID");

                    b.Property<int>("Status");

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("AccessId");

                    b.HasIndex("ApplicationId");

                    b.HasIndex("CompanyID");

                    b.HasIndex("PersonID");

                    b.ToTable("Accesses");
                });

            modelBuilder.Entity("Accounts.Models.Address", b =>
                {
                    b.Property<int>("AddressID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<string>("Complement")
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<string>("District")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 200);

                    b.Property<int?>("Number");

                    b.Property<string>("State")
                        .IsRequired();

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<int?>("ZipCode")
                        .IsRequired();

                    b.HasKey("AddressID");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("Accounts.Models.Application", b =>
                {
                    b.Property<int>("ApplicationId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationName")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<DateTime>("CreatedAt");

                    b.Property<bool>("Enabled");

                    b.Property<bool>("RequiresApproval");

                    b.Property<DateTime>("UpdatedAt");

                    b.Property<string>("Url");

                    b.Property<string>("UseTerms");

                    b.Property<int>("UserType");

                    b.HasKey("ApplicationId");

                    b.ToTable("Applications");
                });

            modelBuilder.Entity("Accounts.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<int>("EletronicSignatureStatus");

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FullUserName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Accounts.Models.AuthenticationToken", b =>
                {
                    b.Property<int>("TokenID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Domain")
                        .IsRequired();

                    b.Property<DateTime>("Expiration");

                    b.Property<string>("Token")
                        .IsRequired();

                    b.Property<DateTime?>("UsedAt");

                    b.Property<string>("UserName")
                        .IsRequired();

                    b.HasKey("TokenID");

                    b.ToTable("AuthenticationTokens");
                });

            modelBuilder.Entity("Accounts.Models.Company", b =>
                {
                    b.Property<int>("CompanyID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AddressID");

                    b.Property<string>("CNPJ")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 14);

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("MunicipalRegistration");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 2000);

                    b.HasKey("CompanyID");

                    b.HasIndex("AddressID");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("Accounts.Models.Person", b =>
                {
                    b.Property<int>("PersonID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AddressID");

                    b.Property<string>("CPF")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 11);

                    b.Property<string>("Dispatcher")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 2000);

                    b.Property<int>("EletronicSignatureStatus");

                    b.Property<string>("Email")
                        .IsRequired();

                    b.Property<string>("LinkSeiProtocol");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 1500);

                    b.Property<string>("RG")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 500);

                    b.Property<long?>("SeiId");

                    b.Property<string>("SeiProtocol");

                    b.HasKey("PersonID");

                    b.HasIndex("AddressID");

                    b.ToTable("People");
                });

            modelBuilder.Entity("Accounts.Models.Phone", b =>
                {
                    b.Property<int>("PhoneID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<string>("Document")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 14);

                    b.Property<string>("Number")
                        .IsRequired();

                    b.Property<DateTime>("UpdatedAt");

                    b.HasKey("PhoneID");

                    b.ToTable("Phones");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Accounts.Models.Access", b =>
                {
                    b.HasOne("Accounts.Models.Application", "Application")
                        .WithMany("Accesses")
                        .HasForeignKey("ApplicationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Accounts.Models.Company")
                        .WithMany("Access")
                        .HasForeignKey("CompanyID");

                    b.HasOne("Accounts.Models.Person")
                        .WithMany("Access")
                        .HasForeignKey("PersonID");
                });

            modelBuilder.Entity("Accounts.Models.Company", b =>
                {
                    b.HasOne("Accounts.Models.Address", "Address")
                        .WithMany()
                        .HasForeignKey("AddressID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Accounts.Models.Person", b =>
                {
                    b.HasOne("Accounts.Models.Address", "Address")
                        .WithMany()
                        .HasForeignKey("AddressID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Accounts.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Accounts.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Accounts.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
