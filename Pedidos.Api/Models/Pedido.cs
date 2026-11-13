using System.ComponentModel.DataAnnotations;
namespace Pedidos.Api.Models;
public class Pedido
{
    [Key] public int IdPedido { get; set; }
    public int IdCliente { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Total { get; set; }
    [Required] public string Estado { get; set; } = "PENDIENTE";
}
