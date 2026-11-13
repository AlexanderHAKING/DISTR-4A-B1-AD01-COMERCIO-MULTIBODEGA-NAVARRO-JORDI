using Catalogo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Data;

public class CatalogoContext : DbContext
{
    public CatalogoContext(DbContextOptions<CatalogoContext> options) : base(options) { }

    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Marca> Marcas { get; set; } = null!;
    public DbSet<Producto> Productos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>().ToTable("Categoria");
        modelBuilder.Entity<Marca>().ToTable("Marca");
        modelBuilder.Entity<Producto>().ToTable("Producto");
        modelBuilder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);
        modelBuilder.Entity<Producto>().HasIndex(p => p.SKU).IsUnique();
        modelBuilder.Entity<Producto>().HasOne<Categoria>()
            .WithMany().HasForeignKey(p => p.IdCategoria).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Producto>().HasOne<Marca>()
            .WithMany().HasForeignKey(p => p.IdMarca).OnDelete(DeleteBehavior.Restrict);
    }
}
