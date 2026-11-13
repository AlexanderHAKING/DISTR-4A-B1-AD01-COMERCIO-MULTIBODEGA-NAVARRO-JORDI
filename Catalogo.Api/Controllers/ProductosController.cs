using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly CatalogoContext _db;
    public ProductosController(CatalogoContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Productos.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var producto = await _db.Productos.FindAsync(id);
        if (producto == null) return NotFound("Producto no encontrado.");
        return Ok(producto);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Producto dato)
    {
        if (dato.Precio <= 0 || string.IsNullOrWhiteSpace(dato.Nombre) || string.IsNullOrWhiteSpace(dato.SKU))
            return BadRequest("Nombre, SKU y precio mayor a cero son obligatorios.");
        if (!await _db.Categorias.AnyAsync(c => c.IdCategoria == dato.IdCategoria))
            return BadRequest("La categoría no existe.");
        if (!await _db.Marcas.AnyAsync(m => m.IdMarca == dato.IdMarca))
            return BadRequest("La marca no existe.");
        if (await _db.Productos.AnyAsync(p => p.SKU == dato.SKU))
            return BadRequest("El SKU ya está registrado.");
        _db.Productos.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdProducto }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, Producto dato)
    {
        if (id != dato.IdProducto) return BadRequest("El ID no coincide.");
        var actual = await _db.Productos.FindAsync(id);
        if (actual == null) return NotFound("Producto no encontrado.");
        if (dato.Precio <= 0 || string.IsNullOrWhiteSpace(dato.Nombre) || string.IsNullOrWhiteSpace(dato.SKU))
            return BadRequest("Nombre, SKU y precio mayor a cero son obligatorios.");
        if (!await _db.Categorias.AnyAsync(c => c.IdCategoria == dato.IdCategoria) ||
            !await _db.Marcas.AnyAsync(m => m.IdMarca == dato.IdMarca))
            return BadRequest("La categoría o la marca no existe.");
        if (await _db.Productos.AnyAsync(p => p.SKU == dato.SKU && p.IdProducto != id))
            return BadRequest("El SKU ya está registrado.");
        actual.IdCategoria = dato.IdCategoria;
        actual.IdMarca = dato.IdMarca;
        actual.Nombre = dato.Nombre;
        actual.SKU = dato.SKU;
        actual.Precio = dato.Precio;
        actual.Estado = dato.Estado;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Productos.FindAsync(id);
        if (dato == null) return NotFound("Producto no encontrado.");
        // Las referencias desde otras bases de datos son lógicas, no FK de SQL Server.
        _db.Productos.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
