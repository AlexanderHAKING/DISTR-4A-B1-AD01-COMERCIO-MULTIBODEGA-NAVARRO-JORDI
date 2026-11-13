using System.ComponentModel.DataAnnotations;
namespace Pedidos.Api.Models;
public class Cliente
{
    [Key] public int IdCliente { get; set; }
    [Required, MaxLength(150)] public string Nombres { get; set; } = string.Empty;
    [Required, StringLength(10, MinimumLength = 10)] public string Cedula { get; set; } = string.Empty;
    [Required, EmailAddress, MaxLength(150)] public string Correo { get; set; } = string.Empty;
}
