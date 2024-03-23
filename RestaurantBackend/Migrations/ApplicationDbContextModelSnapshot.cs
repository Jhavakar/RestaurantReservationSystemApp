﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RestaurantBackend.Data;

#nullable disable

namespace RestaurantBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.3");

            modelBuilder.Entity("RestaurantBackend.Models.Customer", b =>
                {
                    b.Property<int>("CustomerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNo")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("CustomerId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("RestaurantBackend.Models.Payment", b =>
                {
                    b.Property<int>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("ReservationId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("TotalAmount")
                        .HasColumnType("REAL");

                    b.HasKey("PaymentId");

                    b.HasIndex("ReservationId")
                        .IsUnique();

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("RestaurantBackend.Models.Receipt", b =>
                {
                    b.Property<int>("ReceiptId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("PaymentId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReservationId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("TotalPrice")
                        .HasColumnType("REAL");

                    b.HasKey("ReceiptId");

                    b.HasIndex("PaymentId")
                        .IsUnique();

                    b.HasIndex("ReservationId");

                    b.ToTable("Receipts");
                });

            modelBuilder.Entity("RestaurantBackend.Models.Reservation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CustomerId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("ReservationDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReservationEndTime")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReservationTime")
                        .HasColumnType("TEXT");

                    b.Property<int>("TableId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<double>("Total")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("TableId");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("RestaurantBackend.Models.Table", b =>
                {
                    b.Property<int>("TableId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Capacity")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TableNumber")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.HasKey("TableId");

                    b.ToTable("RestaurantTables");
                });

            modelBuilder.Entity("RestaurantBackend.Models.Payment", b =>
                {
                    b.HasOne("RestaurantBackend.Models.Reservation", "Reservation")
                        .WithOne("Payment")
                        .HasForeignKey("RestaurantBackend.Models.Payment", "ReservationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Reservation");
                });

            modelBuilder.Entity("RestaurantBackend.Models.Receipt", b =>
                {
                    b.HasOne("RestaurantBackend.Models.Payment", "Payment")
                        .WithOne("Receipt")
                        .HasForeignKey("RestaurantBackend.Models.Receipt", "PaymentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RestaurantBackend.Models.Reservation", "Reservation")
                        .WithMany()
                        .HasForeignKey("ReservationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Payment");

                    b.Navigation("Reservation");
                });

            modelBuilder.Entity("RestaurantBackend.Models.Reservation", b =>
                {
                    b.HasOne("RestaurantBackend.Models.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RestaurantBackend.Models.Table", "Table")
                        .WithMany("Reservations")
                        .HasForeignKey("TableId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");

                    b.Navigation("Table");
                });

            modelBuilder.Entity("RestaurantBackend.Models.Payment", b =>
                {
                    b.Navigation("Receipt")
                        .IsRequired();
                });

            modelBuilder.Entity("RestaurantBackend.Models.Reservation", b =>
                {
                    b.Navigation("Payment")
                        .IsRequired();
                });

            modelBuilder.Entity("RestaurantBackend.Models.Table", b =>
                {
                    b.Navigation("Reservations");
                });
#pragma warning restore 612, 618
        }
    }
}
