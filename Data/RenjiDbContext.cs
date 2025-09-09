using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using renjibackend.Models;

namespace renjibackend.Data;

public partial class RenjiDbContext : DbContext
{
    public RenjiDbContext()
    {
    }

    public RenjiDbContext(DbContextOptions<RenjiDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Accident> Accidents { get; set; }

    public virtual DbSet<ActionPlan> ActionPlans { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<IncidentReport> IncidentReports { get; set; }

    public virtual DbSet<MaintenanceTeam> MaintenanceTeams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-P3T6S9M\\SQLEXPRESS;Database=RenjiDB;Trusted_Connection=True;Encrypt=False;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accident>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Accident__3214EC07D65820FA");

            entity.ToTable("Accident");

            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<ActionPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ActionPl__3214EC0740BC8101");

            entity.ToTable("ActionPlan");

            entity.Property(e => e.ActionDetail).HasMaxLength(500);
            entity.Property(e => e.CompletedDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.MaintenanceStaff).WithMany(p => p.ActionPlans)
                .HasForeignKey(d => d.MaintenanceStaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActionPlan_MaintenanceTeam");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC07CF48B4E0");

            entity.ToTable("Department");

            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<IncidentReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Incident__3214EC077C1EC1D4");

            entity.ToTable("IncidentReport");

            entity.Property(e => e.AttachmentPath).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.ReportedDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Accident).WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.AccidentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentReport_Accident");

            entity.HasOne(d => d.ActionPlan).WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.ActionPlanId)
                .HasConstraintName("FK_IncidentReport_ActionPlan");

            entity.HasOne(d => d.Department).WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentReport_Department");

            entity.HasOne(d => d.ReportedByNavigation).WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.ReportedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IncidentReport_User");
        });

        modelBuilder.Entity<MaintenanceTeam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Maintena__3214EC075034D761");

            entity.ToTable("MaintenanceTeam");

            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07FB7BFF73");

            entity.ToTable("User");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
