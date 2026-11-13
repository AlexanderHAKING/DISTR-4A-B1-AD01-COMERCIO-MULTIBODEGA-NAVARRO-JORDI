
using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BodegasController : ControllerBase
{
    private readonly InventarioContext _db;
    public BodegasController(InventarioContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Bodegas.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var dato = await _db.Bodegas.FindAsync(id);
        if (dato == null) return NotFound("Bodega no encontrado.");
        return Ok(dato);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Bodega dato)
    {
        if (string.IsNullOrWhiteSpace(dato.Nombre) || string.IsNullOrWhiteSpace(dato.Ciudad) || string.IsNullOrWhiteSpace(dato.Direccion)) return BadRequest("Complete los datos de la bodega.");
        _db.Bodegas.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdBodega }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, Bodega dato)
    {
        if (id != dato.IdBodega) return BadRequest("El ID no coincide.");
        var actual = await _db.Bodegas.FindAsync(id);
        if (actual == null) return NotFound("Bodega no encontrado.");
        if (string.IsNullOrWhiteSpace(dato.Nombre) || string.IsNullOrWhiteSpace(dato.Ciudad) || string.IsNullOrWhiteSpace(dato.Direccion)) return BadRequest("Complete los datos de la bodega.");
        actual.Nombre = dato.Nombre;
        actual.Ciudad = dato.Ciudad;
        actual.Direccion = dato.Direccion;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Bodegas.FindAsync(id);
        if (dato == null) return NotFound("Bodega no encontrado.");
        if (await _db.Existencias.AnyAsync(x => x.IdBodega == id) ||
            await _db.Movimientos.AnyAsync(x => x.IdBodega == id) ||
            await _db.Reservas.AnyAsync(x => x.IdBodega == id))
            return BadRequest("La bodega tiene registros asociados.");
        _db.Bodegas.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
