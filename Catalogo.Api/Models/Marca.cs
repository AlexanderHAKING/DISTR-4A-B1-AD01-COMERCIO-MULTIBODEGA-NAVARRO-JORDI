using System.ComponentModel.DataAnnotations;
namespace Catalogo.Api.Models;
public class Marca
{
    [Key] public int IdMarca { get; set; }
    [Required, MaxLength(100)] public string Nombre { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string PaisOrigen { get; set; } = string.Empty;
}
