using System;
using System.Collections.Generic;
using FinTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Data;

public partial class FinTrackDbContext : DbContext
{
    public FinTrackDbContext(DbContextOptions<FinTrackDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Banco> Bancos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Divida> Dividas { get; set; }

    public virtual DbSet<Recorrente> Recorrentes { get; set; }

    public virtual DbSet<Transacao> Transacoes { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Banco>(entity =>
        {
            entity.ToTable("Banco");

            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            // Obrigatório: o DbSet chama-se Categorias, mas a tabela é Categoria.
            entity.ToTable("Categoria");

            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Categorias)
                .HasForeignKey(d => d.IdUser)
                .HasConstraintName("FK_Categoria_Usuario");
        });

        modelBuilder.Entity<Divida>(entity =>
        {
            // Obrigatório: o DbSet chama-se Dividas, mas a tabela é Divida.
            entity.ToTable("Divida");

            entity.Property(e => e.Descricao)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.TaxaJuro).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ValorParcela).HasColumnType("money");
            entity.Property(e => e.ValorTotal).HasColumnType("money");
            entity.Property(e => e.ValorVista).HasColumnType("money");

            entity.HasOne(d => d.IdBancoNavigation).WithMany(p => p.Dividas)
                .HasForeignKey(d => d.IdBanco)
                .HasConstraintName("FK_Divida_Banco");

            entity.HasOne(d => d.IdRecorrenteNavigation).WithMany(p => p.Dividas)
                .HasForeignKey(d => d.IdRecorrente)
                .HasConstraintName("FK_Divida_Recorrente");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Dividas)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Divida_Usuario");
        });

        modelBuilder.Entity<Recorrente>(entity =>
        {
            entity.ToTable("Recorrente");

            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.Descricao)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TipoVencimento).HasDefaultValue(1);
            entity.Property(e => e.Valor).HasColumnType("money");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Recorrentes)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recorrente_Categoria");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Recorrentes)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Recorrente_Usuario");
        });

        modelBuilder.Entity<Transacao>(entity =>
        {
            entity.ToTable("Transacao");

            entity.Property(e => e.Data)
                .HasColumnType("datetime")
                .HasColumnName("data");
            entity.Property(e => e.Desc).IsUnicode(false);
            entity.Property(e => e.Valor)
                .HasColumnType("money")
                .HasColumnName("valor");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Transacoes)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transacao_Categoria");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Transacoes)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transacao_Usuario");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");

            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Senha)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
