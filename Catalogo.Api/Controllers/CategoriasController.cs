using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly CatalogoContext _db;
    public CategoriasController(CatalogoContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Categorias.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var dato = await _db.Categorias.FindAsync(id);
        if (dato == null) return NotFound("Categoria no encontrado.");
        return Ok(dato);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Categoria dato)
    {
        if (string.IsNullOrWhiteSpace(dato.Nombre)) return BadRequest("El nombre es obligatorio.");
        _db.Categorias.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdCategoria }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, Categoria dato)
    {
        if (id != dato.IdCategoria) return BadRequest("El ID no coincide.");
        var actual = await _db.Categorias.FindAsync(id);
        if (actual == null) return NotFound("Categoria no encontrado.");
        if (string.IsNullOrWhiteSpace(dato.Nombre)) return BadRequest("El nombre es obligatorio.");
        actual.Nombre = dato.Nombre;
        actual.Descripcion = dato.Descripcion;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Categorias.FindAsync(id);
        if (dato == null) return NotFound("Categoria no encontrado.");
        if (await _db.Productos.AnyAsync(p => p.IdCategoria == id)) return BadRequest("La categoría tiene productos asociados.");
        _db.Categorias.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
