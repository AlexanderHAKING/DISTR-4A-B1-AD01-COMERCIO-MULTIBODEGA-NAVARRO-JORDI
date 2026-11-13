using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasController : ControllerBase
{
    private readonly CatalogoContext _db;
    public MarcasController(CatalogoContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Marcas.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var dato = await _db.Marcas.FindAsync(id);
        if (dato == null) return NotFound("Marca no encontrado.");
        return Ok(dato);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Marca dato)
    {
        if (string.IsNullOrWhiteSpace(dato.Nombre) || string.IsNullOrWhiteSpace(dato.PaisOrigen)) return BadRequest("Nombre y país son obligatorios.");
        _db.Marcas.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdMarca }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, Marca dato)
    {
        if (id != dato.IdMarca) return BadRequest("El ID no coincide.");
        var actual = await _db.Marcas.FindAsync(id);
        if (actual == null) return NotFound("Marca no encontrado.");
        if (string.IsNullOrWhiteSpace(dato.Nombre) || string.IsNullOrWhiteSpace(dato.PaisOrigen)) return BadRequest("Nombre y país son obligatorios.");
        actual.Nombre = dato.Nombre;
        actual.PaisOrigen = dato.PaisOrigen;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Marcas.FindAsync(id);
        if (dato == null) return NotFound("Marca no encontrado.");
        if (await _db.Productos.AnyAsync(p => p.IdMarca == id)) return BadRequest("La marca tiene productos asociados.");
        _db.Marcas.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
