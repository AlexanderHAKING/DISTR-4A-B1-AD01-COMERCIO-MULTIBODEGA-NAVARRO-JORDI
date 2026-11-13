using Inventario.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Data;

public class InventarioContext : DbContext
{
    public InventarioContext(DbContextOptions<InventarioContext> options) : base(options) { }

    public DbSet<Bodega> Bodegas { get; set; } = null!;
    public DbSet<Existencia> Existencias { get; set; } = null!;
    public DbSet<MovimientoInventario> Movimientos { get; set; } = null!;
    public DbSet<ReservaStock> Reservas { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bodega>().ToTable("Bodega");
        modelBuilder.Entity<Existencia>().ToTable("Existencia");
        modelBuilder.Entity<MovimientoInventario>().ToTable("MovimientoInventario");
        modelBuilder.Entity<ReservaStock>().ToTable("ReservaStock");
        modelBuilder.Entity<Existencia>().HasIndex(e => new { e.IdProducto, e.IdBodega }).IsUnique();
        modelBuilder.Entity<Existencia>().HasOne<Bodega>()
            .WithMany().HasForeignKey(e => e.IdBodega).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<MovimientoInventario>().HasOne<Bodega>()
            .WithMany().HasForeignKey(m => m.IdBodega).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<ReservaStock>().HasOne<Bodega>()
            .WithMany().HasForeignKey(r => r.IdBodega).OnDelete(DeleteBehavior.Restrict);
    }
}
