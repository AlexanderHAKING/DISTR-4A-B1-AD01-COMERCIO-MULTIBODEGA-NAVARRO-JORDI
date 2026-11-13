using Pedidos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Api.Data;

public class PedidosContext : DbContext
{
    public PedidosContext(DbContextOptions<PedidosContext> options) : base(options) { }

    public DbSet<Cliente> Clientes { get; set; } = null!;
    public DbSet<Pedido> Pedidos { get; set; } = null!;
    public DbSet<DetallePedido> Detalles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>().ToTable("Cliente");
        modelBuilder.Entity<Pedido>().ToTable("Pedido");
        modelBuilder.Entity<DetallePedido>().ToTable("DetallePedido");
        modelBuilder.Entity<Cliente>().HasIndex(c => c.Cedula).IsUnique();
        modelBuilder.Entity<Pedido>().Property(p => p.Total).HasPrecision(18, 2);
        modelBuilder.Entity<DetallePedido>().Property(d => d.PrecioUnitario).HasPrecision(18, 2);
        modelBuilder.Entity<Pedido>().HasOne<Cliente>()
            .WithMany().HasForeignKey(p => p.IdCliente).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<DetallePedido>().HasOne<Pedido>()
            .WithMany().HasForeignKey(d => d.IdPedido).OnDelete(DeleteBehavior.Restrict);
    }
}
