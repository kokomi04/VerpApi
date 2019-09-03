using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class MasterDBContext : DbContext
    {
        public MasterDBContext()
        {
        }

        public MasterDBContext(DbContextOptions<MasterDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Action> Action { get; set; }
        public virtual DbSet<ApiEndpoint> ApiEndpoint { get; set; }
        public virtual DbSet<Config> Config { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<Method> Method { get; set; }
        public virtual DbSet<Module> Module { get; set; }
        public virtual DbSet<ModuleApiEndpointMapping> ModuleApiEndpointMapping { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<RolePermission> RolePermission { get; set; }
        public virtual DbSet<RoleStatus> RoleStatus { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserStatus> UserStatus { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=103.21.149.106;Database=MasterDB;User ID=VErpAdmin;Password=VerpDev123$#1;MultipleActiveResultSets=true");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity<Action>(entity =>
            {
                entity.Property(e => e.ActionId).ValueGeneratedNever();

                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasMaxLength(16);
            });

            modelBuilder.Entity<ApiEndpoint>(entity =>
            {
                entity.Property(e => e.ApiEndpointId).ValueGeneratedNever();

                entity.Property(e => e.Route)
                    .IsRequired()
                    .HasMaxLength(512);
            });

            modelBuilder.Entity<Config>(entity =>
            {
                entity.Property(e => e.ConfigId).ValueGeneratedNever();

                entity.Property(e => e.ConfigName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(512);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.EmployeeCode)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<Method>(entity =>
            {
                entity.Property(e => e.MethodId).ValueGeneratedNever();

                entity.Property(e => e.MethodName)
                    .IsRequired()
                    .HasMaxLength(16);
            });

            modelBuilder.Entity<Module>(entity =>
            {
                entity.Property(e => e.ModuleId).ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(512);

                entity.Property(e => e.ModuleName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<ModuleApiEndpointMapping>(entity =>
            {
                entity.HasKey(e => new { e.ModuleId, e.ApiEndpointId });
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).ValueGeneratedNever();

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => new { e.RoleId, e.ModuleId });
            });

            modelBuilder.Entity<RoleStatus>(entity =>
            {
                entity.Property(e => e.RoleStatusId).ValueGeneratedNever();

                entity.Property(e => e.RoleStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PasswordSalt)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(64);
            });

            modelBuilder.Entity<UserStatus>(entity =>
            {
                entity.Property(e => e.UserStatusId).ValueGeneratedNever();

                entity.Property(e => e.UserStatusName)
                    .IsRequired()
                    .HasMaxLength(128);
            });
        }
    }
}
