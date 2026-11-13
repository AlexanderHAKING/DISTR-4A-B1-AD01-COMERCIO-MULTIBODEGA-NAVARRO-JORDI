using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidosContext _db;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    public PedidosController(PedidosContext db, IHttpClientFactory http, IConfiguration config)
    {
        _db = db;
        _http = http;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos() => Ok(await _db.Pedidos.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUno(int id)
    {
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido == null) return NotFound("Pedido no encontrado.");
        var detalles = await _db.Detalles.Where(d => d.IdPedido == id).ToListAsync();
        return Ok(new { Pedido = pedido, Detalles = detalles });
    }

    // Consulta simple entre Pedidos.Api e Inventario.Api: no reserva ni descuenta stock.
    [HttpGet("{id}/disponibilidad")]
    public async Task<IActionResult> ConsultarDisponibilidad(int id)
    {
        if (!await _db.Pedidos.AnyAsync(p => p.IdPedido == id))
            return NotFound("Pedido no encontrado.");
        var detalles = await _db.Detalles.Where(d => d.IdPedido == id).ToListAsync();
        var productos = detalles.GroupBy(d => d.IdProducto);
        var resultado = new List<object>();
        foreach (var producto in productos)
        {
            int solicitado = producto.Sum(d => d.Cantidad);
            var url = $"{_config["Servicios:InventarioUrl"]}/api/Existencias/disponibles/{producto.Key}";
            try
            {
                var respuesta = await _http.CreateClient().GetAsync(url);
                if (!respuesta.IsSuccessStatusCode)
                    return StatusCode(503, "Inventario.Api no está disponible.");
                var bodegas = await respuesta.Content.ReadFromJsonAsync<List<StockPorBodega>>();
                int disponible = bodegas?.Sum(b => b.Disponible) ?? 0;
                resultado.Add(new { IdProducto = producto.Key, Solicitado = solicitado,
                    Disponible = disponible, Suficiente = disponible >= solicitado });
            }
            catch (HttpRequestException) { return StatusCode(503, "Inventario.Api no está disponible."); }
            catch (TaskCanceledException) { return StatusCode(503, "Inventario.Api no respondió."); }
        }
        return Ok(resultado);
    }

    public class StockPorBodega
    {
        public int Disponible { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Pedido dato)
    {
        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == dato.IdCliente))
            return BadRequest("El cliente no existe.");
        dato.Estado = "PENDIENTE";
        dato.Fecha = DateTime.Now;
        dato.Total = 0;
        _db.Pedidos.Add(dato);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUno), new { id = dato.IdPedido }, dato);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, Pedido dato)
    {
        if (id != dato.IdPedido) return BadRequest("El ID no coincide.");
        var actual = await _db.Pedidos.FindAsync(id);
        if (actual == null) return NotFound("Pedido no encontrado.");
        if (!await _db.Clientes.AnyAsync(c => c.IdCliente == dato.IdCliente))
            return BadRequest("El cliente no existe.");
        var estado = (dato.Estado ?? "").Trim().ToUpperInvariant();
        if (estado != "PENDIENTE" && estado != "CONFIRMADO" && estado != "CANCELADO")
            return BadRequest("Estados: PENDIENTE, CONFIRMADO o CANCELADO.");
        actual.IdCliente = dato.IdCliente;
        actual.Estado = estado;
        // El total y la fecha se administran desde el sistema, no se editan manualmente.
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var dato = await _db.Pedidos.FindAsync(id);
        if (dato == null) return NotFound("Pedido no encontrado.");
        if (await _db.Detalles.AnyAsync(d => d.IdPedido == id))
            return BadRequest("Primero elimine los detalles de este pedido.");
        _db.Pedidos.Remove(dato);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
