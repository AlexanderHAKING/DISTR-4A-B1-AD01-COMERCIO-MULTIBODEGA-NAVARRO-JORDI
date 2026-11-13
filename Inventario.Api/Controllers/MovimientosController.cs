using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly InventarioContext _db;
    private readonly ConsultaCatalogo _catalogo;
    public MovimientosController(InventarioContext db, ConsultaCatalogo catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Movimientos.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var dato = await _db.Movimientos.FindAsync(id);
        if (dato == null) return NotFound("Movimiento no encontrado.");
        return Ok(dato);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(MovimientoInventario dato)
    {
        dato.Tipo = (dato.Tipo ?? "").Trim().ToUpperInvariant();
        if (dato.Tipo != "ENTRADA" && dato.Tipo != "SALIDA")
            return BadRequest("El tipo debe ser ENTRADA o SALIDA.");
        if (dato.Cantidad <= 0) return BadRequest("La cantidad debe ser mayor a cero.");
        if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == dato.IdBodega))
            return BadRequest("La bodega no existe.");
        var existeProducto = await _catalogo.ExisteProducto(dato.IdProducto);
        if (existeProducto == null) return StatusCode(503, "Catalogo.Api no está disponible.");
        if (existeProducto == false) return BadRequest("El producto no existe en el catálogo.");

        var existencia = await _db.Existencias.FirstOrDefaultAsync(e => e.IdProducto == dato.IdProducto &&
            e.IdBodega == dato.IdBodega);
        if (existencia == null)
        {
            if (dato.Tipo == "SALIDA") return BadRequest("No existe stock para realizar una salida.");
            existencia = new Existencia { IdProducto = dato.IdProducto, IdBodega = dato.IdBodega, Stock = 0 };
            _db.Existencias.Add(existencia);
        }
        if (dato.Tipo == "ENTRADA")
        {
            existencia.Stock += dato.Cantidad;
        }
        else
        {
            int reservado = await _db.Reservas.Where(r => r.IdProducto == dato.IdProducto &&
                r.IdBodega == dato.IdBodega && r.Estado == "ACTIVA").SumAsync(r => r.Cantidad);
            if (existencia.Stock - reservado < dato.Cantidad)
                return BadRequest("Stock disponible insuficiente, considerando reservas.");
            existencia.Stock -= dato.Cantidad;
        }

        dato.Fecha = DateTime.Now;
        _db.Movimientos.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdMovimiento }, dato);
    }
    // Los movimientos son históricos: se consultan y crean, pero no se editan ni borran.
}
