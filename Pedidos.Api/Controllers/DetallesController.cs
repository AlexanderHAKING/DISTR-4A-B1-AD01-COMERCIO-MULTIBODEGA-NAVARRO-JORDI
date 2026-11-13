using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DetallesController : ControllerBase
{
    private readonly PedidosContext _db;
    private readonly ConsultaProducto _catalogo;
    public DetallesController(PedidosContext db, ConsultaProducto catalogo)
    {
        _db = db;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Detalles.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var dato = await _db.Detalles.FindAsync(id);
        if (dato == null) return NotFound("Detalle no encontrado.");
        return Ok(dato);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(DetallePedido dato)
    {
        if (dato.Cantidad <= 0 || dato.PrecioUnitario <= 0)
            return BadRequest("Cantidad y precio deben ser mayores a cero.");
        var pedido = await _db.Pedidos.FindAsync(dato.IdPedido);
        if (pedido == null) return BadRequest("El pedido no existe.");
        if (pedido.Estado != "PENDIENTE") return BadRequest("Solo se agregan detalles a pedidos pendientes.");
        var existe = await _catalogo.Existe(dato.IdProducto);
        if (existe == null) return StatusCode(503, "Catalogo.Api no está disponible.");
        if (existe == false) return BadRequest("El producto no existe en el catálogo.");
        _db.Detalles.Add(dato);
        pedido.Total += dato.Cantidad * dato.PrecioUnitario;
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdDetalle }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, DetallePedido dato)
    {
        if (id != dato.IdDetalle) return BadRequest("El ID no coincide.");
        var actual = await _db.Detalles.FindAsync(id);
        if (actual == null) return NotFound("Detalle no encontrado.");
        if (actual.IdPedido != dato.IdPedido) return BadRequest("No puede cambiar el pedido del detalle.");
        if (dato.Cantidad <= 0 || dato.PrecioUnitario <= 0)
            return BadRequest("Cantidad y precio deben ser mayores a cero.");
        var pedido = await _db.Pedidos.FindAsync(actual.IdPedido);
        if (pedido == null) return BadRequest("El pedido ya no existe.");
        if (pedido.Estado != "PENDIENTE") return BadRequest("Solo se modifican pedidos pendientes.");
        var existe = await _catalogo.Existe(dato.IdProducto);
        if (existe == null) return StatusCode(503, "Catalogo.Api no está disponible.");
        if (existe == false) return BadRequest("El producto no existe en el catálogo.");
        pedido.Total -= actual.Cantidad * actual.PrecioUnitario;
        actual.IdProducto = dato.IdProducto;
        actual.Cantidad = dato.Cantidad;
        actual.PrecioUnitario = dato.PrecioUnitario;
        pedido.Total += actual.Cantidad * actual.PrecioUnitario;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Detalles.FindAsync(id);
        if (dato == null) return NotFound("Detalle no encontrado.");
        var pedido = await _db.Pedidos.FindAsync(dato.IdPedido);
        if (pedido == null) return BadRequest("El pedido ya no existe.");
        if (pedido.Estado != "PENDIENTE") return BadRequest("Solo se modifican pedidos pendientes.");
        pedido.Total -= dato.Cantidad * dato.PrecioUnitario;
        _db.Detalles.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
