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

    public virtual DbSet<PluggyConexao> PluggyConexoes { get; set; }

    public virtual DbSet<PluggyItem> PluggyItems { get; set; }

    public virtual DbSet<PluggyFatura> PluggyFaturas { get; set; }

    public virtual DbSet<PluggyInvestimento> PluggyInvestimentos { get; set; }

    public virtual DbSet<Orcamento> Orcamentos { get; set; }

    public virtual DbSet<MetaInvestimento> Metas { get; set; }

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
            entity.Property(e => e.EhMovimentacaoInterna).HasDefaultValue(false);

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
            entity.Property(e => e.PluggyTransactionId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PluggyCategoria)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PluggyCategoriaId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MerchantNome)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.MerchantCnpj)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ContaPluggyId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContaNome)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContaTipo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PluggyBillId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PluggyFaturaPrevista)
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.PluggyValorCompraTotal).HasColumnType("money");

            entity.HasIndex(e => e.PluggyTransactionId)
                .IsUnique()
                .HasFilter("[PluggyTransactionId] IS NOT NULL");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Transacoes)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transacao_Categoria");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Transacoes)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transacao_Usuario");
        });

        modelBuilder.Entity<PluggyConexao>(entity =>
        {
            entity.ToTable("PluggyConexao");

            entity.Property(e => e.ClientIdProtegido).IsUnicode(false);
            entity.Property(e => e.ClientSecretProtegido).IsUnicode(false);

            entity.HasIndex(e => e.IdUser).IsUnique();

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.PluggyConexao)
                .HasForeignKey<PluggyConexao>(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PluggyConexao_Usuario");
        });

        modelBuilder.Entity<PluggyItem>(entity =>
        {
            entity.ToTable("PluggyItem");

            entity.Property(e => e.ItemId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NomeConector)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IconeUrl)
                .HasMaxLength(500)
                .IsUnicode(false);

            entity.HasIndex(e => e.ItemId).IsUnique();

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.PluggyItems)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PluggyItem_Usuario");
        });

        modelBuilder.Entity<PluggyFatura>(entity =>
        {
            entity.ToTable("PluggyFatura");

            entity.Property(e => e.BillId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContaPluggyId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ContaNome)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ValorTotal).HasColumnType("money");
            entity.Property(e => e.ValorMinimo).HasColumnType("money");
            entity.Property(e => e.ValorPago).HasColumnType("money");
            entity.Ignore(e => e.StatusPagamento);

            entity.HasIndex(e => e.BillId).IsUnique();

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.PluggyFaturas)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PluggyFatura_Usuario");
        });

        modelBuilder.Entity<PluggyInvestimento>(entity =>
        {
            entity.ToTable("PluggyInvestimento");

            entity.Property(e => e.InvestmentId)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NomeConector)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Tipo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Subtipo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Nome)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Isin)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.MoedaCodigo)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.TipoTaxa)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Emissor)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Saldo).HasColumnType("money");
            entity.Property(e => e.ValorOriginal).HasColumnType("money");
            entity.Property(e => e.LucroInformado).HasColumnType("money");
            entity.Property(e => e.ValorDisponivelResgate).HasColumnType("money");
            // decimal comum (não money): valor de cota chega com até 6 casas
            // (ex.: 0.010333), que o scale 4 do money truncaria.
            entity.Property(e => e.Quantidade).HasPrecision(18, 6);
            entity.Property(e => e.ValorCota).HasPrecision(18, 6);
            entity.Property(e => e.Taxa).HasPrecision(9, 4);
            entity.Property(e => e.TaxaAnualFixa).HasPrecision(9, 4);

            entity.HasIndex(e => e.InvestmentId).IsUnique();

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.PluggyInvestimentos)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PluggyInvestimento_Usuario");
        });

        modelBuilder.Entity<Orcamento>(entity =>
        {
            entity.ToTable("Orcamento");

            entity.Property(e => e.Valor).HasColumnType("money");

            entity.HasIndex(e => new { e.IdUser, e.IdCategoria }).IsUnique();

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Orcamentos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orcamento_Categoria");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Orcamentos)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Orcamento_Usuario");
        });

        modelBuilder.Entity<MetaInvestimento>(entity =>
        {
            entity.ToTable("MetaInvestimento");

            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ValorAlvo).HasColumnType("money");
            entity.Property(e => e.TipoScope)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CriadoEm).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.Metas)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MetaInvestimento_Usuario");
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
