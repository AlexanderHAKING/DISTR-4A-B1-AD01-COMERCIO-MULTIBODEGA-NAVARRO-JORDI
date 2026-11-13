using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly PedidosContext _db;
    public ClientesController(PedidosContext db) { _db = db; }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Clientes.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var dato = await _db.Clientes.FindAsync(id);
        if (dato == null) return NotFound("Cliente no encontrado.");
        return Ok(dato);
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Cliente dato)
    {
        if (await _db.Clientes.AnyAsync(c => c.Cedula == dato.Cedula)) return BadRequest("La cédula ya está registrada.");
        _db.Clientes.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdCliente }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, Cliente dato)
    {
        if (id != dato.IdCliente) return BadRequest("El ID no coincide.");
        var actual = await _db.Clientes.FindAsync(id);
        if (actual == null) return NotFound("Cliente no encontrado.");
        if (await _db.Clientes.AnyAsync(c => c.Cedula == dato.Cedula && c.IdCliente != id)) return BadRequest("La cédula ya está registrada.");
        actual.Nombres = dato.Nombres;
        actual.Cedula = dato.Cedula;
        actual.Correo = dato.Correo;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Clientes.FindAsync(id);
        if (dato == null) return NotFound("Cliente no encontrado.");
        if (await _db.Pedidos.AnyAsync(p => p.IdCliente == id)) return BadRequest("El cliente tiene pedidos asociados.");
        _db.Clientes.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
