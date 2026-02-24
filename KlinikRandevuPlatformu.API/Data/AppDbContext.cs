using KlinikRandevuPlatformu.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;


namespace KlinikRandevuPlatformu.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceSchedule> ServiceSchedules => Set<ServiceSchedule>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<DiseaseType> DiseaseTypes => Set<DiseaseType>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique Username
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // Clinic -> OwnerUser (AppUser)
        modelBuilder.Entity<Clinic>()
            .HasOne(c => c.OwnerUser)
            .WithMany(u => u.OwnedClinics)
            .HasForeignKey(c => c.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Clinic -> Services
        modelBuilder.Entity<Service>()
            .HasOne(s => s.Clinic)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.ClinicId)
            .OnDelete(DeleteBehavior.Cascade);

        // Service -> Schedules
        modelBuilder.Entity<ServiceSchedule>()
            .HasOne(sc => sc.Service)
            .WithMany(s => s.Schedules)
            .HasForeignKey(sc => sc.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Price precision
        modelBuilder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        // Appointment relations
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Clinic)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.PatientUser)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.PatientUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.DiseaseType)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DiseaseTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // منع حجز نفس الخدمة في نفس الوقت مرتين (اختياري قوي)
        modelBuilder.Entity<Appointment>()
            .HasIndex(a => new { a.ServiceId, a.AppointmentDateTime })
            .IsUnique();
    }
}