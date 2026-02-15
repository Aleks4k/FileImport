using FileImport.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FileImport.Domain.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}
        public DbSet<AuthorizedUser> AuthorizedUsers { get; set; }
        public DbSet<CheckedFile> CheckedFiles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthorizedUser>(entity =>
            {
                entity.ToTable("AuthorizedUser");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int")
                    .IsRequired()
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasColumnType("nvarchar(255)")
                    .HasMaxLength(255)
                    .IsUnicode(true);
            });
            modelBuilder.Entity<CheckedFile>(entity =>
            {
                entity.ToTable("CheckedFile");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.DocumentNumber);
                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasColumnType("int")
                    .IsRequired()
                    .ValueGeneratedOnAdd();
                entity.Property(e => e.DocumentNumber)
                    .HasColumnName("DocumentNumber")
                    .HasColumnType("varchar(6)")
                    .HasMaxLength(6)
                    .IsRequired();
                entity.Property(e => e.AuthorizedUserId)
                    .HasColumnName("authorized_user_id")
                    .HasColumnType("int")
                    .IsRequired();
                entity.Property(e => e.FilePath)
                    .HasColumnName("FilePath")
                    .HasColumnType("nvarchar(2048)")
                    .HasMaxLength(2048)
                    .IsRequired();
                entity.HasOne(d => d.AuthorizedUser)
                    .WithMany(p => p.CheckedFiles)
                    .HasForeignKey(d => d.AuthorizedUserId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.ToTable("CheckedFile", t =>
                {
                    t.HasCheckConstraint(
                        "chk_document_number",
                        "[DocumentNumber] like '[0-9][0-9][0-9][0-9][0-9][0-9]'"
                    );
                });
            });
        }
    }
}
