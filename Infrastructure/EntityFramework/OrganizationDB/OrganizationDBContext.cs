using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VErp.Infrastructure.EF.EFExtensions;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class OrganizationDBContext : DbContext
    {
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<CustomerContact> CustomerContact { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<BusinessInfo> BusinessInfo { get; set; }
        public virtual DbSet<CustomerBankAccount> CustomerBankAccount { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<EmployeeDepartmentMapping> EmployeeDepartmentMapping { get; set; }

        public OrganizationDBContext()
        {
        }

        public OrganizationDBContext(DbContextOptions<OrganizationDBContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

                entity.Property(e => e.PhoneNumber).HasMaxLength(32);

                entity.Property(e => e.TaxIdNo).HasMaxLength(64);

                entity.Property(e => e.Website).HasMaxLength(128);

                entity.Property(e => e.LegalRepresentative).HasMaxLength(128);
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

            modelBuilder.Entity<BusinessInfo>(entity =>
            {
                entity.Property(bi => bi.CompanyName).IsRequired().HasMaxLength(128);

                entity.Property(bi => bi.LegalRepresentative).IsRequired().HasMaxLength(128);

                entity.Property(bi => bi.Address).IsRequired().HasMaxLength(128);

                entity.Property(bi => bi.TaxIdNo).IsRequired().HasMaxLength(64);

                entity.Property(bi => bi.Website).HasMaxLength(128);

                entity.Property(bi => bi.PhoneNumber).IsRequired().HasMaxLength(32);

                entity.Property(bi => bi.Email).IsRequired().HasMaxLength(128);
            });

            modelBuilder.Entity<CustomerBankAccount>(entity =>
            {
                entity.Property(ba => ba.BankName).IsRequired().HasMaxLength(128);

                entity.Property(ba => ba.AccountNumber).IsRequired().HasMaxLength(32);

                entity.Property(ba => ba.SwiffCode).IsRequired().HasMaxLength(64);

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.CustomerBankAccount)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_BankAccount_Customer");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(d => d.DepartmentCode).IsRequired().HasMaxLength(32);

                entity.Property(d => d.DepartmentName).IsRequired().HasMaxLength(128);

                entity.Property(d => d.Description).HasMaxLength(128);

                entity.HasOne(d => d.Parent)
                  .WithMany(d => d.Childs)
                  .HasForeignKey(d => d.ParentId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_Department_SelfKey");
            });

            modelBuilder.Entity<EmployeeDepartmentMapping>(entity =>
            {
                entity.HasOne(m => m.Employee)
                  .WithMany(e => e.EmployeeDepartmentMapping)
                  .HasForeignKey(m => m.UserId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_EmployeeDepartment_User");
                entity.HasOne(m => m.Department)
                 .WithMany(d => d.UserDepartmentMapping)
                 .HasForeignKey(m => m.DepartmentId)
                 .OnDelete(DeleteBehavior.ClientSetNull)
                 .HasConstraintName("FK_EmployeeDepartment_Department");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
