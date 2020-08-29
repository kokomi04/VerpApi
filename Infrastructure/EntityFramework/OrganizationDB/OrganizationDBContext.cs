using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class OrganizationDBContext : DbContext
    {
        public OrganizationDBContext()
        {
        }

        public OrganizationDBContext(DbContextOptions<OrganizationDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BusinessInfo> BusinessInfo { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<CustomerBankAccount> CustomerBankAccount { get; set; }
        public virtual DbSet<CustomerContact> CustomerContact { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; }
        public virtual DbSet<EmployeeSubsidiary> EmployeeSubsidiary { get; set; }
        public virtual DbSet<ObjectProcessObject> ObjectProcessObject { get; set; }
        public virtual DbSet<ObjectProcessStep> ObjectProcessStep { get; set; }
        public virtual DbSet<ObjectProcessStepDepend> ObjectProcessStepDepend { get; set; }
        public virtual DbSet<ObjectProcessStepUser> ObjectProcessStepUser { get; set; }
        public virtual DbSet<Subsidiary> Subsidiary { get; set; }
        public virtual DbSet<SystemParameter> SystemParameter { get; set; }
        public virtual DbSet<UserData> UserData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BusinessInfo>(entity =>
            {
                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LegalRepresentative)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.TaxIdNo)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.Website).HasMaxLength(128);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(128);

                entity.Property(e => e.CustomerCode).HasMaxLength(128);

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CustomerStatusId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.Identify).HasMaxLength(64);

                entity.Property(e => e.LegalRepresentative).HasMaxLength(128);

                entity.Property(e => e.PhoneNumber).HasMaxLength(32);

                entity.Property(e => e.TaxIdNo).HasMaxLength(64);

                entity.Property(e => e.Website).HasMaxLength(128);
            });

            modelBuilder.Entity<CustomerBankAccount>(entity =>
            {
                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.BankName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.SwiffCode)
                    .IsRequired()
                    .HasMaxLength(64)
                    .IsUnicode(false);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerBankAccount)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BankAccount_Customer");
            });

            modelBuilder.Entity<CustomerContact>(entity =>
            {
                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.FullName).HasMaxLength(128);

                entity.Property(e => e.PhoneNumber).HasMaxLength(32);

                entity.Property(e => e.Position).HasMaxLength(128);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerContact)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CustomerContact_Customer");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.DepartmentCode)
                    .IsRequired()
                    .HasMaxLength(32)
                    .IsUnicode(false);

                entity.Property(e => e.DepartmentName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(128);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Department_SelfKey");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.Address).HasMaxLength(512);

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.EmployeeCode).HasMaxLength(64);

                entity.Property(e => e.FullName).HasMaxLength(128);

                entity.Property(e => e.Phone).HasMaxLength(64);
            });

            modelBuilder.Entity<EmployeeDepartmentMapping>(entity =>
            {
                entity.HasKey(e => e.UserDepartmentMappingId)
                    .HasName("PK__UserDepa__3E005141E85C5CB7");

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.EffectiveDate).HasColumnType("date");

                entity.Property(e => e.ExpirationDate).HasColumnType("date");

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.EmployeeDepartmentMapping)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeDepartment_Department");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EmployeeDepartmentMapping)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeDepartment_Employee");
            });

            modelBuilder.Entity<EmployeeSubsidiary>(entity =>
            {
                entity.HasOne(d => d.Subsidiary)
                    .WithMany(p => p.EmployeeSubsidiary)
                    .HasForeignKey(d => d.SubsidiaryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeSubsidiary_Subsidiary");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EmployeeSubsidiary)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_EmployeeSubsidiary_Employee");
            });

            modelBuilder.Entity<ObjectProcessObject>(entity =>
            {
                entity.Property(e => e.Note).HasMaxLength(512);
            });

            modelBuilder.Entity<ObjectProcessStep>(entity =>
            {
                entity.Property(e => e.ObjectProcessStepName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ObjectProcessStepDepend>(entity =>
            {
                entity.HasKey(e => new { e.ObjectProcessStepId, e.DependObjectProcessStepId });

                entity.HasOne(d => d.DependObjectProcessStep)
                    .WithMany(p => p.ObjectProcessStepDependDependObjectProcessStep)
                    .HasForeignKey(d => d.DependObjectProcessStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObjectProcessStepDepend_ObjectProcessStep1");

                entity.HasOne(d => d.ObjectProcessStep)
                    .WithMany(p => p.ObjectProcessStepDependObjectProcessStep)
                    .HasForeignKey(d => d.ObjectProcessStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObjectProcessStepDepend_ObjectProcessStep");
            });

            modelBuilder.Entity<ObjectProcessStepUser>(entity =>
            {
                entity.HasKey(e => new { e.ObjectProcessStepId, e.UserId });

                entity.HasOne(d => d.ObjectProcessStep)
                    .WithMany(p => p.ObjectProcessStepUser)
                    .HasForeignKey(d => d.ObjectProcessStepId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObjectProcessStepUser_ObjectProcessStep");
            });

            modelBuilder.Entity<Subsidiary>(entity =>
            {
                entity.Property(e => e.Address).HasMaxLength(128);

                entity.Property(e => e.Description).HasMaxLength(512);

                entity.Property(e => e.Email).HasMaxLength(128);

                entity.Property(e => e.Fax).HasMaxLength(128);

                entity.Property(e => e.PhoneNumber).HasMaxLength(32);

                entity.Property(e => e.SubsidiaryCode).HasMaxLength(128);

                entity.Property(e => e.SubsidiaryName).HasMaxLength(128);

                entity.Property(e => e.TaxIdNo).HasMaxLength(64);

                entity.HasOne(d => d.ParentSubsidiary)
                    .WithMany(p => p.InverseParentSubsidiary)
                    .HasForeignKey(d => d.ParentSubsidiaryId)
                    .HasConstraintName("FK_Subsidiary_Subsidiary");
            });

            modelBuilder.Entity<SystemParameter>(entity =>
            {
                entity.Property(e => e.CreatedDateTimeUtc).HasColumnType("datetime");

                entity.Property(e => e.FieldName)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Name).HasMaxLength(128);

                entity.Property(e => e.UpdatedDateTimeUtc).HasColumnType("datetime");

                entity.Property(e => e.Value).HasMaxLength(512);
            });

            modelBuilder.Entity<UserData>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.DataKey });

                entity.Property(e => e.DataKey).HasMaxLength(128);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
