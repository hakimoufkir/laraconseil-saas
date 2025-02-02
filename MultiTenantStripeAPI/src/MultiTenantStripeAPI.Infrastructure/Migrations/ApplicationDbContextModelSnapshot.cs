﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MultiTenantStripeAPI.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Permissions");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "View own farms and plots",
                            Name = "ViewOwnData"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Submit treatments",
                            Name = "SubmitTreatment"
                        },
                        new
                        {
                            Id = 3,
                            Description = "Respond to audits",
                            Name = "RespondToAudit"
                        });
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Description = "Grower role",
                            Name = "Grower",
                            TenantId = "tenant1-id"
                        },
                        new
                        {
                            Id = 2,
                            Description = "Station Admin role",
                            Name = "StationAdmin",
                            TenantId = "tenant1-id"
                        });
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.RolePermission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("PermissionId")
                        .HasColumnType("integer");

                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PermissionId");

                    b.HasIndex("RoleId");

                    b.ToTable("RolePermissions");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            PermissionId = 1,
                            RoleId = 1
                        },
                        new
                        {
                            Id = 2,
                            PermissionId = 2,
                            RoleId = 1
                        },
                        new
                        {
                            Id = 3,
                            PermissionId = 3,
                            RoleId = 1
                        });
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.Tenant", b =>
                {
                    b.Property<string>("TenantId")
                        .HasColumnType("text");

                    b.Property<string>("BrandingLogoUrl")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DatabaseConnectionString")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DeactivationReason")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MaxUsers")
                        .HasColumnType("integer");

                    b.Property<string>("PlanType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SubscriptionStatus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TenantName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ThemeColor")
                        .HasColumnType("text");

                    b.HasKey("TenantId");

                    b.ToTable("Tenants");

                    b.HasData(
                        new
                        {
                            TenantId = "tenant1-id",
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            DatabaseConnectionString = "ConnectionStringForTenant1",
                            Email = "tenant1@example.com",
                            LastUpdated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            MaxUsers = 0,
                            PlanType = "Grower",
                            SubscriptionStatus = "Active",
                            TenantName = "Tenant One"
                        },
                        new
                        {
                            TenantId = "tenant2-id",
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            DatabaseConnectionString = "ConnectionStringForTenant2",
                            Email = "tenant2@example.com",
                            LastUpdated = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            MaxUsers = 0,
                            PlanType = "Station",
                            SubscriptionStatus = "Pending",
                            TenantName = "Tenant Two"
                        });
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("TenantId");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Email = "john.doe@example.com",
                            TenantId = "tenant1-id",
                            UserName = "John Doe"
                        },
                        new
                        {
                            Id = 2,
                            Email = "jane.smith@example.com",
                            TenantId = "tenant1-id",
                            UserName = "Jane Smith"
                        },
                        new
                        {
                            Id = 3,
                            Email = "emily.davis@example.com",
                            TenantId = "tenant2-id",
                            UserName = "Emily Davis"
                        });
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.UserRole", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("UserRoles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            RoleId = 1,
                            UserId = 1
                        },
                        new
                        {
                            Id = 2,
                            RoleId = 2,
                            UserId = 2
                        },
                        new
                        {
                            Id = 3,
                            RoleId = 1,
                            UserId = 3
                        });
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.RolePermission", b =>
                {
                    b.HasOne("MultiTenantStripeAPI.Domain.Entities.Permission", "Permission")
                        .WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MultiTenantStripeAPI.Domain.Entities.Role", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.User", b =>
                {
                    b.HasOne("MultiTenantStripeAPI.Domain.Entities.Tenant", "Tenant")
                        .WithMany()
                        .HasForeignKey("TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Tenant");
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.UserRole", b =>
                {
                    b.HasOne("MultiTenantStripeAPI.Domain.Entities.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MultiTenantStripeAPI.Domain.Entities.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.User", b =>
                {
                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}
