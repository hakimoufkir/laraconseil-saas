﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MultiTenantStripeAPI.Infrastructure.Persistence.Context;

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
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MultiTenantStripeAPI.Domain.Entities.Tenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SubscriptionStatus")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TenantId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("TenantName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("TenantId")
                        .IsUnique()
                        .HasFilter("[TenantId] IS NOT NULL");

                    b.ToTable("Tenants");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Email = "tenant1@example.com",
                            SubscriptionStatus = "Active",
                            TenantId = "tenant1-id",
                            TenantName = "Tenant One"
                        },
                        new
                        {
                            Id = 2,
                            Email = "tenant2@example.com",
                            SubscriptionStatus = "Pending",
                            TenantId = "tenant2-id",
                            TenantName = "Tenant Two"
                        },
                        new
                        {
                            Id = 3,
                            Email = "tenant3@example.com",
                            SubscriptionStatus = "Expired",
                            TenantId = "tenant3-id",
                            TenantName = "Tenant Three"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}