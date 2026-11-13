using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly InventarioContext _db;
    public ReservasController(InventarioContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Reservas.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var dato = await _db.Reservas.FindAsync(id);
        if (dato == null) return NotFound("Reserva no encontrada.");
        return Ok(dato);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(ReservaStock dato)
    {
        if (dato.Cantidad <= 0) return BadRequest("La cantidad debe ser positiva.");
        if (dato.Estado.Trim().ToUpperInvariant() != "ACTIVA")
            return BadRequest("La nueva reserva debe iniciar en estado ACTIVA.");
        var existencia = await _db.Existencias.FirstOrDefaultAsync(e => e.IdProducto == dato.IdProducto && e.IdBodega == dato.IdBodega);
        if (existencia == null) return BadRequest("No existe este producto en la bodega.");
        int reservado = await _db.Reservas.Where(r => r.IdProducto == dato.IdProducto &&
            r.IdBodega == dato.IdBodega && r.Estado == "ACTIVA").SumAsync(r => r.Cantidad);
        if (existencia.Stock - reservado < dato.Cantidad) return BadRequest("Stock disponible insuficiente.");
        dato.Estado = "ACTIVA";
        _db.Reservas.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdReserva }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, ReservaStock dato)
    {
        if (id != dato.IdReserva) return BadRequest("El ID no coincide.");
        var actual = await _db.Reservas.FindAsync(id);
        if (actual == null) return NotFound("Reserva no encontrada.");
        if (actual.IdProducto != dato.IdProducto || actual.IdBodega != dato.IdBodega)
            return BadRequest("No se puede cambiar de producto ni de bodega.");
        if (dato.Cantidad <= 0) return BadRequest("La cantidad debe ser positiva.");
        var estado = (dato.Estado ?? "").Trim().ToUpperInvariant();
        if (estado != "ACTIVA" && estado != "CANCELADA")
            return BadRequest("Estado permitido: ACTIVA o CANCELADA.");
        var existencia = await _db.Existencias.FirstOrDefaultAsync(e => e.IdProducto == dato.IdProducto && e.IdBodega == dato.IdBodega);
        if (estado == "ACTIVA")
        {
            if (existencia == null) return BadRequest("No existe stock en la bodega.");
            int otrasReservas = await _db.Reservas.Where(r => r.IdReserva != id && r.IdProducto == dato.IdProducto &&
                r.IdBodega == dato.IdBodega && r.Estado == "ACTIVA").SumAsync(r => r.Cantidad);
            if (existencia.Stock - otrasReservas < dato.Cantidad) return BadRequest("Stock disponible insuficiente.");
        }
        actual.Cantidad = dato.Cantidad;
        actual.Estado = estado;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Reservas.FindAsync(id);
        if (dato == null) return NotFound("Reserva no encontrada.");
        _db.Reservas.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
