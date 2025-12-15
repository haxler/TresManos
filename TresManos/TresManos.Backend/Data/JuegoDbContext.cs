using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using TresManos.Shared.Entities;

namespace TresManos.Backend.Data;

public class JuegoDbContext : DbContext
{
    public JuegoDbContext(DbContextOptions<JuegoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Partida> Partidas { get; set; }
    public DbSet<Ronda> Rondas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasIndex(e => e.NombreUsuario).IsUnique();
            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("GETDATE()");
        });

        // Configuración de Partida
        modelBuilder.Entity<Partida>(entity =>
        {
            entity.Property(e => e.FechaInicio).HasDefaultValueSql("GETDATE()");
            entity.Property(e => e.EsRevancha).HasDefaultValue(false);

            // Relación Jugador1
            entity.HasOne(p => p.Jugador1)
                .WithMany(u => u.PartidasComoJugador1)
                .HasForeignKey(p => p.UsuarioId_Jugador1)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Jugador2
            entity.HasOne(p => p.Jugador2)
                .WithMany(u => u.PartidasComoJugador2)
                .HasForeignKey(p => p.UsuarioId_Jugador2)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Ganador
            entity.HasOne(p => p.Ganador)
                .WithMany(u => u.PartidasGanadas)
                .HasForeignKey(p => p.GanadorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Perdedor
            entity.HasOne(p => p.Perdedor)
                .WithMany(u => u.PartidasPerdidas)
                .HasForeignKey(p => p.PerdedorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Auto-relación (Revancha)
            entity.HasOne(p => p.PartidaOriginal)
                .WithMany(p => p.Revanchas)
                .HasForeignKey(p => p.PartidaOriginalId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuración de Ronda
        modelBuilder.Entity<Ronda>(entity =>
        {
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETDATE()");

            // Relación con Partida
            entity.HasOne(r => r.Partida)
                .WithMany(p => p.Rondas)
                .HasForeignKey(r => r.PartidaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Ganador
            entity.HasOne(r => r.GanadorUsuario)
                .WithMany(u => u.RondasGanadas)
                .HasForeignKey(r => r.GanadorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Perdedor
            entity.HasOne(r => r.PerdedorUsuario)
                .WithMany(u => u.RondasPerdidas)
                .HasForeignKey(r => r.PerdedorUsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}