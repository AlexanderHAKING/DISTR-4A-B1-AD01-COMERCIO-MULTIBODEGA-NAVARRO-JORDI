using Inventario.Api.Data;
using Inventario.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExistenciasController : ControllerBase
{
    private readonly InventarioContext _db;
    private readonly ConsultaCatalogo _catalogo;
    public ExistenciasController(InventarioContext db, ConsultaCatalogo catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Existencias.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var dato = await _db.Existencias.FindAsync(id);
        if (dato == null) return NotFound("Existencia no encontrada.");
        return Ok(dato);
    }

    [HttpGet("disponibles/{idProducto}")]
    public async Task<IActionResult> Disponibles(int idProducto)
    {
        var existencias = await _db.Existencias.Where(e => e.IdProducto == idProducto).ToListAsync();
        var resultado = new List<object>();
        foreach (var e in existencias)
        {
            int reservado = await _db.Reservas.Where(r => r.IdProducto == idProducto &&
                r.IdBodega == e.IdBodega && r.Estado == "ACTIVA").SumAsync(r => r.Cantidad);
            resultado.Add(new { e.IdBodega, e.Stock, Reservado = reservado,
                Disponible = e.Stock - reservado });
        }
        return Ok(resultado);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Existencia dato)
    {
        if (dato.Stock < 0) return BadRequest("El stock no puede ser negativo.");
        if (!await _db.Bodegas.AnyAsync(b => b.IdBodega == dato.IdBodega))
            return BadRequest("La bodega no existe.");
        var existe = await _catalogo.ExisteProducto(dato.IdProducto);
        if (existe == null) return StatusCode(503, "Catalogo.Api no está disponible.");
        if (existe == false) return BadRequest("El producto no existe en el catálogo.");
        if (await _db.Existencias.AnyAsync(e => e.IdProducto == dato.IdProducto && e.IdBodega == dato.IdBodega))
            return BadRequest("Ya existe stock para este producto en esta bodega.");
        _db.Existencias.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdExistencia }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, Existencia dato)
    {
        if (id != dato.IdExistencia) return BadRequest("El ID no coincide.");
        var actual = await _db.Existencias.FindAsync(id);
        if (actual == null) return NotFound("Existencia no encontrada.");
        if (actual.IdProducto != dato.IdProducto || actual.IdBodega != dato.IdBodega)
            return BadRequest("No se puede cambiar el producto o bodega de una existencia.");
        if (dato.Stock < 0) return BadRequest("El stock no puede ser negativo.");
        var reservado = await _db.Reservas.Where(r => r.IdProducto == dato.IdProducto &&
            r.IdBodega == dato.IdBodega && r.Estado == "ACTIVA").SumAsync(r => r.Cantidad);
        if (dato.Stock < reservado) return BadRequest("El stock no puede quedar por debajo de lo reservado.");
        actual.Stock = dato.Stock;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Existencias.FindAsync(id);
        if (dato == null) return NotFound("Existencia no encontrada.");
        if (await _db.Reservas.AnyAsync(r => r.IdProducto == dato.IdProducto &&
            r.IdBodega == dato.IdBodega && r.Estado == "ACTIVA"))
            return BadRequest("Primero cancele las reservas activas.");
        if (await _db.Movimientos.AnyAsync(m => m.IdProducto == dato.IdProducto && m.IdBodega == dato.IdBodega))
            return BadRequest("Hay movimientos registrados. No se debe borrar su existencia.");
        _db.Existencias.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
