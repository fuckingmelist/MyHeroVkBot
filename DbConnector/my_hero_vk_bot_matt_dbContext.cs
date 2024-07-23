using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MyHeroVkBot.Models;

namespace MyHeroVkBot.DbConnector
{
    public partial class my_hero_vk_bot_matt_dbContext : DbContext
    {
        public my_hero_vk_bot_matt_dbContext()
        {
        }

        public my_hero_vk_bot_matt_dbContext(DbContextOptions<my_hero_vk_bot_matt_dbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Cash> Cashes { get; set; } = null!;
        public virtual DbSet<Hero> Heroes { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserHero> UserHeroes { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=83.147.246.87:5432;Database=my_hero_vk_bot_matt_db;Username=my_hero_vk_bot_matt_user;Password=BEaTBEaT97");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cash>(entity =>
            {
                entity.HasKey(e => e.Key)
                    .HasName("cash_pk");

                entity.ToTable("cash");

                entity.Property(e => e.Key)
                    .HasMaxLength(255)
                    .HasColumnName("key");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.Property(e => e.Value).HasColumnName("value");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Cashes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("cash_user_id_fk");
            });

            modelBuilder.Entity<Hero>(entity =>
            {
                entity.ToTable("hero");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Accepted).HasColumnName("accepted");

                entity.Property(e => e.AudioId).HasColumnName("audio_id");

                entity.Property(e => e.CheckingUserId).HasColumnName("checkingUserId");

                entity.Property(e => e.PhotoId).HasColumnName("photo_id");

                entity.Property(e => e.Text)
                    .HasMaxLength(90000)
                    .HasColumnName("text");

                entity.Property(e => e.VideoId).HasColumnName("video_id");

                entity.HasOne(d => d.CheckingUser)
                    .WithMany(p => p.Heroes)
                    .HasForeignKey(d => d.CheckingUserId)
                    .HasConstraintName("hero_user_id_fk");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.IsAdmin).HasColumnName("isAdmin");

                entity.Property(e => e.State)
                    .HasMaxLength(255)
                    .HasColumnName("state");
            });

            modelBuilder.Entity<UserHero>(entity =>
            {
                entity.ToTable("user_hero");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.HeroId).HasColumnName("hero_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Hero)
                    .WithMany(p => p.UserHeroes)
                    .HasForeignKey(d => d.HeroId)
                    .HasConstraintName("user_hero_hero_id_fk");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserHeroes)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_hero_user_id_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
